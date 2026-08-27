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
        new(1,  "Общество с ограниченной ответственностью «Нечаев и Partners»",  "7701345678", "1013456789012", "ООО «Н и P»",  ExecutiveBodyA, PersonData.CreateAdmin("nechaev.va", "Нечаев Василий Алексеевич", "Нечаев", "Василий", "Алексеевич", "nechaev.va")),
        new(2,  "Общество с ограниченной ответственностью «Соболев Групп»",  "7702345678", "1023456789012", "ООО «Соболев Групп»",  ExecutiveBodyA, PersonData.CreateAdmin("sobolev.dn", "Соболев Дмитрий Николаевич", "Соболев", "Дмитрий", "Николаевич", "sobolev.dn")),
        new(3,  "Общество с ограниченной ответственностью «Токарев Технолоджиз»",  "7703345678", "1033456789012", "ООО «ТТ»",  ExecutiveBodyA, PersonData.CreateAdmin("tokarev.as", "Токарев Андрей Сергеевич", "Токарев", "Андрей", "Сергеевич", "tokarev.as")),
        new(4,  "Общество с ограниченной ответственностью «Ермаков Консалтинг»",  "7704345678", "1043456789012", "ООО «ЕК»",  ExecutiveBodyA, PersonData.CreateAdmin("ermakov.ia", "Ермаков Игорь Александрович", "Ермаков", "Игорь", "Александрович", "ermakov.ia")),
        new(5,  "Общество с ограниченной ответственностью «Ларионов Девелопмент»",  "7705345678", "1053456789012", "ООО «ЛД»",  ExecutiveBodyA, PersonData.CreateAdmin("larionov.sp", "Ларионов Сергей Павлович", "Ларионов", "Сергей", "Павлович", "larionov.sp")),
        new(6,  "Общество с ограниченной ответственностью «Савельев Инвест»",  "7706345678", "1063456789012", "ООО «СИ»",  ExecutiveBodyA, PersonData.CreateAdmin("savelyev.rt", "Савельев Роман Тимурович", "Савельев", "Роман", "Тимурович", "savelyev.rt")),

        // ── Типовые уставы 07–12 (ExecutiveBody B) ──────────────────
        new(7,  "Общество с ограниченной ответственностью «Наумов Трейд»",  "7707345678", "1073456789012", "ООО «НТ»",  ExecutiveBodyB, PersonData.CreateAdmin("naumov.vr", "Наумов Виталий Романович", "Наумов", "Виталий", "Романович", "naumov.vr")),
        new(8,  "Общество с ограниченной ответственностью «Щукин Сервис»",  "7708345678", "1083456789012", "ООО «ЩС»",  ExecutiveBodyB, PersonData.CreateAdmin("shchukin.mo", "Щукин Михаил Олегович", "Щукин", "Михаил", "Олегович", "shchukin.mo")),
        new(9,  "Общество с ограниченной ответственностью «Скамыкин Логистикс»",  "7709345678", "1093456789012", "ООО «СЛ»",  ExecutiveBodyB, PersonData.CreateAdmin("skomykin.vk", "Скамыкин Виктор Кириллович", "Скамыкин", "Виктор", "Кириллович", "skomykin.vk")),
        new(10, "Общество с ограниченной ответственностью «Тихонов Энерджи»",  "7710345678", "1103456789012", "ООО «ТЭ»",  ExecutiveBodyB, PersonData.CreateAdmin("tikhonov.ab", "Тихонов Аркадий Борисович", "Тихонов", "Аркадий", "Борисович", "tikhonov.ab")),
        new(11, "Общество с ограниченной ответственностью «Зимин Медиа»",  "7711345678", "1113456789012", "ООО «ЗМ»",  ExecutiveBodyB, PersonData.CreateAdmin("zimin.fi", "Зимин Фёдор Ильич", "Зимин", "Фёдор", "Ильич", "zimin.fi")),
        new(12, "Общество с ограниченной ответственностью «Пономарёв Солюшнз»",  "7712345678", "1123456789012", "ООО «ПС»",  ExecutiveBodyB, PersonData.CreateAdmin("ponomarev.vs", "Пономарёв Вадим Сергеевич", "Пономарёв", "Вадим", "Сергеевич", "ponomarev.vs")),

        // ── Типовые уставы 13–18 (ExecutiveBody C) ──────────────────
        new(13, "Общество с ограниченной ответственностью «Высоцкий Холдинг»",  "7713345678", "1133456789012", "ООО «ВХ»",  ExecutiveBodyC, PersonData.CreateAdmin("vysockiy.ov", "Высоцкий Олег Васильевич", "Высоцкий", "Олег", "Васильевич", "vysockiy.ov")),
        new(14, "Общество с ограниченной ответственностью «Гладков Тех»",  "7714345678", "1143456789012", "ООО «ГТ»",  ExecutiveBodyC, PersonData.CreateAdmin("gladkov.sa", "Гладков Сергей Алексеевич", "Гладков", "Сергей", "Алексеевич", "gladkov.sa")),
        new(15, "Общество с ограниченной ответственностью «Давыденко Групп»",  "7715345678", "1153456789012", "ООО «ДГ»",  ExecutiveBodyC, PersonData.CreateAdmin("davydenko.ti", "Давыденко Тарас Иванович", "Давыденко", "Тарас", "Иванович", "davydenko.ti")),
        new(16, "Общество с ограниченной ответственностью «Ельцов Финанс»",  "7716345678", "1163456789012", "ООО «ЕФ»",  ExecutiveBodyC, PersonData.CreateAdmin("eltsov.vn", "Ельцов Виктор Николаевич", "Ельцов", "Виктор", "Николаевич", "eltsov.vn")),
        new(17, "Общество с ограниченной ответственностью «Зайцев Продакшн»",  "7717345678", "1173456789012", "ООО «ЗП»",  ExecutiveBodyC, PersonData.CreateAdmin("zaitsev.mo", "Зайцев Максим Олегович", "Зайцев", "Максим", "Олегович", "zaitsev.mo")),
        new(18, "Общество с ограниченной ответственностью «Капустин Альянс»",  "7718345678", "1183456789012", "ООО «КА»",  ExecutiveBodyC, PersonData.CreateAdmin("kapustin.yv", "Капустин Ярослав Вадимович", "Капустин", "Ярослав", "Вадимович", "kapustin.yv")),

        // ── Типовые уставы 19–24 (ExecutiveBody A) ──────────────────
        new(19, "Общество с ограниченной ответственностью «Лихачёв Инновейшн»",  "7719345678", "1193456789012", "ООО «ЛИ»",  ExecutiveBodyA, PersonData.CreateAdmin("likhachev.gv", "Лихачёв Глеб Викторович", "Лихачёв", "Глеб", "Викторович", "likhachev.gv")),
        new(20, "Общество с ограниченной ответственностью «Матвеев Лабс»",  "7720345678", "1203456789012", "ООО «МЛ»",  ExecutiveBodyA, PersonData.CreateAdmin("matveev.yi", "Матвеев Ярослав Игоревич", "Матвеев", "Ярослав", "Игоревич", "matveev.yi")),
        new(21, "Общество с ограниченной ответственностью «Носков Инвестментс»",  "7721345678", "1213456789012", "ООО «НИ»",  ExecutiveBodyA, PersonData.CreateAdmin("noskov.vs", "Носков Виталий Сергеевич", "Носков", "Виталий", "Сергеевич", "noskov.vs")),
        new(22, "Общество с ограниченной ответственностью «Овчинников Девелопмент»",  "7722345678", "1223456789012", "ООО «ОД»",  ExecutiveBodyA, PersonData.CreateAdmin("ovchinnikov.so", "Овчинников Святослав Олегович", "Овчинников", "Святослав", "Олегович", "ovchinnikov.so")),
        new(23, "Общество с ограниченной ответственностью «Пустырников Консалтинг»",  "7723345678", "1233456789012", "ООО «ПК»",  ExecutiveBodyA, PersonData.CreateAdmin("pustyrnikov.iy", "Пустырников Игорь Юрьевич", "Пустырников", "Игорь", "Юрьевич", "pustyrnikov.iy")),
        new(24, "Общество с ограниченной ответственностью «Ртищев Технолоджиз»",  "7724345678", "1243456789012", "ООО «РТ»",  ExecutiveBodyA, PersonData.CreateAdmin("rtishchev.aa", "Ртищев Аркадий Андреевич", "Ртищев", "Аркадий", "Андреевич", "rtishchev.aa")),

        // ── Типовые уставы 25–30 (ExecutiveBody B) ──────────────────
        new(25, "Общество с ограниченной ответственностью «Сухов Групп»",  "7725345678", "1253456789012", "ООО «СГ»",  ExecutiveBodyB, PersonData.CreateAdmin("sukhov.da", "Сухов Данил Александрович", "Сухов", "Данил", "Александрович", "sukhov.da")),
        new(26, "Общество с ограниченной ответственностью «Толкачёв Сервис»",  "7726345678", "1263456789012", "ООО «ТС»",  ExecutiveBodyB, PersonData.CreateAdmin("tolkachev.no", "Толкачёв Никита Олегович", "Толкачёв", "Никита", "Олегович", "tolkachev.no")),
        new(27, "Общество с ограниченной ответственностью «Ушаков Трейд»",  "7727345678", "1273456789012", "ООО «УТ»",  ExecutiveBodyB, PersonData.CreateAdmin("ushakov.vs", "Ушаков Владислав Сергеевич", "Ушаков", "Владислав", "Сергеевич", "ushakov.vs")),
        new(28, "Общество с ограниченной ответственностью «Филиппов Логистикс»",  "7728345678", "1283456789012", "ООО «ФЛ»",  ExecutiveBodyB, PersonData.CreateAdmin("filippov.so", "Филиппов Семён Олегович", "Филиппов", "Семён", "Олегович", "filippov.so")),
        new(29, "Общество с ограниченной ответственностью «Харитонов Энерджи»",  "7729345678", "1293456789012", "ООО «ХЭ»",  ExecutiveBodyB, PersonData.CreateAdmin("kharitonov.an", "Харитонов Алексей Николаевич", "Харитонов", "Алексей", "Николаевич", "kharitonov.an")),
        new(30, "Общество с ограниченной ответственностью «Цыганков Медиа»",  "7730345678", "1303456789012", "ООО «ЦМ»",  ExecutiveBodyB, PersonData.CreateAdmin("tsygankov.av", "Цыганков Артём Вадимович", "Цыганков", "Артём", "Вадимович", "tsygankov.av")),

        // ── Типовые уставы 31–36 (ExecutiveBody C) ──────────────────
        new(31, "Общество с ограниченной ответственностью «Шмелёв Альянс»",  "7731345678", "1313456789012", "ООО «ША»",  ExecutiveBodyC, PersonData.CreateAdmin("shmelev.oa", "Шмелёв Олег Александрович", "Шмелёв", "Олег", "Александрович", "shmelev.oa")),
        new(32, "Общество с ограниченной ответственностью «Юдин Финанс»",  "7732345678", "1323456789012", "ООО «ЮФ»",  ExecutiveBodyC, PersonData.CreateAdmin("yudin.ri", "Юдин Роман Игоревич", "Юдин", "Роман", "Игоревич", "yudin.ri")),
        new(33, "Общество с ограниченной ответственностью «Яковлев Продакшн»",  "7733345678", "1333456789012", "ООО «ЯП»",  ExecutiveBodyC, PersonData.CreateAdmin("yakovlev.sd", "Яковлев Святослав Дмитриевич", "Яковлев", "Святослав", "Дмитриевич", "yakovlev.sd")),
        new(34, "Общество с ограниченной ответственностью «Абросимов Инновейшн»",  "7734345678", "1343456789012", "ООО «АИ»",  ExecutiveBodyC, PersonData.CreateAdmin("abrosimov.pv", "Абросимов Павел Валерьевич", "Абросимов", "Павел", "Валерьевич", "abrosimov.pv")),
        new(35, "Общество с ограниченной ответственностью «Булатов Лабс»",  "7735345678", "1353456789012", "ООО «БЛ»",  ExecutiveBodyC, PersonData.CreateAdmin("bulatov.ts", "Булатов Тарас Сергеевич", "Булатов", "Тарас", "Сергеевич", "bulatov.ts")),
        new(36, "Общество с ограниченной ответственностью «Васильев Инвестментс»",  "7736345678", "1363456789012", "ООО «ВИ»",  ExecutiveBodyC, PersonData.CreateAdmin("vasiliev.vo", "Васильев Владислав Олегович", "Васильев", "Владислав", "Олегович", "vasiliev.vo")),

        // ── Нетиповые уставы 37–50 (ExecutiveBody A по умолчанию) ───
        new(37,  "Общество с ограниченной ответственностью «Гарин Холдинг»", "7801987654", "2019876543212", "ООО «ГХ»",  ExecutiveBodyA, PersonData.CreateAdmin("garin.sa", "Гарин Станислав Андреевич", "Гарин", "Станислав", "Андреевич", "garin.sa")),
        new(38,  "Общество с ограниченной ответственностью «Дементьев Тех»", "7802987654", "2029876543212", "ООО «ДТ»",  ExecutiveBodyA, PersonData.CreateAdmin("dementiev.rs", "Дементьев Роман Сергеевич", "Дементьев", "Роман", "Сергеевич", "dementiev.rs")),
        new(39,  "Общество с ограниченной ответственностью «Ефимов Групп»", "7803987654", "2039876543212", "ООО «ЕГ»",  ExecutiveBodyA, PersonData.CreateAdmin("efimov.yd", "Ефимов Ярослав Дмитриевич", "Ефимов", "Ярослав", "Дмитриевич", "efimov.yd")),
        new(40,  "Общество с ограниченной ответственностью «Жуков Финанс»", "7804987654", "2049876543212", "ООО «ЖФ»",  ExecutiveBodyA, PersonData.CreateAdmin("zhukov.dp", "Жуков Даниил Павлович", "Жуков", "Даниил", "Павлович", "zhukov.dp")),
        new(41,  "Общество с ограниченной ответственностью «Зайцев Продакшн»", "7805987654", "2059876543212", "ООО «ЗП»",  ExecutiveBodyA, PersonData.CreateAdmin("zaitsev.so", "Зайцев Святослав Олегович", "Зайцев", "Святослав", "Олегович", "zaitsev.so")),
        new(42,  "Общество с ограниченной ответственностью «Ильин Инновейшн»", "7806987654", "2069876543212", "ООО «ЛИ»",  ExecutiveBodyA, PersonData.CreateAdmin("ilin.vn", "Ильин Владислав Николаевич", "Ильин", "Владислав", "Николаевич", "ilin.vn")),
        new(43,  "Общество с ограниченной ответственностью «Козлов Лабс»", "7807987654", "2079876543212", "ООО «КЛ»",  ExecutiveBodyA, PersonData.CreateAdmin("kozlov.ra", "Козлов Ринат Алексеевич", "Козлов", "Ринат", "Алексеевич", "kozlov.ra")),
        new(44,  "Общество с ограниченной ответственностью «Ларионов Инвестментс»", "7808987654", "2089876543212", "ООО «ЛИ»",  ExecutiveBodyA, PersonData.CreateAdmin("larionov.si", "Ларионов Станислав Игоревич", "Ларионов", "Станислав", "Игоревич", "larionov.si")),
        new(45,  "Общество с ограниченной ответственностью «Михайлов Девелопмент»", "7809987654", "2099876543212", "ООО «МД»",  ExecutiveBodyA, PersonData.CreateAdmin("mikhailov.as", "Михайлов Артём Сергеевич", "Михайлов", "Артём", "Сергеевич", "mikhailov.as")),
        new(46,  "Общество с ограниченной ответственностью «Нечаев Консалтинг»", "7810987654", "2109876543212", "ООО «НК»",  ExecutiveBodyA, PersonData.CreateAdmin("nechaev.dv", "Нечаев Данил Вадимович", "Нечаев", "Данил", "Вадимович", "nechaev.dv")),
        new(47,  "Общество с ограниченной ответственностью «Овчинников Трейд»", "7811987654", "2119876543212", "ООО «ОТ»",  ExecutiveBodyA, PersonData.CreateAdmin("ovchinnikov.tr", "Овчинников Тимур Романович", "Овчинников", "Тимур", "Романович", "ovchinnikov.tr")),
        new(48,  "Общество с ограниченной ответственностью «Пономарёв Сервис»", "7812987654", "2129876543212", "ООО «ПС»",  ExecutiveBodyA, PersonData.CreateAdmin("ponomarev.ip", "Пономарёв Игорь Павлович", "Пономарёв", "Игорь", "Павлович", "ponomarev.ip")),
        new(49,  "Общество с ограниченной ответственностью «Рябов Логистикс»", "7813987654", "2139876543212", "ООО «РЛ»",  ExecutiveBodyA, PersonData.CreateAdmin("ryabov.vs", "Рябов Владислав Сергеевич", "Рябов", "Владислав", "Сергеевич", "ryabov.vs")),
        new(50,  "Общество с ограниченной ответственностью «Савельев Медиа»", "7814987654", "2149876543212", "ООО «СМ»",  ExecutiveBodyA, PersonData.CreateAdmin("savelyev.ro", "Савельев Ринат Олегович", "Савельев", "Ринат", "Олегович", "savelyev.ro")),
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
            Gd: new() { Uid = "nechaev.va", Login = "nechaev.va", FullName = "Нечаев Василий Алексеевич", LastName = "Нечаев", FirstName = "Василий", MiddleName = "Алексеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Жиров Антон Тарасович", 60m, login: "zhirov.at1"),
                PersonData.CreateParticipant("Жирова Елена Тарасовна", 40m, login: "zhirova.et1"),
            ]),
        [2] = new(
            Gd: new() { Uid = "sobolev.dn", Login = "sobolev.dn", FullName = "Соболев Дмитрий Николаевич", LastName = "Соболев", FirstName = "Дмитрий", MiddleName = "Николаевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Бирюков Олег Степанович", 34m, login: "birukov.os2"),
                PersonData.CreateParticipant("Бирюкова Ирина Степановна", 33m, login: "birukova.is2"),
                PersonData.CreateParticipant("Бирюков Станислав Степанович", 33m, login: "birukov.ss2"),
            ]),
        [3] = new(
            Gd: new() { Uid = "tokarev.as", Login = "tokarev.as", FullName = "Токарев Андрей Сергеевич", LastName = "Токарев", FirstName = "Андрей", MiddleName = "Сергеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Вешняков Максим Юрьевич", 100m, login: "veshnyakov.my3"),
            ]),
        [4] = new(
            Gd: new() { Uid = "ermakov.ia", Login = "ermakov.ia", FullName = "Ермаков Игорь Александрович", LastName = "Ермаков", FirstName = "Игорь", MiddleName = "Александрович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Зубов Владимир Игоревич", 50m, login: "zubov.vi4"),
                PersonData.CreateParticipant("Зубова Наталья Игоревна", 50m, login: "zubova.ni4"),
            ]),
        [5] = new(
            Gd: new() { Uid = "larionov.sp", Login = "larionov.sp", FullName = "Ларионов Сергей Павлович", LastName = "Ларионов", FirstName = "Сергей", MiddleName = "Павлович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Прохоров Алексей Викторович", 25m, login: "prokhorev.av5"),
                PersonData.CreateParticipant("Прохорова Ольга Викторовна", 25m, login: "prokhoreva.ov5"),
                PersonData.CreateParticipant("Прохоров Пётр Викторович", 50m, login: "prokhorev.pv5"),
            ]),
        [6] = new(
            Gd: new() { Uid = "savelyev.rt", Login = "savelyev.rt", FullName = "Савельев Роман Тимурович", LastName = "Савельев", FirstName = "Роман", MiddleName = "Тимурович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Широков Денис Александрович", 60m, login: "shirokov.da6"),
                PersonData.CreateParticipant("Широкова Мария Александровна", 40m, login: "shirokova.ma6"),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 07–12 (ExecutiveBody B): участники = ЕИО
        // ════════════════════════════════════════════════════════════════
        [7] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Комаров Степан Андреевич", 60m, isDirector: true, login: "komarov.sa7"),
                PersonData.CreateParticipant("Комарова Вера Андреевна", 40m, isDirector: true, login: "komarova.va7"),
            ]),
        [8] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Лапин Дмитрий Сергеевич", 34m, isDirector: true, login: "lapin.ds8"),
                PersonData.CreateParticipant("Лапина Анна Сергеевна", 33m, isDirector: true, login: "lapina.as8"),
                PersonData.CreateParticipant("Лапин Игорь Сергеевич", 33m, isDirector: true, login: "lapin.is8"),
            ]),
        [9] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Рябов Николай Вадимович", 100m, isDirector: true, login: "ryabov.nv9"),
            ]),
        [10] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Евстигнеев Павел Данилович", 50m, isDirector: true, login: "evstignxeev.pd10"),
                PersonData.CreateParticipant("Евстигнеева Татьяна Даниловна", 50m, isDirector: true, login: "evstignxeeva.td10"),
            ]),
        [11] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Калачёв Ринат Александрович", 25m, isDirector: true, login: "kalachev.ra11"),
                PersonData.CreateParticipant("Калачёва Светлана Александровна", 25m, isDirector: true, login: "kalacheva.sa11"),
                PersonData.CreateParticipant("Калачёв Тимур Александрович", 50m, isDirector: true, login: "kalachev.ta11"),
            ]),
        [12] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Горбунов Евгений Леонидович", 60m, isDirector: true, login: "gorbunov.el12"),
                PersonData.CreateParticipant("Горбунова Марина Леонидовна", 40m, isDirector: true, login: "gorbunova.ml12"),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 13–18 (ExecutiveBody C): участники = ЕИО совместно
        // ════════════════════════════════════════════════════════════════
        [13] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Шестаков Пётр Николаевич", 60m, isDirector: true, login: "shestakov.pn13"),
                PersonData.CreateParticipant("Шестакова Лариса Николаевна", 40m, isDirector: true, login: "shestakova.ln13"),
            ]),
        [14] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Суханов Илья Максимович", 34m, isDirector: true, login: "sukhanov.im14"),
                PersonData.CreateParticipant("Суханова Екатерина Максимовна", 33m, isDirector: true, login: "sukhanova.em14"),
                PersonData.CreateParticipant("Суханов Кирилл Максимович", 33m, isDirector: true, login: "sukhanov.km14"),
            ]),
        [15] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Бельский Глеб Андреевич", 100m, isDirector: true, login: "belsky.ga15"),
            ]),
        [16] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Головин Святослав Алексеевич", 50m, isDirector: true, login: "golovin.sa16"),
                PersonData.CreateParticipant("Головина Надежда Алексеевна", 50m, isDirector: true, login: "golovina.na16"),
            ]),
        [17] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Денисов Владислав Юрьевич", 25m, isDirector: true, login: "denisov.vy17"),
                PersonData.CreateParticipant("Денисова Ксения Юрьевна", 25m, isDirector: true, login: "denisova.ky17"),
                PersonData.CreateParticipant("Денисов Роман Юрьевич", 50m, isDirector: true, login: "denisov.ry17"),
            ]),
        [18] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Ершов Тимур Сергеевич", 60m, isDirector: true, login: "ershov.ts18"),
                PersonData.CreateParticipant("Ершова Алиса Сергеевна", 40m, isDirector: true, login: "ershova.as18"),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 19–24 (ExecutiveBody A): ГД отдельно + участники
        // ════════════════════════════════════════════════════════════════
        [19] = new(
            Gd: new() { Uid = "likhachev.gv", Login = "likhachev.gv", FullName = "Лихачёв Глеб Викторович", LastName = "Лихачёв", FirstName = "Глеб", MiddleName = "Викторович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Фролов Станислав Андреевич", 60m, login: "frolov.sa19"),
                PersonData.CreateParticipant("Фролова Виктория Андреевна", 40m, login: "frolova.va19"),
            ]),
        [20] = new(
            Gd: new() { Uid = "matveev.yi", Login = "matveev.yi", FullName = "Матвеев Ярослав Игоревич", LastName = "Матвеев", FirstName = "Ярослав", MiddleName = "Игоревич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Харитонов Семён Павлович", 34m, login: "kharitonov.sp20"),
                PersonData.CreateParticipant("Харитонова Дарья Павловна", 33m, login: "kharitonova.dp20"),
                PersonData.CreateParticipant("Харитонов Илья Павлович", 33m, login: "kharitonov.ip20"),
            ]),
        [21] = new(
            Gd: new() { Uid = "noskov.vs", Login = "noskov.vs", FullName = "Носков Виталий Сергеевич", LastName = "Носков", FirstName = "Виталий", MiddleName = "Сергеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Цветков Михаил Евгеньевич", 100m, login: "tsvetkov.me21"),
            ]),
        [22] = new(
            Gd: new() { Uid = "ovchinnikov.so", Login = "ovchinnikov.so", FullName = "Овчинников Святослав Олегович", LastName = "Овчинников", FirstName = "Святослав", MiddleName = "Олегович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Чесноков Денис Валерьевич", 50m, login: "chesnokov.dv22"),
                PersonData.CreateParticipant("Чеснокова Оксана Валерьевна", 50m, login: "chesnokova.ov22"),
            ]),
        [23] = new(
            Gd: new() { Uid = "pustyrnikov.iy", Login = "pustyrnikov.iy", FullName = "Пустырников Игорь Юрьевич", LastName = "Пустырников", FirstName = "Игорь", MiddleName = "Юрьевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Шульга Тарас Игоревич", 25m, login: "shulga.ti23"),
                PersonData.CreateParticipant("Шульга Марина Игоревна", 25m, login: "shulga.mi23"),
                PersonData.CreateParticipant("Шульга Алексей Игоревич", 50m, login: "shulga.ai23"),
            ]),
        [24] = new(
            Gd: new() { Uid = "rtishchev.aa", Login = "rtishchev.aa", FullName = "Ртищев Аркадий Андреевич", LastName = "Ртищев", FirstName = "Аркадий", MiddleName = "Андреевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Юдин Кирилл Леонидович", 60m, login: "yudin.kl24"),
                PersonData.CreateParticipant("Юдинова Анна Леонидовна", 40m, login: "yudinova.al24"),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 25–30 (ExecutiveBody B): участники = ЕИО
        // ════════════════════════════════════════════════════════════════
        [25] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Абрамов Роман Викторович", 60m, isDirector: true, login: "abramov.rv25"),
                PersonData.CreateParticipant("Абрамова Ирина Викторовна", 40m, isDirector: true, login: "abramova.iv25"),
            ]),
        [26] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Блинов Степан Андреевич", 34m, isDirector: true, login: "blinov.sa26"),
                PersonData.CreateParticipant("Блинова Татьяна Андреевна", 33m, isDirector: true, login: "blinova.ta26"),
                PersonData.CreateParticipant("Блинов Артём Андреевич", 33m, isDirector: true, login: "blinov.aa26"),
            ]),
        [27] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Виноградов Пётр Дмитриевич", 100m, isDirector: true, login: "vinogradov.pd27"),
            ]),
        [28] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Громов Ринат Игоревич", 50m, isDirector: true, login: "gromov.ri28"),
                PersonData.CreateParticipant("Громова Елена Игоревна", 50m, isDirector: true, login: "gromova.ei28"),
            ]),
        [29] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Демидов Илья Сергеевич", 25m, isDirector: true, login: "demidov.is29"),
                PersonData.CreateParticipant("Демидова Кристина Сергеевна", 25m, isDirector: true, login: "demidova.ks29"),
                PersonData.CreateParticipant("Демидов Тимур Сергеевич", 50m, isDirector: true, login: "demidov.ts29"),
            ]),
        [30] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Ермаков Святослав Павлович", 60m, isDirector: true, login: "ermakov.sp30"),
                PersonData.CreateParticipant("Ермакова Виктория Павловна", 40m, isDirector: true, login: "ermakova.vp30"),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 31–36 (ExecutiveBody C): участники = ЕИО совместно
        // ════════════════════════════════════════════════════════════════
        [31] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Жуков Денис Викторович", 60m, isDirector: true, login: "zhukov.dv31"),
                PersonData.CreateParticipant("Жукова Светлана Викторовна", 40m, isDirector: true, login: "zhukova.sv31"),
            ]),
        [32] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Зимовец Илья Сергеевич", 34m, isDirector: true, login: "zimovec.is32"),
                PersonData.CreateParticipant("Зимовец Анна Сергеевна", 33m, isDirector: true, login: "zimovec.as32"),
                PersonData.CreateParticipant("Зимовец Кирилл Сергеевич", 33m, isDirector: true, login: "zimovec.ks32"),
            ]),
        [33] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Казаков Николай Олегович", 100m, isDirector: true, login: "kazakov.no33"),
            ]),
        [34] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Ларин Ринат Андреевич", 50m, isDirector: true, login: "larin.ra34"),
                PersonData.CreateParticipant("Ларина Ольга Андреевна", 50m, isDirector: true, login: "larina.oa34"),
            ]),
        [35] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Мещеряков Артём Павлович", 25m, isDirector: true, login: "meshcheryakov.ap35"),
                PersonData.CreateParticipant("Мещерякова Елена Павловна", 25m, isDirector: true, login: "meshcheryakova.ep35"),
                PersonData.CreateParticipant("Мещеряков Владислав Павлович", 50m, isDirector: true, login: "meshcheryakov.vp35"),
            ]),
        [36] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Некрасов Дмитрий Викторович", 60m, isDirector: true, login: "nekrasov.dv36"),
                PersonData.CreateParticipant("Некрасова Мария Викторовна", 40m, isDirector: true, login: "nekrasova.mv36"),
            ]),

        // ════════════════════════════════════════════════════════════════
        // Нетиповые уставы 37–50 (ExecutiveBody A): ГД + участники
        // ════════════════════════════════════════════════════════════════
        [37] = new(
            Gd: new() { Uid = "garin.sa", Login = "garin.sa", FullName = "Гарин Станислав Андреевич", LastName = "Гарин", FirstName = "Станислав", MiddleName = "Андреевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Ельцов Игорь Вадимович", 60m, login: "eltsov.iv37"),
                PersonData.CreateParticipant("Ельцов Вера Вадимовна", 40m, login: "eltsova.vv37"),
            ]),
        [38] = new(
            Gd: new() { Uid = "dementiev.rs", Login = "dementiev.rs", FullName = "Дементьев Роман Сергеевич", LastName = "Дементьев", FirstName = "Роман", MiddleName = "Сергеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Зубков Семён Александрович", 34m, login: "zubkov.sa38"),
                PersonData.CreateParticipant("Зубкова Татьяна Александровна", 33m, login: "zubkova.ta38"),
                PersonData.CreateParticipant("Зубков Аркадий Александрович", 33m, login: "zubkov.aa38"),
            ]),
        [39] = new(
            Gd: new() { Uid = "efimov.yd", Login = "efimov.yd", FullName = "Ефимов Ярослав Дмитриевич", LastName = "Ефимов", FirstName = "Ярослав", MiddleName = "Дмитриевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Ильин Максим Викторович", 100m, login: "ilin.mv39"),
            ]),
        [40] = new(
            Gd: new() { Uid = "zhukov.dp", Login = "zhukov.dp", FullName = "Жуков Даниил Павлович", LastName = "Жуков", FirstName = "Даниил", MiddleName = "Павлович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Корнеев Алексей Сергеевич", 50m, login: "korneev.as40"),
                PersonData.CreateParticipant("Корнеева Наталья Сергеевна", 50m, login: "korneeva.ns40"),
            ]),
        [41] = new(
            Gd: new() { Uid = "zaitsev.so", Login = "zaitsev.so", FullName = "Зайцев Святослав Олегович", LastName = "Зайцев", FirstName = "Святослав", MiddleName = "Олегович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Лебедев Тарас Валерьевич", 25m, login: "lebedev.tv41"),
                PersonData.CreateParticipant("Лебедева Ольга Валерьевна", 25m, login: "lebedeva.ov41"),
                PersonData.CreateParticipant("Лебедев Пётр Валерьевич", 50m, login: "lebedev.pv41"),
            ]),
        [42] = new(
            Gd: new() { Uid = "ilin.vn", Login = "ilin.vn", FullName = "Ильин Владислав Николаевич", LastName = "Ильин", FirstName = "Владислав", MiddleName = "Николаевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Мельников Станислав Дмитриевич", 60m, login: "melnikov.sd42"),
                PersonData.CreateParticipant("Мельникова Виктория Дмитриевна", 40m, login: "melnikova.vd42"),
            ]),
        [43] = new(
            Gd: new() { Uid = "kozlov.ra", Login = "kozlov.ra", FullName = "Козлов Ринат Алексеевич", LastName = "Козлов", FirstName = "Ринат", MiddleName = "Алексеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Носков Семён Олегович", 34m, login: "noskov.so43"),
                PersonData.CreateParticipant("Носкова Анна Олеговна", 33m, login: "noskova.ao43"),
                PersonData.CreateParticipant("Носков Игорь Олегович", 33m, login: "noskov.io43"),
            ]),
        [44] = new(
            Gd: new() { Uid = "larionov.si", Login = "larionov.si", FullName = "Ларионов Станислав Игоревич", LastName = "Ларионов", FirstName = "Станислав", MiddleName = "Игоревич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Овсов Роман Андреевич", 50m, login: "ovsov.ra44"),
                PersonData.CreateParticipant("Овсова Марина Андреевна", 50m, login: "ovsova.ma44"),
            ]),
        [45] = new(
            Gd: new() { Uid = "mikhailov.as", Login = "mikhailov.as", FullName = "Михайлов Артём Сергеевич", LastName = "Михайлов", FirstName = "Артём", MiddleName = "Сергеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Павлов Святослав Викторович", 60m, login: "pavlov.sv45"),
                PersonData.CreateParticipant("Павлова Елена Викторовна", 40m, login: "pavlova.ev45"),
            ]),
        [46] = new(
            Gd: new() { Uid = "nechaev.dv", Login = "nechaev.dv", FullName = "Нечаев Данил Вадимович", LastName = "Нечаев", FirstName = "Данил", MiddleName = "Вадимович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Рогов Илья Николаевич", 60m, login: "rogov.in46"),
                PersonData.CreateParticipant("Рогова Ксения Николаевна", 40m, login: "rogova.kn46"),
            ]),
        [47] = new(
            Gd: new() { Uid = "ovchinnikov.tr", Login = "ovchinnikov.tr", FullName = "Овчинников Тимур Романович", LastName = "Овчинников", FirstName = "Тимур", MiddleName = "Романович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Сафонов Аркадий Сергеевич", 60m, login: "safonov.as47"),
                PersonData.CreateParticipant("Сафонова Дарья Сергеевна", 40m, login: "safonova.ds47"),
            ]),
        [48] = new(
            Gd: new() { Uid = "ponomarev.ip", Login = "ponomarev.ip", FullName = "Пономарёв Игорь Павлович", LastName = "Пономарёв", FirstName = "Игорь", MiddleName = "Павлович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Селезнёв Роман Олегович", 34m, login: "seleznev.ro48"),
                PersonData.CreateParticipant("Селезнёва Вера Олеговна", 33m, login: "selezneva.vo48"),
                PersonData.CreateParticipant("Селезнёв Максим Олегович", 33m, login: "seleznev.mo48"),
            ]),
        [49] = new(
            Gd: new() { Uid = "ryabov.vs", Login = "ryabov.vs", FullName = "Рябов Владислав Сергеевич", LastName = "Рябов", FirstName = "Владислав", MiddleName = "Сергеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Тарасов Станислав Игоревич", 50m, login: "tarasov.si49"),
                PersonData.CreateParticipant("Тарасова Алиса Игоревна", 50m, login: "tarasova.ai49"),
            ]),
        [50] = new(
            Gd: new() { Uid = "savelyev.ro", Login = "savelyev.ro", FullName = "Савельев Ринат Олегович", LastName = "Савельев", FirstName = "Ринат", MiddleName = "Олегович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Уваров Пётр Вадимович", 60m, login: "uvarov.pv50"),
                PersonData.CreateParticipant("Уварова Наталья Вадимовна", 40m, login: "uvarova.nv50"),
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
        char ExecutiveBodyType,
        PersonData AdminUser);

    /// <summary>Данные лица (LDAP-пользователь или участник).</summary>
    public sealed record PersonData
    {
        /// <summary>UID в LDAP (пусто для участников без LDAP).</summary>
        public string Uid { get; init; } = string.Empty;

        /// <summary>Логин для UI (фамилия.инициалы, транскрипция с русского).</summary>
        public string Login { get; init; } = string.Empty;

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

        /// <summary>Создать участника (без LDAP, только ФИО + доля + логин).</summary>
        public static PersonData CreateParticipant(string fullName, decimal sharePercent, bool isDirector = false, string login = "") =>
            new() { FullName = fullName, SharePercent = sharePercent, IsDirector = isDirector, Login = login };

        /// <summary>Создать администратора ЮЛ (LDAP-пользователь с ролью LE_ADMIN).</summary>
        public static PersonData CreateAdmin(string uid, string fullName, string lastName, string firstName, string middleName, string login) =>
            new() { Uid = uid, Login = login, FullName = fullName, LastName = lastName, FirstName = firstName, MiddleName = middleName, Position = "Администратор" };
    }

    /// <summary>Набор лиц для одного ЮЛ.</summary>
    public sealed record EntityPersons(PersonData? Gd, IReadOnlyList<PersonData> Participants);
}
