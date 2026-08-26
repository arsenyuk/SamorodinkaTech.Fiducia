namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Фиксированный набор данных для E2E-тестов уставов ООО.
/// Содержит 36 типовых + 14 нетиповых уставов с привязанными ЮЛ и лицами.
/// </summary>
public static class CharterTestDataFixed
{
    // ══════════════════════════════════════════════════════════════════════
    // LDAP-пользователь для Администратора системы
    // ══════════════════════════════════════════════════════════════════════

    public const string SysAdminDisplayName = "Васильева Вера Васильевна";
    public const string SysAdminLogin = "v.vasilyeva";

    // ══════════════════════════════════════════════════════════════════════
    // Типы исполнительного органа
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Генеральный директор — отдельное лицо.</summary>
    public const char ExecutiveBodyA = 'A';

    /// <summary>Каждый участник самостоятельно действующий директор.</summary>
    public const char ExecutiveBodyB = 'B';

    /// <summary>Все участники совместно действующие директора.</summary>
    public const char ExecutiveBodyC = 'C';

    // ══════════════════════════════════════════════════════════════════════
    // Роли
    // ══════════════════════════════════════════════════════════════════════

    public const string RoleLeAdmin = "LE_ADMIN";
    public const string RoleCeo = "CEO";

    // ══════════════════════════════════════════════════════════════════════
    // Данные записей
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Фиксированный список всех юридических лиц (36 типовых + 14 нетиповых).</summary>
    public static readonly LegalEntityRecord[] LegalEntities =
    [
        // ── Типовые уставы 01–06 (ExecutiveBody A) ──────────────────
        new(1,  "Общество с ограниченной ответственностью «Тестовый Устав 01»",  "7701345678", "1013456789012", "ООО «Тест 01»",  ExecutiveBodyA),
        new(2,  "Общество с ограниченной ответственностью «Тестовый Устав 02»",  "7702345678", "1023456789012", "ООО «Тест 02»",  ExecutiveBodyA),
        new(3,  "Общество с ограниченной ответственностью «Тестовый Устав 03»",  "7703345678", "1033456789012", "ООО «Тест 03»",  ExecutiveBodyA),
        new(4,  "Общество с ограниченной ответственностью «Тестовый Устав 04»",  "7704345678", "1043456789012", "ООО «Тест 04»",  ExecutiveBodyA),
        new(5,  "Общество с ограниченной ответственностью «Тестовый Устав 05»",  "7705345678", "1053456789012", "ООО «Тест 05»",  ExecutiveBodyA),
        new(6,  "Общество с ограниченной ответственностью «Тестовый Устав 06»",  "7706345678", "1063456789012", "ООО «Тест 06»",  ExecutiveBodyA),

        // ── Типовые уставы 07–12 (ExecutiveBody B) ──────────────────
        new(7,  "Общество с ограниченной ответственностью «Тестовый Устав 07»",  "7707345678", "1073456789012", "ООО «Тест 07»",  ExecutiveBodyB),
        new(8,  "Общество с ограниченной ответственностью «Тестовый Устав 08»",  "7708345678", "1083456789012", "ООО «Тест 08»",  ExecutiveBodyB),
        new(9,  "Общество с ограниченной ответственностью «Тестовый Устав 09»",  "7709345678", "1093456789012", "ООО «Тест 09»",  ExecutiveBodyB),
        new(10, "Общество с ограниченной ответственностью «Тестовый Устав 10»",  "7710345678", "1103456789012", "ООО «Тест 10»",  ExecutiveBodyB),
        new(11, "Общество с ограниченной ответственностью «Тестовый Устав 11»",  "7711345678", "1113456789012", "ООО «Тест 11»",  ExecutiveBodyB),
        new(12, "Общество с ограниченной ответственностью «Тестовый Устав 12»",  "7712345678", "1123456789012", "ООО «Тест 12»",  ExecutiveBodyB),

        // ── Типовые уставы 13–18 (ExecutiveBody C) ──────────────────
        new(13, "Общество с ограниченной ответственностью «Тестовый Устав 13»",  "7713345678", "1133456789012", "ООО «Тест 13»",  ExecutiveBodyC),
        new(14, "Общество с ограниченной ответственностью «Тестовый Устав 14»",  "7714345678", "1143456789012", "ООО «Тест 14»",  ExecutiveBodyC),
        new(15, "Общество с ограниченной ответственностью «Тестовый Устав 15»",  "7715345678", "1153456789012", "ООО «Тест 15»",  ExecutiveBodyC),
        new(16, "Общество с ограниченной ответственностью «Тестовый Устав 16»",  "7716345678", "1163456789012", "ООО «Тест 16»",  ExecutiveBodyC),
        new(17, "Общество с ограниченной ответственностью «Тестовый Устав 17»",  "7717345678", "1173456789012", "ООО «Тест 17»",  ExecutiveBodyC),
        new(18, "Общество с ограниченной ответственностью «Тестовый Устав 18»",  "7718345678", "1183456789012", "ООО «Тест 18»",  ExecutiveBodyC),

        // ── Типовые уставы 19–24 (ExecutiveBody A) ──────────────────
        new(19, "Общество с ограниченной ответственностью «Тестовый Устав 19»",  "7719345678", "1193456789012", "ООО «Тест 19»",  ExecutiveBodyA),
        new(20, "Общество с ограниченной ответственностью «Тестовый Устав 20»",  "7720345678", "1203456789012", "ООО «Тест 20»",  ExecutiveBodyA),
        new(21, "Общество с ограниченной ответственностью «Тестовый Устав 21»",  "7721345678", "1213456789012", "ООО «Тест 21»",  ExecutiveBodyA),
        new(22, "Общество с ограниченной ответственностью «Тестовый Устав 22»",  "7722345678", "1223456789012", "ООО «Тест 22»",  ExecutiveBodyA),
        new(23, "Общество с ограниченной ответственностью «Тестовый Устав 23»",  "7723345678", "1233456789012", "ООО «Тест 23»",  ExecutiveBodyA),
        new(24, "Общество с ограниченной ответственностью «Тестовый Устав 24»",  "7724345678", "1243456789012", "ООО «Тест 24»",  ExecutiveBodyA),

        // ── Типовые уставы 25–30 (ExecutiveBody B) ──────────────────
        new(25, "Общество с ограниченной ответственностью «Тестовый Устав 25»",  "7725345678", "1253456789012", "ООО «Тест 25»",  ExecutiveBodyB),
        new(26, "Общество с ограниченной ответственностью «Тестовый Устав 26»",  "7726345678", "1263456789012", "ООО «Тест 26»",  ExecutiveBodyB),
        new(27, "Общество с ограниченной ответственностью «Тестовый Устав 27»",  "7727345678", "1273456789012", "ООО «Тест 27»",  ExecutiveBodyB),
        new(28, "Общество с ограниченной ответственностью «Тестовый Устав 28»",  "7728345678", "1283456789012", "ООО «Тест 28»",  ExecutiveBodyB),
        new(29, "Общество с ограниченной ответственностью «Тестовый Устав 29»",  "7729345678", "1293456789012", "ООО «Тест 29»",  ExecutiveBodyB),
        new(30, "Общество с ограниченной ответственностью «Тестовый Устав 30»",  "7730345678", "1303456789012", "ООО «Тест 30»",  ExecutiveBodyB),

        // ── Типовые уставы 31–36 (ExecutiveBody C) ──────────────────
        new(31, "Общество с ограниченной ответственностью «Тестовый Устав 31»",  "7731345678", "1313456789012", "ООО «Тест 31»",  ExecutiveBodyC),
        new(32, "Общество с ограниченной ответственностью «Тестовый Устав 32»",  "7732345678", "1323456789012", "ООО «Тест 32»",  ExecutiveBodyC),
        new(33, "Общество с ограниченной ответственностью «Тестовый Устав 33»",  "7733345678", "1333456789012", "ООО «Тест 33»",  ExecutiveBodyC),
        new(34, "Общество с ограниченной ответственностью «Тестовый Устав 34»",  "7734345678", "1343456789012", "ООО «Тест 34»",  ExecutiveBodyC),
        new(35, "Общество с ограниченной ответственностью «Тестовый Устав 35»",  "7735345678", "1353456789012", "ООО «Тест 35»",  ExecutiveBodyC),
        new(36, "Общество с ограниченной ответственностью «Тестовый Устав 36»",  "7736345678", "1363456789012", "ООО «Тест 36»",  ExecutiveBodyC),

        // ── Нетиповые уставы 37–50 (ExecutiveBody A по умолчанию) ───
        new(37,  "Общество с ограниченной ответственностью «Нетиповой Устав 01»", "7801987654", "2019876543212", "ООО «НТ 01»",  ExecutiveBodyA),
        new(38,  "Общество с ограниченной ответственностью «Нетиповой Устав 02»", "7802987654", "2029876543212", "ООО «НТ 02»",  ExecutiveBodyA),
        new(39,  "Общество с ограниченной ответственностью «Нетиповой Устав 03»", "7803987654", "2039876543212", "ООО «НТ 03»",  ExecutiveBodyA),
        new(40,  "Общество с ограниченной ответственностью «Нетиповой Устав 04»", "7804987654", "2049876543212", "ООО «НТ 04»",  ExecutiveBodyA),
        new(41,  "Общество с ограниченной ответственностью «Нетиповой Устав 05»", "7805987654", "2059876543212", "ООО «НТ 05»",  ExecutiveBodyA),
        new(42,  "Общество с ограниченной ответственностью «Нетиповой Устав 06»", "7806987654", "2069876543212", "ООО «НТ 06»",  ExecutiveBodyA),
        new(43,  "Общество с ограниченной ответственностью «Нетиповой Устав 07»", "7807987654", "2079876543212", "ООО «НТ 07»",  ExecutiveBodyA),
        new(44,  "Общество с ограниченной ответственностью «Нетиповой Устав 08»", "7808987654", "2089876543212", "ООО «НТ 08»",  ExecutiveBodyA),
        new(45,  "Общество с ограниченной ответственностью «Нетиповой Устав 09»", "7809987654", "2099876543212", "ООО «НТ 09»",  ExecutiveBodyA),
        new(46,  "Общество с ограниченной ответственностью «Нетиповой Устав 10»", "7810987654", "2109876543212", "ООО «НТ 10»",  ExecutiveBodyA),
        new(47,  "Общество с ограниченной ответственностью «Нетиповой Устав 11»", "7811987654", "2119876543212", "ООО «НТ 11»",  ExecutiveBodyA),
        new(48,  "Общество с ограниченной ответственностью «Нетиповой Устав 12»", "7812987654", "2129876543212", "ООО «НТ 12»",  ExecutiveBodyA),
        new(49,  "Общество с ограниченной ответственностью «Нетиповой Устав 13»", "7813987654", "2139876543212", "ООО «НТ 13»",  ExecutiveBodyA),
        new(50,  "Общество с ограниченной ответственностью «Нетиповой Устав 14»", "7814987654", "2149876543212", "ООО «НТ 14»",  ExecutiveBodyA),
    ];

    /// <summary>
    /// Фиксированный список лиц для каждого ЮЛ.
    /// Ключ — порядковый номер ЮЛ (1–50).
    /// </summary>
    public static readonly Dictionary<int, EntityPersons> PersonsByEntity = new()
    {
        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 01–06 (ExecutiveBody A): ГД отдельно + участники
        // ════════════════════════════════════════════════════════════════
        [1] = new(
            Gd: new() { Uid = "gd.tu01", FullName = "ГД Тестович Тестовый01", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый01", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый01", 60m),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый01", 40m),
            ]),
        [2] = new(
            Gd: new() { Uid = "gd.tu02", FullName = "ГД Тестович Тестовый02", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый02", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый02", 34m),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый02", 33m),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый02", 33m),
            ]),
        [3] = new(
            Gd: new() { Uid = "gd.tu03", FullName = "ГД Тестович Тестовый03", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый03", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый03", 100m),
            ]),
        [4] = new(
            Gd: new() { Uid = "gd.tu04", FullName = "ГД Тестович Тестовый04", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый04", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый04", 50m),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый04", 50m),
            ]),
        [5] = new(
            Gd: new() { Uid = "gd.tu05", FullName = "ГД Тестович Тестовый05", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый05", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый05", 25m),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый05", 25m),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый05", 50m),
            ]),
        [6] = new(
            Gd: new() { Uid = "gd.tu06", FullName = "ГД Тестович Тестовый06", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый06", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый06", 60m),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый06", 40m),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 07–12 (ExecutiveBody B): участники = ЕИО
        // ════════════════════════════════════════════════════════════════
        [7] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый07", 60m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый07", 40m, isDirector: true),
            ]),
        [8] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый08", 34m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый08", 33m, isDirector: true),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый08", 33m, isDirector: true),
            ]),
        [9] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый09", 100m, isDirector: true),
            ]),
        [10] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый10", 50m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый10", 50m, isDirector: true),
            ]),
        [11] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый11", 25m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый11", 25m, isDirector: true),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый11", 50m, isDirector: true),
            ]),
        [12] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый12", 60m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый12", 40m, isDirector: true),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 13–18 (ExecutiveBody C): участники = ЕИО совместно
        // ════════════════════════════════════════════════════════════════
        [13] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый13", 60m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый13", 40m, isDirector: true),
            ]),
        [14] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый14", 34m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый14", 33m, isDirector: true),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый14", 33m, isDirector: true),
            ]),
        [15] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый15", 100m, isDirector: true),
            ]),
        [16] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый16", 50m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый16", 50m, isDirector: true),
            ]),
        [17] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый17", 25m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый17", 25m, isDirector: true),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый17", 50m, isDirector: true),
            ]),
        [18] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый18", 60m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый18", 40m, isDirector: true),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 19–24 (ExecutiveBody A): ГД отдельно + участники
        // ════════════════════════════════════════════════════════════════
        [19] = new(
            Gd: new() { Uid = "gd.tu19", FullName = "ГД Тестович Тестовый19", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый19", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый19", 60m),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый19", 40m),
            ]),
        [20] = new(
            Gd: new() { Uid = "gd.tu20", FullName = "ГД Тестович Тестовый20", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый20", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый20", 34m),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый20", 33m),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый20", 33m),
            ]),
        [21] = new(
            Gd: new() { Uid = "gd.tu21", FullName = "ГД Тестович Тестовый21", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый21", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый21", 100m),
            ]),
        [22] = new(
            Gd: new() { Uid = "gd.tu22", FullName = "ГД Тестович Тестовый22", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый22", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый22", 50m),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый22", 50m),
            ]),
        [23] = new(
            Gd: new() { Uid = "gd.tu23", FullName = "ГД Тестович Тестовый23", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый23", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый23", 25m),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый23", 25m),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый23", 50m),
            ]),
        [24] = new(
            Gd: new() { Uid = "gd.tu24", FullName = "ГД Тестович Тестовый24", LastName = "Тестович", FirstName = "ГД", MiddleName = "Тестовый24", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый24", 60m),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый24", 40m),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 25–30 (ExecutiveBody B): участники = ЕИО
        // ════════════════════════════════════════════════════════════════
        [25] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый25", 60m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый25", 40m, isDirector: true),
            ]),
        [26] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый26", 34m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый26", 33m, isDirector: true),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый26", 33m, isDirector: true),
            ]),
        [27] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый27", 100m, isDirector: true),
            ]),
        [28] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый28", 50m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый28", 50m, isDirector: true),
            ]),
        [29] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый29", 25m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый29", 25m, isDirector: true),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый29", 50m, isDirector: true),
            ]),
        [30] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый30", 60m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый30", 40m, isDirector: true),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 31–36 (ExecutiveBody C): участники = ЕИО совместно
        // ════════════════════════════════════════════════════════════════
        [31] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый31", 60m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый31", 40m, isDirector: true),
            ]),
        [32] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый32", 34m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый32", 33m, isDirector: true),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый32", 33m, isDirector: true),
            ]),
        [33] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый33", 100m, isDirector: true),
            ]),
        [34] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый34", 50m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестович34", 50m, isDirector: true),
            ]),
        [35] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый35", 25m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый35", 25m, isDirector: true),
                PersonData.CreateParticipant("Участник 3 Тестович Тестовый35", 50m, isDirector: true),
            ]),
        [36] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Тестовый36", 60m, isDirector: true),
                PersonData.CreateParticipant("Участник 2 Тестович Тестовый36", 40m, isDirector: true),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Нетиповые уставы 37–50 (ExecutiveBody A): ГД + участники
        // ════════════════════════════════════════════════════════════════
        [37] = new(
            Gd: new() { Uid = "gd.nu01", FullName = "ГД Тестович Нетиповой01", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой01", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой01", 60m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой01", 40m),
            ]),
        [38] = new(
            Gd: new() { Uid = "gd.nu02", FullName = "ГД Тестович Нетиповой02", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой02", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой02", 34m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой02", 33m),
                PersonData.CreateParticipant("Участник 3 Тестович Нетиповой02", 33m),
            ]),
        [39] = new(
            Gd: new() { Uid = "gd.nu03", FullName = "ГД Тестович Нетиповой03", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой03", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой03", 100m),
            ]),
        [40] = new(
            Gd: new() { Uid = "gd.nu04", FullName = "ГД Тестович Нетиповой04", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой04", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой04", 50m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой04", 50m),
            ]),
        [41] = new(
            Gd: new() { Uid = "gd.nu05", FullName = "ГД Тестович Нетиповой05", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой05", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой05", 25m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой05", 25m),
                PersonData.CreateParticipant("Участник 3 Тестович Нетиповой05", 50m),
            ]),
        [42] = new(
            Gd: new() { Uid = "gd.nu06", FullName = "ГД Тестович Нетиповой06", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой06", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой06", 60m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой06", 40m),
            ]),
        [43] = new(
            Gd: new() { Uid = "gd.nu07", FullName = "ГД Тестович Нетиповой07", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой07", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой07", 34m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой07", 33m),
                PersonData.CreateParticipant("Участник 3 Тестович Нетиповой07", 33m),
            ]),
        [44] = new(
            Gd: new() { Uid = "gd.nu08", FullName = "ГД Тестович Нетиповой08", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой08", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой08", 100m),
            ]),
        [45] = new(
            Gd: new() { Uid = "gd.nu09", FullName = "ГД Тестович Нетиповой09", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой09", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой09", 50m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой09", 50m),
            ]),
        [46] = new(
            Gd: new() { Uid = "gd.nu10", FullName = "ГД Тестович Нетиповой10", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой10", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой10", 25m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой10", 25m),
                PersonData.CreateParticipant("Участник 3 Тестович Нетиповой10", 50m),
            ]),
        [47] = new(
            Gd: new() { Uid = "gd.nu11", FullName = "ГД Тестович Нетиповой11", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой11", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой11", 60m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой11", 40m),
            ]),
        [48] = new(
            Gd: new() { Uid = "gd.nu12", FullName = "ГД Тестович Нетиповой12", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой12", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой12", 34m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой12", 33m),
                PersonData.CreateParticipant("Участник 3 Тестович Нетиповой12", 33m),
            ]),
        [49] = new(
            Gd: new() { Uid = "gd.nu13", FullName = "ГД Тестович Нетиповой13", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой13", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой13", 100m),
            ]),
        [50] = new(
            Gd: new() { Uid = "gd.nu14", FullName = "ГД Тестович Нетиповой14", LastName = "Тестович", FirstName = "ГД", MiddleName = "Нетиповой14", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Участник 1 Тестович Нетиповой14", 50m),
                PersonData.CreateParticipant("Участник 2 Тестович Нетиповой14", 50m),
            ]),
    };

    // ══════════════════════════════════════════════════════════════════════
    // Записи данных
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Данные юридического лица.</summary>
    public sealed record LegalEntityRecord(
        int Number,
        string Name,
        string Inn,
        string Ogrn,
        string ShortName,
        char ExecutiveBodyType);

    /// <summary>Данные лица (LDAP-пользователь или участник).</summary>
    public sealed record PersonData
    {
        /// <summary>UID в LDAP (пусто для участников без LDAP).</summary>
        public string Uid { get; init; } = string.Empty;

        /// <summary>Полное ФИО.</summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>Фамилия.</summary>
        public string LastName { get; init; } = string.Empty;

        /// <summary>Имя.</summary>
        public string FirstName { get; init; } = string.Empty;

        /// <summary>Отчество.</summary>
        public string MiddleName { get; init; } = string.Empty;

        /// <summary>Должность.</summary>
        public string Position { get; init; } = string.Empty;

        /// <summary>Доля участника (%).</summary>
        public decimal SharePercent { get; init; }

        /// <summary>Участник является ЕИО (для типов B/C).</summary>
        public bool IsDirector { get; init; }

        /// <summary>Создать участника (без LDAP, только ФИО + доля).</summary>
        public static PersonData CreateParticipant(string fullName, decimal sharePercent, bool isDirector = false) =>
            new() { FullName = fullName, SharePercent = sharePercent, IsDirector = isDirector };
    }

    /// <summary>Набор лиц для одного ЮЛ.</summary>
    public sealed record EntityPersons(PersonData? Gd, IReadOnlyList<PersonData> Participants);
}
