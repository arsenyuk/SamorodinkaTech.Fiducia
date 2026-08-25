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
        await page.WaitForTimeoutAsync(1000);

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

    private static string EscapeJs(string value) => value.Replace("'", "\\'").Replace("\\", "\\\\");
}
