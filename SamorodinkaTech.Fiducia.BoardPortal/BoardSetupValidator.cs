using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Валидатор назначений ролей в Совете директоров.
/// Проверяет бизнес-правила: ровно 1 Председатель, не более 1 Зам. председателя и т.д.
/// </summary>
public static class BoardSetupValidator
{
    /// <summary>
    /// Валидация списка назначений ролей в СД.
    /// </summary>
    /// <param name="assignments">Назначения для проверки.</param>
    /// <param name="ctx">Контекст БД (для проверки существования участников).</param>
    /// <param name="legalEntityId">Идентификатор юридического лица.</param>
    /// <returns>Список ошибок (пустой = валидация пройдена).</returns>
    public static async Task<List<string>> ValidateAsync(
        List<BoardSetupAssignment> assignments,
        FiduciaDbContext ctx,
        Guid legalEntityId)
    {
        var errors = new List<string>();

        if (assignments == null || assignments.Count == 0)
        {
            errors.Add("Необходимо назначить хотя бы одного Председателя СД");
            return errors;
        }

        // Ровно 1 CHAIR
        var chairs = assignments.Count(a => a.RoleCode == "CHAIR");
        if (chairs != 1)
            errors.Add("Необходим ровно 1 Председатель СД");

        // Не более 1 DEPUTY_CHAIR
        var deputies = assignments.Count(a => a.RoleCode == "DEPUTY_CHAIR");
        if (deputies > 1)
            errors.Add("Не более 1 Зам. председателя СД");

        // Не более 1 SECRETARY
        var secretaries = assignments.Count(a => a.RoleCode == "SECRETARY");
        if (secretaries > 1)
            errors.Add("Не более 1 Секретаря СД");

        // Проверка на дубли (один участник — одна роль)
        var duplicates = assignments
            .GroupBy(a => new { a.ParticipantId, a.RoleCode })
            .Where(g => g.Count() > 1);
        if (duplicates.Any())
            errors.Add("Обнаружены дублированные назначения");

        // Все участники активны и принадлежат данному ЮЛ
        var participantIds = assignments.Select(a => a.ParticipantId).Distinct().ToList();
        var validParticipants = await ctx.BoardParticipants
            .Where(p => participantIds.Contains(p.Id) && p.IsActive && p.LegalEntityId == legalEntityId)
            .Select(p => p.Id)
            .ToHashSetAsync();

        var invalid = participantIds.Where(id => !validParticipants.Contains(id)).ToList();
        if (invalid.Count > 0)
            errors.Add($"Участники не найдены или неактивны: {string.Join(", ", invalid)}");

        return errors;
    }

    /// <summary>
    /// Синхронная валидация (без проверки БД) — для unit-тестов.
    /// </summary>
    public static List<string> ValidateSync(List<BoardSetupAssignment> assignments)
    {
        var errors = new List<string>();

        if (assignments == null || assignments.Count == 0)
        {
            errors.Add("Необходимо назначить хотя бы одного Председателя СД");
            return errors;
        }

        var chairs = assignments.Count(a => a.RoleCode == "CHAIR");
        if (chairs != 1)
            errors.Add("Необходим ровно 1 Председатель СД");

        var deputies = assignments.Count(a => a.RoleCode == "DEPUTY_CHAIR");
        if (deputies > 1)
            errors.Add("Не более 1 Зам. председателя СД");

        var secretaries = assignments.Count(a => a.RoleCode == "SECRETARY");
        if (secretaries > 1)
            errors.Add("Не более 1 Секретаря СД");

        var duplicates = assignments
            .GroupBy(a => new { a.ParticipantId, a.RoleCode })
            .Where(g => g.Count() > 1);
        if (duplicates.Any())
            errors.Add("Обнаружены дублированные назначения");

        return errors;
    }
}
