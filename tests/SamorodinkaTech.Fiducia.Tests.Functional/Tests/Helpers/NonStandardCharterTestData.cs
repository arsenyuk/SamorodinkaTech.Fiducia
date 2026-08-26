namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Тестовые данные для параметризованных E2E-тестов нетипового устава ООО.
/// </summary>
public static class NonStandardCharterTestData
{
    // LDAP user data
    public const string LdapUid = "test.ns.charter";
    public const string LdapCn = "Нетиповой Тест Уставович";
    public const string LdapSn = "Нетиповой";
    public const string LdapGivenName = "Тест";
    public const string LdapPassword = "test1234";

    public const string SysAdminDisplayName = "Васильева Вера Васильевна";

    public const string EmployeeLastName = "Нетиповой";
    public const string EmployeeFirstName = "Тест";
    public const string EmployeeMiddleName = "Уставович";
    public const string EmployeePosition = "Генеральный директор";
    public const string EmployeeLogin = LdapUid;

    public const string RoleLeAdmin = "LE_ADMIN";
    public const string RoleCeo = "CEO";

    // ══════════════════════════════════════════════════════════════════════
    // Параметры нетипового устава ( LegalEntityCharter fields )
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Выход участника: разрешён, мин. доля 5%, макс. 40%, требует единогласия ОСУ.</summary>
    public const string ExitAllowed = "true";

    /// <summary>Минимальная доля участника для права на выход (%).</summary>
    public const string ExitMinSharePercent = "5";

    /// <summary>Максимальная доля участника для права на выход (%).</summary>
    public const string ExitMaxSharePercent = "40";

    /// <summary>Условия выхода — текстовое описание.</summary>
    public const string ExitConditionDescription = "по истечении 2 лет с момента вступления";

    /// <summary>Выход требует единогласного решения ОСУ.</summary>
    public const string ExitRequiresUnanimousOsu = "true";

    /// <summary>Переход доли к участникам без согласия остальных.</summary>
    public const string TransferToParticipantsWithoutConsent = "true";

    /// <summary>Переход доли к третьим лицам: CONSENT / WITHOUT_CONSENT / FORBIDDEN.</summary>
    public const string TransferToThirdParties = "CONSENT";

    /// <summary>Преимущественное право покупки доли.</summary>
    public const string PreemptiveRight = "true";

    /// <summary>Переход доли к наследникам без согласия остальных.</summary>
    public const string InheritanceWithoutConsent = "true";

    /// <summary>Тип единоличного исполнительного органа: A / B / C.</summary>
    public const string ExecutiveBody = "A";

    /// <summary>Совет директоров — только для нетипового устава.</summary>
    public const string HasBoardOfDirectors = "true";

    /// <summary>СД принимает решение о созыве ОСУ.</summary>
    public const string BoardDecidesConveningOsu = "true";

    /// <summary>Порог доли участника для требования о созыве ВОСУ (%).</summary>
    public const string VosuThresholdPercent = "15";

    // ══════════════════════════════════════════════════════════════════════
    // Имена параметров (соответствуют UI-элементам на вкладке «Устав»)
    // ══════════════════════════════════════════════════════════════════════

    public static class ParameterNames
    {
        public const string ExitAllowed = "exit-allowed";
        public const string ExitMinSharePercent = "exit-min-share";
        public const string ExitMaxSharePercent = "exit-max-share";
        public const string ExitConditionDescription = "exit-condition";
        public const string ExitRequiresUnanimousOsu = "exit-unanimous";
        public const string TransferToParticipants = "transfer-participants";
        public const string TransferToThirdParties = "transfer-third-parties";
        public const string PreemptiveRight = "preemptive-right";
        public const string InheritanceWithoutConsent = "inheritance";
        public const string ExecutiveBody = "executive-body";
        public const string HasBoardOfDirectors = "has-board";
        public const string BoardDecidesConveningOsu = "board-convenes-osu";
        public const string VosuThresholdPercent = "vosu-threshold";
    }

    public static string GetLegalEntityName(int testIndex) =>
        $"Общество с ограниченной ответственностью «Нетиповой Устав {testIndex:D2}»";

    public static string GetLegalEntityInn(int testIndex) =>
        $"78{testIndex:D2}987654";

    public static string GetShortName(int testIndex) =>
        $"ООО «НТ {testIndex:D2}»";

    public static string GetOgrn(int testIndex) =>
        $"2{testIndex:D2}987654321";
}
