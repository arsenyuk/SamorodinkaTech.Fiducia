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
}
