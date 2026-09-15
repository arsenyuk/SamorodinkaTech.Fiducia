using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Extension-методы для IPage, упрощающие работу с селекторами Playwright.
/// </summary>
public static class PageExtensions
{
    /// <summary>
    /// Ожидает появления элемента по селектору и бросает исключение, если элемент не найден.
    /// Комбинирует WaitForSelectorAsync + null-check + throw в один вызов.
    /// </summary>
    /// <param name="page">Страница Playwright.</param>
    /// <param name="selector">CSS-селектор.</param>
    /// <param name="humanName">Человекочитаемое имя поля для сообщения об ошибке.</param>
    /// <param name="timeoutMs">Таймаут ожидания в миллисекундах (по умолчанию — из GlobalFixture).</param>
    /// <returns>Найденный элемент.</returns>
    /// <exception cref="InvalidOperationException">Элемент не найден за отведённое время.</exception>
    public static async Task<IElementHandle> WaitForRequiredSelectorAsync(
        this IPage page,
        string selector,
        string humanName,
        int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? GlobalFixture.TestOptions.TimeoutMs;
        var element = await page.WaitForSelectorAsync(
            selector,
            new PageWaitForSelectorOptions { Timeout = timeout });

        if (element is null)
            throw new InvalidOperationException(
                $"Не найдено поле «{humanName}» ({selector})");

        return element;
    }
}
