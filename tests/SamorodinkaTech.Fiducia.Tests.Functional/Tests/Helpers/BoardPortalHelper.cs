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
    /// Заполнить основные поля ЮЛ на странице /legal-entities.
    /// </summary>
    public static async Task FillLegalEntityFieldsAsync(
        IPage page,
        string? shortName = null,
        string? ogrn = null)
    {
        await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/legal-entities"));
        await AuthHelper.WaitForBlazorReady(page);

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
            await page.EvaluateAsync(
                $@"() => {{
                    const inputs = document.querySelectorAll('input');
                    for (const input of inputs) {{
                        const ph = (input.placeholder || '').toLowerCase();
                        const nm = (input.name || '').toLowerCase();
                        if (ph.includes('краткое') || nm.includes('shortname')) {{
                            input.value = '{EscapeJs(shortName)}';
                            input.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            return;
                        }}
                    }}
                }}");
        }

        if (ogrn is not null)
        {
            await page.EvaluateAsync(
                $@"() => {{
                    const inputs = document.querySelectorAll('input');
                    for (const input of inputs) {{
                        const ph = (input.placeholder || '').toLowerCase();
                        const nm = (input.name || '').toLowerCase();
                        if (ph.includes('огрн') || nm.includes('ogrn')) {{
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
            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/legal-entities"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForTimeoutAsync(1000);
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
            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/legal-entities"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForTimeoutAsync(1000);
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

        // Select "Нетиповой" option in the charter type dropdown
        await page.EvaluateAsync(
            @"() => {
                const selects = document.querySelectorAll('select');
                for (const sel of selects) {
                    const opts = sel.querySelectorAll('option');
                    if (opts.length >= 2) {
                        for (const opt of opts) {
                            const txt = (opt.textContent || '').toLowerCase();
                            if (txt.includes('нетиповой') || txt.includes('индивидуальн') || txt.includes('custom')) {
                                sel.value = opt.value;
                                sel.dispatchEvent(new Event('change', { bubbles: true }));
                                return;
                            }
                        }
                        if (opts.length > 0) {
                            sel.value = opts[0].value;
                            sel.dispatchEvent(new Event('change', { bubbles: true }));
                        }
                    }
                }
            }");
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
                await page.EvaluateAsync(
                    $@"() => {{
                        const sel = document.querySelector('[data-testid=""{testId}""]');
                        if (sel) {{
                            sel.value = '{EscapeJs(value)}';
                            sel.dispatchEvent(new Event('change', {{ bubbles: true }}));
                        }}
                    }}");
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
        var hasCustomFields = await page.EvaluateAsync<bool>(
            @"() => {
                // Проверяем наличие полей данных нетипового устава:
                // либо checkboxes с data-testid, либо конкретные элементы формы
                const checkboxes = document.querySelectorAll('input[type=""checkbox""]');
                const selects = document.querySelectorAll('select');
                const textInputs = document.querySelectorAll('input[type=""text""], input[type=""number""]');

                // Нетиповой устав показывает минимум 3+ элементов управления
                // (в отличие от типового, где только 1 select с номерами 01-36)
                return (checkboxes.length + selects.length + textInputs.length) >= 3;
            }");
        hasCustomFields.Should().BeTrue(
            "Поля нетипового устава (checkboxes, selects, inputs) должны быть видны после выбора 'Нетиповой'");
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
        decimal? shareAmount = null)
    {
        var sharePercentJson = sharePercent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null";
        var shareAmountJson = shareAmount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null";

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
    private record AddParticipantResponse(Guid Id);

    /// <summary>DTO-ответ при получении списка участников.</summary>
    private record ParticipantListResponse(int Count);

    private static string EscapeJs(string value) => value.Replace("'", "\\'").Replace("\\", "\\\\");
}
