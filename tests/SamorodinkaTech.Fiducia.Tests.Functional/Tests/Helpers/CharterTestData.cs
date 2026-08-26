namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Тестовые данные для 36 типовых уставов ООО.
/// </summary>
public static class CharterTestData
{
    // LDAP user data
    public const string LdapUid = "test.gd";
    public const string LdapCn = "Тестов Тест Тестович";
    public const string LdapSn = "Тестов";
    public const string LdapGivenName = "Тест";
    public const string LdapPassword = "test1234";

    // System admin user (Васильева Вера Васильевна — SYS_ADMIN в Basic auth)
    public const string SysAdminDisplayName = "Васильева Вера Васильевна";

    // Employee data for Board Portal login
    public const string EmployeeLastName = "Тестов";
    public const string EmployeeFirstName = "Тест";
    public const string EmployeeMiddleName = "Тестович";
    public const string EmployeePosition = "Генеральный директор";
    public const string EmployeeLogin = LdapUid;

    // Role codes
    public const string RoleLeAdmin = "LE_ADMIN";
    public const string RoleCeo = "CEO";

    /// <summary>
    /// Сгенерировать уникальное название ЮЛ для указанного номера устава.
    /// </summary>
    public static string GetLegalEntityName(int charterNumber) =>
        $"Общество с ограниченной ответственностью «Тестовый Устав {charterNumber:D2}»";

    /// <summary>
    /// Сгенерировать уникальный ИНН для указанного номера устава.
    /// ИНН ООО: 10 цифр, начинается на 77.
    /// </summary>
    public static string GetLegalEntityInn(int charterNumber) =>
        $"77{charterNumber:D2}345678";

    /// <summary>
    /// Display name пользователя для входа в Board Portal.
    /// </summary>
    public static string GetBoardPortalDisplayName() => LdapCn;

    /// <summary>
    /// Ожидаемое отсутствие ошибок при сохранении.
    /// </summary>
    public static void AssertNoErrors(string? errorMessage)
    {
        if (!string.IsNullOrEmpty(errorMessage))
            throw new InvalidOperationException(
                $"Ожидалось отсутствие ошибок, но получено: '{errorMessage}'");
    }

    // ══════════════════════════════════════════════════════════════════════
    // Тестовые данные участников
    // ══════════════════════════════════════════════════════════════════════

    private static readonly Random SharedRandom = new();

    /// <summary>Минимальное количество участников.</summary>
    public const int MinParticipantCount = 1;

    /// <summary>Максимальное количество участников.</summary>
    public const int MaxParticipantCount = 3;

    /// <summary>
    /// Сгенерировать случайное количество участников от 1 до 3.
    /// </summary>
    public static int GetRandomParticipantCount() =>
        SharedRandom.Next(MinParticipantCount, MaxParticipantCount + 1);

    /// <summary>
    /// Сгенерировать ФИО участника для указанного номера устава и индекса.
    /// </summary>
    public static string GetParticipantFullName(int charterNumber, int participantIndex) =>
        $"Участник {participantIndex} Тестовый{charterNumber:D2}";

    /// <summary>
    /// Уставы с ExecutiveBody = B (каждый участник — директор): 07–12, 25–30.
    /// </summary>
    public static bool IsExecutiveBodyB(int charterNumber) =>
        (charterNumber is >= 7 and <= 12) ||
        (charterNumber is >= 25 and <= 30);

    /// <summary>
    /// Уставы с ExecutiveBody = C (все участники совместно): 13–18, 31–36.
    /// </summary>
    public static bool IsExecutiveBodyC(int charterNumber) =>
        (charterNumber is >= 13 and <= 18) ||
        (charterNumber is >= 31 and <= 36);

    /// <summary>
    /// Нужно ли добавлять участников для данного номера устава.
    /// Не добавлять для ExecutiveBody B и C.
    /// </summary>
    public static bool ShouldAddParticipants(int charterNumber) =>
        !IsExecutiveBodyB(charterNumber) && !IsExecutiveBodyC(charterNumber);

    /// <summary>
    /// Равномерно распределить 100% между N участниками.
    /// Последний участник получает остаток.
    /// </summary>
    public static decimal[] GetSharePercents(int participantCount)
    {
        if (participantCount <= 0) return [];

        var baseShare = Math.Floor(100m / participantCount);
        var percents = new decimal[participantCount];
        var remaining = 100m;

        for (var i = 0; i < participantCount - 1; i++)
        {
            percents[i] = baseShare;
            remaining -= baseShare;
        }

        percents[^1] = remaining;
        return percents;
    }
}
