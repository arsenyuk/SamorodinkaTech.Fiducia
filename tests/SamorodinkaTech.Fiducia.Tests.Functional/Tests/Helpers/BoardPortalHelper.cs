using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для взаимодействия с Board Portal (порт 5002).
/// </summary>
public static class BoardPortalHelper
{
    private const int DefaultTimeout = 15_000;

    /// <summary>
    /// Навигация через меню Board Portal. Кликает по ссылке в левом меню.
    /// </summary>
    public static async Task NavigateToAsync(IPage page, string menuHref)
    {
        // Раскрыть sidebar если collapsed (Bootstrap collapse на десктопе)
        var toggler = await page.QuerySelectorAsync("button.navbar-toggler");
        if (toggler != null && await toggler.IsVisibleAsync())
        {
            await toggler.ClickAsync();
            await page.WaitForTimeoutAsync(300);
        }

        var link = page.Locator($"a[href='{menuHref}']");
        if (await link.CountAsync() > 0)
        {
            await link.First.ClickAsync();
        }
        else
        {
            await page.ClickAsync($"a[href='{menuHref}']");
        }
        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Заполнить основные поля ЮЛ на странице /legal-entities.
    /// </summary>
    public static async Task FillLegalEntityFieldsAsync(
        IPage page,
        string? shortName = null,
        string? ogrn = null)
    {
        await NavigateToAsync(page, "legal-entities");

        // Ждём загрузки данных ЮЛ (вкладка "Карточка ЮЛ" появляется только после загрузки)
        try
        {
            await page.WaitForFunctionAsync(
                @"() => {
                    const tabs = document.querySelectorAll('button.nav-link');
                    for (const tab of tabs) {
                        if (tab.textContent.includes('Карточка ЮЛ')) return true;
                    }
                    return false;
                }",
                null,
                new PageWaitForFunctionOptions { Timeout = 15_000 });
        }
        catch
        {
            // Проверяем, есть ли сообщение об ошибке
            var errorText = await page.EvaluateAsync<string>(
                "() => document.querySelector('.alert-danger')?.textContent ?? ''");
            if (!string.IsNullOrEmpty(errorText))
                throw new InvalidOperationException(
                    $"LegalEntities page error: {errorText}");
            throw new InvalidOperationException(
                "LegalEntities: вкладка 'Карточка ЮЛ' не появилась — данные ЮЛ не загружены.");
        }

        // Click on "Карточка ЮЛ" tab (tab 0)
        await page.EvaluateAsync(
            @"() => {
                const tabs = document.querySelectorAll('button.nav-link');
                for (const tab of tabs) {
                    if (tab.textContent.includes('Карточка ЮЛ')) { tab.click(); return; }
                }
            }");
        await page.WaitForTimeoutAsync(500);

        // Fill editable fields if they exist as inputs
        if (shortName is not null)
        {
            var shortNameInput = await page.EvaluateAsync<bool>(
                @"() => {
                    const inputs = document.querySelectorAll('input');
                    for (const input of inputs) {
                        const ph = (input.placeholder || '').toLowerCase();
                        if (ph.includes('краткое')) return true;
                    }
                    return false;
                }");
            shortNameInput.Should().BeTrue("Поле 'Краткое наименование' должно быть доступно");

            await page.EvaluateAsync(
                $@"() => {{
                    const inputs = document.querySelectorAll('input');
                    for (const input of inputs) {{
                        const ph = (input.placeholder || '').toLowerCase();
                        if (ph.includes('краткое')) {{
                            input.value = '{EscapeJs(shortName)}';
                            input.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            return;
                        }}
                    }}
                }}");
        }

        if (ogrn is not null)
        {
            var ogrnInput = await page.EvaluateAsync<bool>(
                @"() => {
                    const inputs = document.querySelectorAll('input');
                    for (const input of inputs) {
                        const ph = (input.placeholder || '').toLowerCase();
                        if (ph.includes('огрн')) return true;
                    }
                    return false;
                }");
            ogrnInput.Should().BeTrue("Поле 'ОГРН' должно быть доступно");

            await page.EvaluateAsync(
                $@"() => {{
                    const inputs = document.querySelectorAll('input');
                    for (const input of inputs) {{
                        const ph = (input.placeholder || '').toLowerCase();
                        if (ph.includes('огрн')) {{
                            input.value = '{EscapeJs(ogrn)}';
                            input.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            return;
                        }}
                    }}
                }}");
        }
    }

    /// <summary>
    /// Выбрать типовой устав (номер 01-36) на вкладке "Устав" страницы /legal-entities.
    /// </summary>
    public static async Task SelectStandardCharterAsync(IPage page, int charterNumber)
    {
        // Navigate to legal entities page if not already there
        if (!page.Url.Contains("/legal-entities"))
        {
            await NavigateToAsync(page, "legal-entities");
        }

        // Click on "Устав" tab (tab index 3, only for LLC)
        await page.EvaluateAsync(
            @"() => {
                const tabs = document.querySelectorAll('button.nav-link');
                for (const tab of tabs) {
                    if (tab.textContent.includes('Устав')) { tab.click(); return; }
                }
            }");
        await page.WaitForTimeoutAsync(500);

        // Select the charter number from dropdown
        var paddedNumber = charterNumber.ToString("D2");
        await page.EvaluateAsync(
            $@"() => {{
                const selects = document.querySelectorAll('select');
                for (const sel of selects) {{
                    const opts = sel.querySelectorAll('option');
                    if (opts.length >= 36) {{
                        sel.value = '{paddedNumber}';
                        sel.dispatchEvent(new Event('change', {{ bubbles: true }}));
                        return;
                    }}
                }}
            }}");
    }

    /// <summary>
    /// Нажать "Сохранить" и проверить отсутствие ошибок.
    /// </summary>
    public static async Task SaveAndVerifyAsync(IPage page)
    {
        // Click "Сохранить"
        await page.EvaluateAsync(
            @"() => {
                const buttons = document.querySelectorAll('button');
                for (const btn of buttons) {
                    if (btn.textContent.includes('Сохранить') && btn.classList.contains('btn-primary')) {
                        btn.click(); return;
                    }
                }
            }");

        // Wait for save to complete (spinner disappears)
        await page.WaitForFunctionAsync(
            @"() => document.querySelector('.spinner-border') === null ||
                     document.querySelector('.spinner-border')?.offsetParent === null",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Wait for success message or completion
        await page.WaitForTimeoutAsync(2000);

        // Verify no error alerts
        var hasError = await page.EvaluateAsync<bool>(
            "() => document.querySelectorAll('.alert-danger').length > 0");
        if (hasError)
        {
            var errorText = await page.EvaluateAsync<string>(
                "() => document.querySelector('.alert-danger')?.textContent ?? ''");
            throw new InvalidOperationException(
                $"Save resulted in error: '{errorText}'");
        }
    }

    /// <summary>
    /// Полный сценарий: заполнение полей ЮЛ + выбор устава + сохранение.
    /// </summary>
    public static async Task CompleteLegalEntitySetupAsync(
        IPage page,
        int charterNumber,
        string? shortName = null,
        string? ogrn = null)
    {
        await FillLegalEntityFieldsAsync(page, shortName, ogrn);
        await SelectStandardCharterAsync(page, charterNumber);
        await SaveAndVerifyAsync(page);
    }

    // ══════════════════════════════════════════════════════════════════════
    // Нетиповой устав
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Выбрать "Нетиповой устав" на вкладке "Устав" страницы /legal-entities.
    /// Выбирает option с value="" или text containing "Нетиповой" в first select.
    /// </summary>
    public static async Task SelectNonStandardCharterAsync(IPage page)
    {
        if (!page.Url.Contains("/legal-entities"))
        {
            await NavigateToAsync(page, "legal-entities");
        }

        // Click "Устав" tab
        await page.EvaluateAsync(
            @"() => {
                const tabs = document.querySelectorAll('button.nav-link');
                for (const tab of tabs) {
                    if (tab.textContent.includes('Устав')) { tab.click(); return; }
                }
            }");
        await page.WaitForTimeoutAsync(500);

        // Select "Нетиповой" option — первый option в charter type select
        var charterSelect = page.Locator("select.form-select-sm.flex-grow-1");
        if (await charterSelect.CountAsync() > 0)
        {
            // Устанавливаем через JS — SelectOptionAsync не триггерит Blazor @bind:set
            var emptyValue = await charterSelect.EvaluateAsync<string>(
                "el => el.options[0]?.value ?? ''");
            await charterSelect.EvaluateAsync(
                $@"(el, val) => {{
                    el.value = val;
                    el.dispatchEvent(new Event('change', {{ bubbles: true }}));
                }}", emptyValue);
        }
        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForTimeoutAsync(1000); // Ждём появления полей нетипового устава
    }

    /// <summary>
    /// Настроить параметр нетипового устава по data-testid атрибуту.
    /// Поддерживает: checkbox (toggle), select (dropdown), input (text/number).
    /// </summary>
    public static async Task ConfigureCharterParameterAsync(
        IPage page,
        string testId,
        string value)
    {
        // Определяем тип элемента по data-testid
        var elementType = await page.EvaluateAsync<string>(
            $@"() => {{
                const el = document.querySelector('[data-testid=""{testId}""]');
                if (!el) return 'not-found';
                if (el.tagName === 'INPUT' && el.type === 'checkbox') return 'checkbox';
                if (el.tagName === 'SELECT') return 'select';
                if (el.tagName === 'INPUT') return 'input';
                return 'unknown';
            }}");

        switch (elementType)
        {
            case "checkbox":
                var isChecked = await page.EvaluateAsync<bool>(
                    $"() => document.querySelector('[data-testid=\"{testId}\"]')?.checked ?? false");
                var shouldBeChecked = bool.Parse(value);
                if (isChecked != shouldBeChecked)
                {
                    await page.EvaluateAsync(
                        $@"() => {{
                            const el = document.querySelector('[data-testid=""{testId}""]');
                            if (el) {{ el.click(); el.dispatchEvent(new Event('change', {{ bubbles: true }})); }}
                        }}");
                }
                break;

            case "select":
                // Нормализуем регистр: "true"/"false" → "True"/"False" (для bool-селектов Blazor)
                var normalizedValue = value.Equals("true", StringComparison.OrdinalIgnoreCase)
                    ? "True"
                    : value.Equals("false", StringComparison.OrdinalIgnoreCase)
                        ? "False"
                        : value;
                await page.SelectOptionAsync($"[data-testid=\"{testId}\"]", normalizedValue);
                break;

            case "input":
                await page.EvaluateAsync(
                    $@"() => {{
                        const inp = document.querySelector('[data-testid=""{testId}""]');
                        if (inp) {{
                            inp.value = '{EscapeJs(value)}';
                            inp.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            inp.dispatchEvent(new Event('change', {{ bubbles: true }}));
                        }}
                    }}");
                break;

            case "not-found":
                throw new InvalidOperationException(
                    $"Элемент с data-testid='{testId}' не найден на странице устава.");

            default:
                throw new InvalidOperationException(
                    $"Неподдерживаемый тип элемента '{elementType}' для data-testid='{testId}'.");
        }

        await page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Проверить, что на странице отображаются поля нетипового устава
    /// (а не выпадающий список типовых уставов 01-36).
    /// </summary>
    public static async Task AssertNonStandardCharterFieldsVisibleAsync(IPage page)
    {
        // Раскрываем все секции аккордеона, чтобы проверить содержимое
        await page.EvaluateAsync(
            @"() => {
                const buttons = document.querySelectorAll('.accordion-button');
                buttons.forEach(btn => {
                    if (btn.classList.contains('collapsed')) btn.click();
                });
            }");
        await page.WaitForTimeoutAsync(500);

        var content = await page.ContentAsync();

        // Все 17 параметров нетипового устава
        content.Should().Contain("Исполнительный орган",
            "Нетиповой устав: 'Исполнительный орган'");
        content.Should().Contain("Выход участника",
            "Нетиповой устав: 'Выход участника'");
        content.Should().Contain("Преимущественное право",
            "Нетиповой устав: 'Преимущественное право'");
        content.Should().Contain("Совет директоров",
            "Нетиповой устав: 'Совет директоров'");
        content.Should().Contain("Подтверждение протокола",
            "Нетиповой устав: 'Подтверждение протокола'");
        content.Should().Contain("Обязательный аудит",
            "Нетиповой устав: 'Обязательный аудит'");
        content.Should().Contain("Ревизионная комиссия",
            "Нетиповой устав: 'Ревизионная комиссия'");
        content.Should().Contain("Срок полномочий",
            "Нетиповой устав: 'Срок полномочий'");
        content.Should().Contain("Переход доли наследникам",
            "Нетиповой устав: 'Переход доли наследникам'");

        // Проверяем наличие select-элементов (минимум 6: ExecutiveBody, Exit, Preemptive, Board, Audit, Revision)
        var selects = page.Locator("select.form-select");
        (await selects.CountAsync()).Should().BeGreaterThanOrEqualTo(6,
            "Нетиповой устав должен содержать минимум 6 select для параметров");
    }

    /// <summary>
    /// Проверить, что Совет директоров доступен (видна вкладка или поле HasBoardOfDirectors).
    /// </summary>
    public static async Task AssertBoardOfDirectorsAvailableAsync(IPage page)
    {
        var hasBoardOption = await page.EvaluateAsync<bool>(
            @"() => {
                const boardCheckbox = document.querySelector('[data-testid=""has-board""]');
                if (boardCheckbox) return true;

                // Или вкладка «Совет директоров» стала видимой
                const tabs = document.querySelectorAll('button.nav-link');
                for (const tab of tabs) {
                    if (tab.textContent.includes('Совет директоров')) return true;
                }
                return false;
            }");
        hasBoardOption.Should().BeTrue(
            "Совет директоров должен быть доступен при нетиповом уставе");
    }

    // ══════════════════════════════════════════════════════════════════════
    // Тип исполнительного органа (нетиповой устав)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Установить тип исполнительного органа (A/B/C/D/E/F) в select на странице нетипового устава.
    /// Select использует @bind="_executiveBodyStr" — устанавливаем value через JS.
    /// </summary>
    public static async Task SetExecutiveBodyAsync(IPage page, string executiveBodyType)
    {
        await page.EvaluateAsync(
            $@"() => {{
                const selects = document.querySelectorAll('select');
                for (const sel of selects) {{
                    const opts = sel.querySelectorAll('option');
                    for (const opt of opts) {{
                        if (opt.value === '{executiveBodyType}') {{
                            sel.value = '{executiveBodyType}';
                            sel.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            return;
                        }}
                    }}
                }}
            }}");
        await page.WaitForTimeoutAsync(500);
    }

    // ══════════════════════════════════════════════════════════════════════
    // Участники общества
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Добавить участника общества через API (POST /api/participants).
    /// Использует fetch() в page.EvaluateAsync для прямого вызова API.
    /// Требует авторизованную сессию Board Portal.
    /// </summary>
    public static async Task<Guid> AddParticipantAsync(
        IPage page,
        string fullName,
        decimal? sharePercent = null,
        decimal? shareAmount = null,
        string? dulTypeCode = null,
        string? dulSeries = null,
        string? dulNumber = null)
    {
        var sharePercentJson = sharePercent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null";
        var shareAmountJson = shareAmount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null";
        var dulTypeCodeJson = dulTypeCode != null ? $"'{EscapeJs(dulTypeCode)}'" : "null";
        var dulSeriesJson = dulSeries != null ? $"'{EscapeJs(dulSeries)}'" : "null";
        var dulNumberJson = dulNumber != null ? $"'{EscapeJs(dulNumber)}'" : "null";

        var result = await page.EvaluateAsync<AddParticipantResponse>(
            $@"async () => {{
                const response = await fetch('/api/participants', {{
                    method: 'POST',
                    headers: {{ 'Content-Type': 'application/json' }},
                    credentials: 'same-origin',
                    body: JSON.stringify({{
                        participantType: 'FL',
                        fullName: '{EscapeJs(fullName)}',
                        sharePercent: {sharePercentJson},
                        shareAmount: {shareAmountJson},
                        dulTypeCode: {dulTypeCodeJson},
                        dulSeries: {dulSeriesJson},
                        dulNumber: {dulNumberJson}
                    }})
                }});
                if (!response.ok) {{
                    const body = await response.text();
                    throw new Error(`POST /api/participants failed: ${{response.status}} ${{body}}`);
                }}
                return await response.json();
            }}");

        return result.Id;
    }

    /// <summary>
    /// Добавить N участников для указанного номера устава.
    /// Каждый участник получает уникальное ФИО и долю.
    /// </summary>
    public static async Task AddParticipantsForCharterAsync(
        IPage page,
        int charterNumber,
        int count)
    {
        var percents = CharterTestData.GetSharePercents(count);

        for (var i = 0; i < count; i++)
        {
            var fullName = CharterTestData.GetParticipantFullName(charterNumber, i + 1);
            var sharePercent = percents[i];

            await AddParticipantAsync(page, fullName, sharePercent: sharePercent);
            await page.WaitForTimeoutAsync(300);
        }
    }

    /// <summary>
    /// Добавить N участников для нетипового устава.
    /// </summary>
    public static async Task AddParticipantsForNonStandardCharterAsync(
        IPage page,
        int testIndex,
        int count)
    {
        var percents = CharterTestData.GetSharePercents(count);

        for (var i = 0; i < count; i++)
        {
            var fullName = NonStandardCharterTestData.GetParticipantFullName(testIndex, i + 1);
            var sharePercent = percents[i];

            await AddParticipantAsync(page, fullName, sharePercent: sharePercent);
            await page.WaitForTimeoutAsync(300);
        }
    }

    /// <summary>
    /// Проверить, что участники успешно добавлены (через GET /api/participants).
    /// </summary>
    public static async Task AssertParticipantCountAsync(IPage page, int expectedCount)
    {
        var result = await page.EvaluateAsync<ParticipantListResponse>(
            @"async () => {
                const response = await fetch('/api/participants', {
                    method: 'GET',
                    credentials: 'same-origin'
                });
                if (!response.ok) {
                    throw new Error(`GET /api/participants failed: ${response.status}`);
                }
                const data = await response.json();
                return { count: Array.isArray(data) ? data.length : 0 };
            }");

        result.Count.Should().Be(expectedCount,
            $"Ожидалось {expectedCount} участников, получено {result.Count}");
    }

    /// <summary>DTO-ответ при добавлении участника.</summary>
    private class AddParticipantResponse { public Guid Id { get; set; } }

    /// <summary>DTO-ответ при получении участника.</summary>
    internal class ParticipantResponse
    {
        public Guid Id { get; set; }
        public Guid? EcosystemParticipantId { get; set; }
        public string? FullName { get; set; }
    }

    /// <summary>DTO-ответ при получении списка участников.</summary>
    private class ParticipantListResponse { public int Count { get; set; } }

    /// <summary>
    /// Добавить участника с ПДн (паспорт, ИНН) через API.
    /// Используется для ЕДИН-тестов.
    /// </summary>
    public static async Task<Guid> AddParticipantWithPersonalDataAsync(
        IPage page,
        string fullName,
        string? passportSeries = null,
        string? passportNumber = null,
        string? personInn = null,
        string participantType = "FL",
        decimal? sharePercent = null,
        decimal? shareAmount = null)
    {
        var sharePercentJson = sharePercent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null";
        var shareAmountJson = shareAmount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null";
        var passportSeriesJson = passportSeries != null ? $"'{EscapeJs(passportSeries)}'" : "null";
        var passportNumberJson = passportNumber != null ? $"'{EscapeJs(passportNumber)}'" : "null";
        var personInnJson = personInn != null ? $"'{EscapeJs(personInn)}'" : "null";

        var result = await page.EvaluateAsync<AddParticipantResponse>(
            $@"async () => {{
                const response = await fetch('/api/participants', {{
                    method: 'POST',
                    headers: {{ 'Content-Type': 'application/json' }},
                    credentials: 'same-origin',
                    body: JSON.stringify({{
                        participantType: '{EscapeJs(participantType)}',
                        fullName: '{EscapeJs(fullName)}',
                        passportSeries: {passportSeriesJson},
                        passportNumber: {passportNumberJson},
                        personInn: {personInnJson},
                        sharePercent: {sharePercentJson},
                        shareAmount: {shareAmountJson}
                    }})
                }});
                if (!response.ok) {{
                    const body = await response.text();
                    throw new Error(`POST /api/participants failed: ${{response.status}} ${{body}}`);
                }}
                return await response.json();
            }}");

        return result.Id;
    }

    /// <summary>
    /// Получить данные участника по ID через API.
    /// </summary>
    internal static async Task<ParticipantResponse?> GetParticipantAsync(IPage page, Guid participantId)
    {
        return await page.EvaluateAsync<ParticipantResponse?>(
            $@"async () => {{
                const response = await fetch('/api/participants/{participantId}', {{
                    credentials: 'same-origin'
                }});
                if (!response.ok) return null;
                return await response.json();
            }}");
    }

    /// <summary>Получить EcosystemParticipantId для участника (связь с ФЛ).</summary>
    public static async Task<Guid?> GetEcosystemParticipantIdAsync(IPage page, Guid participantId)
    {
        var participant = await GetParticipantAsync(page, participantId);
        return participant?.EcosystemParticipantId;
    }

    // ══════════════════════════════════════════════════════════════════════
    // Вкладка «ГД» — генеральный директор
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Проверить, что вкладка «ГД» видна на странице.
    /// </summary>
    public static async Task AssertGeneralDirectorTabVisibleAsync(IPage page)
    {
        var gdTab = page.Locator("button.nav-link", new() { HasText = "ГД" });
        await gdTab.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });
        var isVisible = await gdTab.IsVisibleAsync();
        isVisible.Should().BeTrue("Вкладка «ГД» должна быть видна для ООО с executive_body = 'A'");
    }

    /// <summary>
    /// Проверить, что вкладка «ГД» НЕ видна на странице.
    /// </summary>
    public static async Task AssertGeneralDirectorTabNotVisibleAsync(IPage page)
    {
        var gdTabs = await page.Locator("button.nav-link", new() { HasText = "ГД" }).AllAsync();
        gdTabs.Should().BeEmpty("Вкладка «ГД» не должна быть видна");
    }

    /// <summary>
    /// Перейти на вкладку «ГД».
    /// </summary>
    public static async Task ClickGeneralDirectorTabAsync(IPage page)
    {
        var gdTab = page.Locator("button.nav-link", new() { HasText = "ГД" });
        await gdTab.ClickAsync();
        await page.WaitForTimeoutAsync(500);
    }

    /// <summary>
    /// Выбрать участника в качестве генерального директора (dropdown).
    /// </summary>
    public static async Task SelectGeneralDirectorAsync(IPage page, string participantFullName)
    {
        // Ищем select внутри вкладки ГД
        await page.EvaluateAsync(
            $@"() => {{
                const selects = document.querySelectorAll('select');
                for (const sel of selects) {{
                    const opts = sel.querySelectorAll('option');
                    for (const opt of opts) {{
                        if (opt.textContent.includes('{EscapeJs(participantFullName)}')) {{
                            sel.value = opt.value;
                            sel.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            return;
                        }}
                    }}
                }}
            }}");
        await page.WaitForTimeoutAsync(500);
    }

    /// <summary>
    /// Установить СНИЛС для генерального директора.
    /// </summary>
    public static async Task SetGeneralDirectorSnilsAsync(IPage page, string snils)
    {
        // Ищем input для СНИЛС внутри вкладки ГД
        await page.EvaluateAsync(
            $@"() => {{
                const inputs = document.querySelectorAll('input[type=""text""]');
                for (const inp of inputs) {{
                    const ph = inp.getAttribute('placeholder') || '';
                    if (ph.includes('123-456-789')) {{
                        inp.value = '{EscapeJs(snils)}';
                        inp.dispatchEvent(new Event('input', {{ bubbles: true }}));
                        inp.dispatchEvent(new Event('change', {{ bubbles: true }}));
                        return;
                    }}
                }}
            }}");
        await page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Проверить, что данные ГД отображаются (ФИО участника,readonly-поля).
    /// </summary>
    public static async Task AssertGeneralDirectorDataVisibleAsync(IPage page, string expectedFullName)
    {
        var content = await page.ContentAsync();
        content.Should().Contain("Генеральный директор",
            "Вкладка ГД должна содержать заголовок «Генеральный директор»");
        content.Should().Contain(expectedFullName,
            $"Данные ГД должны содержать ФИО «{expectedFullName}»");
    }

    /// <summary>
    /// Проверить, что отображается сообщение об отсутствии участников.
    /// </summary>
    public static async Task AssertNoParticipantsForGdMessageAsync(IPage page)
    {
        var content = await page.ContentAsync();
        content.Should().Contain("Добавьте участников-физлиц",
            "При отсутствии FL-участников должно отображаться сообщение-инструкция");
    }

    /// <summary>
    /// Проверить, что вкладка «ГД» содержит readonly-поля с данными участника.
    /// </summary>
    public static async Task AssertGeneralDirectorReadonlyFieldsAsync(IPage page)
    {
        var content = await page.ContentAsync();
        content.Should().Contain("Данные участника (только чтение)",
            "Данные участника-ГД должны отображаться в режиме «только чтение»");
        content.Should().Contain("СНИЛС",
            "Должно быть поле для ввода СНИЛС");
    }

    private static string EscapeJs(string value) => value.Replace("'", "\\'").Replace("\\", "\\\\");
}
