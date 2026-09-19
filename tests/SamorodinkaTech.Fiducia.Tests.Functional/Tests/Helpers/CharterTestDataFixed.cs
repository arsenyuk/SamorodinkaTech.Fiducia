namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Фиксированный набор данных для E2E-тестов уставов ООО.
/// Содержит 36 типовых + 14 индивидуальных уставов с привязанными ЮЛ и лицами.
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
    public const string RoleParticipant = "PARTICIPANT";

    // ══════════════════════════════════════════════════════════════════════
    // Данные записей
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Фиксированный список всех юридических лиц (36 типовых + 14 индивидуальных).</summary>
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

        // ── 7 моделей ЕИО: индивидуальный устав (номера 51–57) ────────────
        // Модель 1: ГД — наёмный сотрудник (Type A)
        new(51,  "Общество с ограниченной ответственностью «Тарасов Инвест»", "7815987654", "2159876543212", "ООО «ТИ»",  ExecutiveBodyA, PersonData.CreateAdmin("tarasov.ev", "Тарасов Евгений Владимирович", "Тарасов", "Евгений", "Владимирович", "tarasov.ev")),
        // Модель 2: ГД — участник общества (Type A)
        new(52,  "Общество с ограниченной ответственностью «Уваров Тех»", "7816987654", "2169876543212", "ООО «УТ»",  ExecutiveBodyA, PersonData.CreateAdmin("uvarov.di", "Уваров Дмитрий Игоревич", "Уваров", "Дмитрий", "Игоревич", "uvarov.di")),
        // Модель 3: Управляющий — ИП (Type D)
        new(53,  "Общество с ограниченной ответственностью «Фёдоров Групп»", "7817987654", "2179876543212", "ООО «ФГ»",  'D', PersonData.CreateAdmin("fedorov.ka", "Фёдоров Кирилл Андреевич", "Фёдоров", "Кирилл", "Андреевич", "fedorov.ka")),
        // Модель 4: Управляющая организация (Type E)
        new(54,  "Общество с ограниченной ответственностью «Хромов Лабс»", "7818987654", "2189876543212", "ООО «ХЛ»",  'E', PersonData.CreateAdmin("khromov.ni", "Хромов Никита Игоревич", "Хромов", "Никита", "Игоревич", "khromov.ni")),
        // Модель 5: Все участники — директора (Type B)
        new(55,  "Общество с ограниченной ответственностью «Цветков Продакшн»", "7819987654", "2199876543212", "ООО «ЦП»",  ExecutiveBodyB, PersonData.CreateAdmin("tsvetkov.sa", "Цветков Станислав Андреевич", "Цветков", "Станислав", "Андреевич", "tsvetkov.sa")),
        // Модель 6: Все участники совместно (Type C)
        new(56,  "Общество с ограниченной ответственностью «Шестаков Финанс»", "7820987654", "2209876543212", "ООО «ШФ»",  ExecutiveBodyC, PersonData.CreateAdmin("shestakov.vi", "Шестаков Виктор Игоревич", "Шестаков", "Виктор", "Игоревич", "shestakov.vi")),
        // Модель 7: Несколько ЕИО (Type F)
        new(57,  "Общество с ограниченной ответственностью «Щербаков Консалтинг»", "7821987654", "2219876543212", "ООО «ЩК»",  'F', PersonData.CreateAdmin("shcherbakov.am", "Щербаков Артём Максимович", "Щербаков", "Артём", "Максимович", "shcherbakov.am")),

        // ── Вкладка «ГД»: тесты назначения генерального директора (58–63) ────
        // ГД — участник общества (Type A, индивидуальный устав)
        new(58,  "Общество с ограниченной ответственностью «Якушев Технолоджиз»", "7822987654", "2229876543212", "ООО «ЯТ»",  ExecutiveBodyA, PersonData.CreateAdmin("yakushev.di", "Якушев Денис Игоревич", "Якушев", "Денис", "Игоревич", "yakushev.di")),
        // ГД — участник общества (Type A, индивидуальный устав, 2 участника)
        new(59,  "Общество с ограниченной ответственностью «Абрамов Финанс»", "7823987654", "2239876543212", "ООО «АФ»",  ExecutiveBodyA, PersonData.CreateAdmin("abramov.sa", "Абрамов Сергей Александрович", "Абрамов", "Сергей", "Александрович", "abramov.sa")),
        // ГД — индивидуальный устав, ExecBody=A, сохранение с СНИЛС
        new(60,  "Общество с ограниченной ответственностью «Баранов Лабс»", "7824987654", "2249876543212", "ООО «БЛ»",  ExecutiveBodyA, PersonData.CreateAdmin("baranov.pk", "Баранов Пётр Кириллович", "Баранов", "Пётр", "Кириллович", "baranov.pk")),
        // ГД — ExecBody=B (не A), вкладка ГД НЕ должна отображаться
        new(61,  "Общество с ограниченной ответственностью «Виноградов Сервис»", "7825987654", "2259876543212", "ООО «ВС»",  ExecutiveBodyB, PersonData.CreateAdmin("vinogradov.ma", "Виноградов Максим Андреевич", "Виноградов", "Максим", "Андреевич", "vinogradov.ma")),
        // ГД — ExecBody=C (не A), вкладка ГД НЕ должна отображаться
        new(62,  "Общество с ограниченной ответственностью «Громов Инвест»", "7826987654", "2269876543212", "ООО «ГИ»",  ExecutiveBodyC, PersonData.CreateAdmin("gromov.nv", "Громов Никита Владимирович", "Громов", "Никита", "Владимирович", "gromov.nv")),
        // ГД — типовой устав с ExecBody=A, вкладка ГД должна отображаться
        new(63,  "Общество с ограниченной ответственностью «Демидов Продакшн»", "7827987654", "2279876543212", "ООО «ДП»",  ExecutiveBodyA, PersonData.CreateAdmin("demidov.oi", "Демидов Олег Игоревич", "Демидов", "Олег", "Игоревич", "demidov.oi")),

        // ── Первичный ввод состава СД: Вариант 1 — только Председатель (64) ────
        new(64,  "Общество с ограниченной ответственностью «Ершов Технолоджиз»", "7828987654", "2289876543212", "ООО «ЕТ»",  ExecutiveBodyA, PersonData.CreateAdmin("ershov.di", "Ершов Денис Игоревич", "Ершов", "Денис", "Игоревич", "ershov.di")),
        // ── Первичный ввод состава СД: Вариант 2 — Председатель + Зам. председателя (65) ────
        new(65,  "Общество с ограниченной ответственностью «Жуков Консалтинг»", "7829987654", "2299876543212", "ООО «ЖК»",  ExecutiveBodyA, PersonData.CreateAdmin("zhukov.sa", "Жуков Станислав Андреевич", "Жуков", "Станислав", "Андреевич", "zhukov.sa")),
        // ── Первичный ввод состава СД: Вариант 3 — Председатель + Секретарь (66) ────
        new(66,  "Общество с ограниченной ответственностью «Зимин Финанс»", "7830987654", "2309876543212", "ООО «ЗФ»",  ExecutiveBodyA, PersonData.CreateAdmin("zimin.pk", "Зимин Пётр Кириллович", "Зимин", "Пётр", "Кириллович", "zimin.pk")),
        // ── Требование участника о созыве ВОСУ (67) ────
        new(67,  "Общество с ограниченной ответственностью «Иванов Трейд»", "7831987654", "2319876543212", "ООО «ИТ»",  ExecutiveBodyA, PersonData.CreateAdmin("ivanov.tm", "Иванов Тимур Романович", "Иванов", "Тимур", "Романович", "ivanov.tm")),
        // ── Изменение сведений участника / версионирование ДУЛ (68) ────
        new(68,  "Общество с ограниченной ответственностью «Казаков и Partners»", "7832987654", "2329876543212", "ООО «К&P»",  ExecutiveBodyA, PersonData.CreateAdmin("kazakov.nv", "Казаков Николай Викторович", "Казаков", "Николай", "Викторович", "kazakov.nv")),
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
                PersonData.CreateParticipant("Жиров Антон Тарасович", 60m, login: "zhirov.at") with { Uid="zhirov.at" },
                PersonData.CreateParticipant("Жирова Елена Тарасовна", 40m, login: "zhirova.et") with { Uid="zhirova.et" },
            ]),
        [2] = new(
            Gd: new() { Uid = "sobolev.dn", Login = "sobolev.dn", FullName = "Соболев Дмитрий Николаевич", LastName = "Соболев", FirstName = "Дмитрий", MiddleName = "Николаевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Бирюков Олег Степанович", 34m, login: "birukov.os") with { Uid="birukov.os" },
                PersonData.CreateParticipant("Бирюкова Ирина Степановна", 33m, login: "birukova.is") with { Uid="birukova.is" },
                PersonData.CreateParticipant("Бирюков Станислав Степанович", 33m, login: "birukov.ss") with { Uid="birukov.ss" },
            ]),
        [3] = new(
            Gd: new() { Uid = "tokarev.as", Login = "tokarev.as", FullName = "Токарев Андрей Сергеевич", LastName = "Токарев", FirstName = "Андрей", MiddleName = "Сергеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Вешняков Максим Юрьевич", 100m, login: "veshnyakov.my") with { Uid="veshnyakov.my" },
            ]),
        [4] = new(
            Gd: new() { Uid = "ermakov.ia", Login = "ermakov.ia", FullName = "Ермаков Игорь Александрович", LastName = "Ермаков", FirstName = "Игорь", MiddleName = "Александрович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Зубов Владимир Игоревич", 50m, login: "zubov.vi") with { Uid="zubov.vi" },
                PersonData.CreateParticipant("Зубова Наталья Игоревна", 50m, login: "zubova.ni") with { Uid="zubova.ni" },
            ]),
        [5] = new(
            Gd: new() { Uid = "larionov.sp", Login = "larionov.sp", FullName = "Ларионов Сергей Павлович", LastName = "Ларионов", FirstName = "Сергей", MiddleName = "Павлович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Прохоров Алексей Викторович", 25m, login: "prokhorev.av") with { Uid="prokhorev.av" },
                PersonData.CreateParticipant("Прохорова Ольга Викторовна", 25m, login: "prokhoreva.ov") with { Uid="prokhoreva.ov" },
                PersonData.CreateParticipant("Прохоров Пётр Викторович", 50m, login: "prokhorev.pv") with { Uid="prokhorev.pv" },
            ]),
        [6] = new(
            Gd: new() { Uid = "savelyev.rt", Login = "savelyev.rt", FullName = "Савельев Роман Тимурович", LastName = "Савельев", FirstName = "Роман", MiddleName = "Тимурович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Широков Денис Александрович", 60m, login: "shirokov.da") with { Uid="shirokov.da" },
                PersonData.CreateParticipant("Широкова Мария Александровна", 40m, login: "shirokova.ma") with { Uid="shirokova.ma" },
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 07–12 (ExecutiveBody B): участники = ЕИО
        // ════════════════════════════════════════════════════════════════
        [7] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Комаров Степан Андреевич", 60m, isDirector: true, login: "komarov.sa") with { Uid="komarov.sa" },
                PersonData.CreateParticipant("Комарова Вера Андреевна", 40m, isDirector: true, login: "komarova.va") with { Uid="komarova.va" },
            ]),
        [8] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Лапин Дмитрий Сергеевич", 34m, isDirector: true, login: "lapin.ds") with { Uid="lapin.ds" },
                PersonData.CreateParticipant("Лапина Анна Сергеевна", 33m, isDirector: true, login: "lapina.as") with { Uid="lapina.as" },
                PersonData.CreateParticipant("Лапин Игорь Сергеевич", 33m, isDirector: true, login: "lapin.is") with { Uid="lapin.is" },
            ]),
        [9] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Рябов Николай Вадимович", 100m, isDirector: true, login: "ryabov.nv") with { Uid="ryabov.nv" },
            ]),
        [10] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Евстигнеев Павел Данилович", 50m, isDirector: true, login: "evstignxeev.pd") with { Uid="evstignxeev.pd" },
                PersonData.CreateParticipant("Евстигнеева Татьяна Даниловна", 50m, isDirector: true, login: "evstignxeeva.td") with { Uid="evstignxeeva.td" },
            ]),
        [11] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Калачёв Ринат Александрович", 25m, isDirector: true, login: "kalachev.ra") with { Uid="kalachev.ra" },
                PersonData.CreateParticipant("Калачёва Светлана Александровна", 25m, isDirector: true, login: "kalacheva.sa") with { Uid="kalacheva.sa" },
                PersonData.CreateParticipant("Калачёв Тимур Александрович", 50m, isDirector: true, login: "kalachev.ta") with { Uid="kalachev.ta" },
            ]),
        [12] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Горбунов Евгений Леонидович", 60m, isDirector: true, login: "gorbunov.el") with { Uid="gorbunov.el" },
                PersonData.CreateParticipant("Горбунова Марина Леонидовна", 40m, isDirector: true, login: "gorbunova.ml") with { Uid="gorbunova.ml" },
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 13–18 (ExecutiveBody C): участники = ЕИО совместно
        // ════════════════════════════════════════════════════════════════
        [13] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Шестаков Пётр Николаевич", 60m, isDirector: true, login: "shestakov.pn") with { Uid="shestakov.pn" },
                PersonData.CreateParticipant("Шестакова Лариса Николаевна", 40m, isDirector: true, login: "shestakova.ln") with { Uid="shestakova.ln" },
            ]),
        [14] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Суханов Илья Максимович", 34m, isDirector: true, login: "sukhanov.im") with { Uid="sukhanov.im" },
                PersonData.CreateParticipant("Суханова Екатерина Максимовна", 33m, isDirector: true, login: "sukhanova.em") with { Uid="sukhanova.em" },
                PersonData.CreateParticipant("Суханов Кирилл Максимович", 33m, isDirector: true, login: "sukhanov.km") with { Uid="sukhanov.km" },
            ]),
        [15] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Бельский Глеб Андреевич", 100m, isDirector: true, login: "belsky.ga") with { Uid="belsky.ga" },
            ]),
        [16] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Головин Святослав Алексеевич", 50m, isDirector: true, login: "golovin.sa") with { Uid="golovin.sa" },
                PersonData.CreateParticipant("Головина Надежда Алексеевна", 50m, isDirector: true, login: "golovina.na") with { Uid="golovina.na" },
            ]),
        [17] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Денисов Владислав Юрьевич", 25m, isDirector: true, login: "denisov.vy") with { Uid="denisov.vy" },
                PersonData.CreateParticipant("Денисова Ксения Юрьевна", 25m, isDirector: true, login: "denisova.ky") with { Uid="denisova.ky" },
                PersonData.CreateParticipant("Денисов Роман Юрьевич", 50m, isDirector: true, login: "denisov.ry") with { Uid="denisov.ry" },
            ]),
        [18] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Ершов Тимур Сергеевич", 60m, isDirector: true, login: "ershov.ts") with { Uid="ershov.ts" },
                PersonData.CreateParticipant("Ершова Алиса Сергеевна", 40m, isDirector: true, login: "ershova.as") with { Uid="ershova.as" },
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 19–24 (ExecutiveBody A): ГД отдельно + участники
        // ════════════════════════════════════════════════════════════════
        [19] = new(
            Gd: new() { Uid = "likhachev.gv", Login = "likhachev.gv", FullName = "Лихачёв Глеб Викторович", LastName = "Лихачёв", FirstName = "Глеб", MiddleName = "Викторович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Фролов Станислав Андреевич", 60m, login: "frolov.sa") with { Uid="frolov.sa" },
                PersonData.CreateParticipant("Фролова Виктория Андреевна", 40m, login: "frolova.va") with { Uid="frolova.va" },
            ]),
        [20] = new(
            Gd: new() { Uid = "matveev.yi", Login = "matveev.yi", FullName = "Матвеев Ярослав Игоревич", LastName = "Матвеев", FirstName = "Ярослав", MiddleName = "Игоревич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Харитонов Семён Павлович", 34m, login: "kharitonov.sp") with { Uid="kharitonov.sp" },
                PersonData.CreateParticipant("Харитонова Дарья Павловна", 33m, login: "kharitonova.dp") with { Uid="kharitonova.dp" },
                PersonData.CreateParticipant("Харитонов Илья Павлович", 33m, login: "kharitonov.ip") with { Uid="kharitonov.ip" },
            ]),
        [21] = new(
            Gd: new() { Uid = "noskov.vs", Login = "noskov.vs", FullName = "Носков Виталий Сергеевич", LastName = "Носков", FirstName = "Виталий", MiddleName = "Сергеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Цветков Михаил Евгеньевич", 100m, login: "tsvetkov.me") with { Uid="tsvetkov.me" },
            ]),
        [22] = new(
            Gd: new() { Uid = "ovchinnikov.so", Login = "ovchinnikov.so", FullName = "Овчинников Святослав Олегович", LastName = "Овчинников", FirstName = "Святослав", MiddleName = "Олегович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Чесноков Денис Валерьевич", 50m, login: "chesnokov.dv") with { Uid="chesnokov.dv" },
                PersonData.CreateParticipant("Чеснокова Оксана Валерьевна", 50m, login: "chesnokova.ov") with { Uid="chesnokova.ov" },
            ]),
        [23] = new(
            Gd: new() { Uid = "pustyrnikov.iy", Login = "pustyrnikov.iy", FullName = "Пустырников Игорь Юрьевич", LastName = "Пустырников", FirstName = "Игорь", MiddleName = "Юрьевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Шульга Тарас Игоревич", 25m, login: "shulga.ti") with { Uid="shulga.ti" },
                PersonData.CreateParticipant("Шульга Марина Игоревна", 25m, login: "shulga.mi") with { Uid="shulga.mi" },
                PersonData.CreateParticipant("Шульга Алексей Игоревич", 50m, login: "shulga.ai") with { Uid="shulga.ai" },
            ]),
        [24] = new(
            Gd: new() { Uid = "rtishchev.aa", Login = "rtishchev.aa", FullName = "Ртищев Аркадий Андреевич", LastName = "Ртищев", FirstName = "Аркадий", MiddleName = "Андреевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Юдин Кирилл Леонидович", 60m, login: "yudin.kl") with { Uid="yudin.kl" },
                PersonData.CreateParticipant("Юдинова Анна Леонидовна", 40m, login: "yudinova.al") with { Uid="yudinova.al" },
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 25–30 (ExecutiveBody B): участники = ЕИО
        // ════════════════════════════════════════════════════════════════
        [25] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Абрамов Роман Викторович", 60m, isDirector: true, login: "abramov.rv") with { Uid="abramov.rv" },
                PersonData.CreateParticipant("Абрамова Ирина Викторовна", 40m, isDirector: true, login: "abramova.iv") with { Uid="abramova.iv" },
            ]),
        [26] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Блинов Степан Андреевич", 34m, isDirector: true, login: "blinov.sa") with { Uid="blinov.sa" },
                PersonData.CreateParticipant("Блинова Татьяна Андреевна", 33m, isDirector: true, login: "blinova.ta") with { Uid="blinova.ta" },
                PersonData.CreateParticipant("Блинов Артём Андреевич", 33m, isDirector: true, login: "blinov.aa") with { Uid="blinov.aa" },
            ]),
        [27] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Виноградов Пётр Дмитриевич", 100m, isDirector: true, login: "vinogradov.pd") with { Uid="vinogradov.pd" },
            ]),
        [28] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Громов Ринат Игоревич", 50m, isDirector: true, login: "gromov.ri") with { Uid="gromov.ri" },
                PersonData.CreateParticipant("Громова Елена Игоревна", 50m, isDirector: true, login: "gromova.ei") with { Uid="gromova.ei" },
            ]),
        [29] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Демидов Илья Сергеевич", 25m, isDirector: true, login: "demidov.is") with { Uid="demidov.is" },
                PersonData.CreateParticipant("Демидова Кристина Сергеевна", 25m, isDirector: true, login: "demidova.ks") with { Uid="demidova.ks" },
                PersonData.CreateParticipant("Демидов Тимур Сергеевич", 50m, isDirector: true, login: "demidov.ts") with { Uid="demidov.ts" },
            ]),
        [30] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Ермаков Святослав Павлович", 60m, isDirector: true, login: "ermakov.sp") with { Uid="ermakov.sp" },
                PersonData.CreateParticipant("Ермакова Виктория Павловна", 40m, isDirector: true, login: "ermakova.vp") with { Uid="ermakova.vp" },
            ]),

        // ════════════════════════════════════════════════════════════════
        // Типовые уставы 31–36 (ExecutiveBody C): участники = ЕИО совместно
        // ════════════════════════════════════════════════════════════════
        [31] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Жуков Денис Викторович", 60m, isDirector: true, login: "zhukov.dv") with { Uid="zhukov.dv" },
                PersonData.CreateParticipant("Жукова Светлана Викторовна", 40m, isDirector: true, login: "zhukova.sv") with { Uid="zhukova.sv" },
            ]),
        [32] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Зимовец Илья Сергеевич", 34m, isDirector: true, login: "zimovec.is") with { Uid="zimovec.is" },
                PersonData.CreateParticipant("Зимовец Анна Сергеевна", 33m, isDirector: true, login: "zimovec.as") with { Uid="zimovec.as" },
                PersonData.CreateParticipant("Зимовец Кирилл Сергеевич", 33m, isDirector: true, login: "zimovec.ks") with { Uid="zimovec.ks" },
            ]),
        [33] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Казаков Николай Олегович", 100m, isDirector: true, login: "kazakov.no") with { Uid="kazakov.no" },
            ]),
        [34] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Ларин Ринат Андреевич", 50m, isDirector: true, login: "larin.ra") with { Uid="larin.ra" },
                PersonData.CreateParticipant("Ларина Ольга Андреевна", 50m, isDirector: true, login: "larina.oa") with { Uid="larina.oa" },
            ]),
        [35] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Мещеряков Артём Павлович", 25m, isDirector: true, login: "meshcheryakov.ap") with { Uid="meshcheryakov.ap" },
                PersonData.CreateParticipant("Мещерякова Елена Павловна", 25m, isDirector: true, login: "meshcheryakova.ep") with { Uid="meshcheryakova.ep" },
                PersonData.CreateParticipant("Мещеряков Владислав Павлович", 50m, isDirector: true, login: "meshcheryakov.vp") with { Uid="meshcheryakov.vp" },
            ]),
        [36] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Некрасов Дмитрий Викторович", 60m, isDirector: true, login: "nekrasov.dv") with { Uid="nekrasov.dv" },
                PersonData.CreateParticipant("Некрасова Мария Викторовна", 40m, isDirector: true, login: "nekrasova.mv") with { Uid="nekrasova.mv" },
            ]),

        // ════════════════════════════════════════════════════════════════
        // Нетиповые уставы 37–50 (ExecutiveBody A): ГД + участники
        // ════════════════════════════════════════════════════════════════
        [37] = new(
            Gd: new() { Uid = "garin.sa", Login = "garin.sa", FullName = "Гарин Станислав Андреевич", LastName = "Гарин", FirstName = "Станислав", MiddleName = "Андреевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Ельцов Игорь Вадимович", 60m, login: "eltsov.iv") with { Uid="eltsov.iv" },
                PersonData.CreateParticipant("Ельцов Вера Вадимовна", 40m, login: "eltsova.vv") with { Uid="eltsova.vv" },
            ]),
        [38] = new(
            Gd: new() { Uid = "dementiev.rs", Login = "dementiev.rs", FullName = "Дементьев Роман Сергеевич", LastName = "Дементьев", FirstName = "Роман", MiddleName = "Сергеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Зубков Семён Александрович", 34m, login: "zubkov.sa") with { Uid="zubkov.sa" },
                PersonData.CreateParticipant("Зубкова Татьяна Александровна", 33m, login: "zubkova.ta") with { Uid="zubkova.ta" },
                PersonData.CreateParticipant("Зубков Аркадий Александрович", 33m, login: "zubkov.aa") with { Uid="zubkov.aa" },
            ]),
        [39] = new(
            Gd: new() { Uid = "efimov.yd", Login = "efimov.yd", FullName = "Ефимов Ярослав Дмитриевич", LastName = "Ефимов", FirstName = "Ярослав", MiddleName = "Дмитриевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Ильин Максим Викторович", 100m, login: "ilin.mv") with { Uid="ilin.mv" },
            ]),
        [40] = new(
            Gd: new() { Uid = "zhukov.dp", Login = "zhukov.dp", FullName = "Жуков Даниил Павлович", LastName = "Жуков", FirstName = "Даниил", MiddleName = "Павлович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Корнеев Алексей Сергеевич", 50m, login: "korneev.as") with { Uid="korneev.as" },
                PersonData.CreateParticipant("Корнеева Наталья Сергеевна", 50m, login: "korneeva.ns") with { Uid="korneeva.ns" },
            ]),
        [41] = new(
            Gd: new() { Uid = "zaitsev.so", Login = "zaitsev.so", FullName = "Зайцев Святослав Олегович", LastName = "Зайцев", FirstName = "Святослав", MiddleName = "Олегович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Лебедев Тарас Валерьевич", 25m, login: "lebedev.tv") with { Uid="lebedev.tv" },
                PersonData.CreateParticipant("Лебедева Ольга Валерьевна", 25m, login: "lebedeva.ov") with { Uid="lebedeva.ov" },
                PersonData.CreateParticipant("Лебедев Пётр Валерьевич", 50m, login: "lebedev.pv") with { Uid="lebedev.pv" },
            ]),
        [42] = new(
            Gd: new() { Uid = "ilin.vn", Login = "ilin.vn", FullName = "Ильин Владислав Николаевич", LastName = "Ильин", FirstName = "Владислав", MiddleName = "Николаевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Мельников Станислав Дмитриевич", 60m, login: "melnikov.sd") with { Uid="melnikov.sd" },
                PersonData.CreateParticipant("Мельникова Виктория Дмитриевна", 40m, login: "melnikova.vd") with { Uid="melnikova.vd" },
            ]),
        [43] = new(
            Gd: new() { Uid = "kozlov.ra", Login = "kozlov.ra", FullName = "Козлов Ринат Алексеевич", LastName = "Козлов", FirstName = "Ринат", MiddleName = "Алексеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Носков Семён Олегович", 34m, login: "noskov.so") with { Uid="noskov.so" },
                PersonData.CreateParticipant("Носкова Анна Олеговна", 33m, login: "noskova.ao") with { Uid="noskova.ao" },
                PersonData.CreateParticipant("Носков Игорь Олегович", 33m, login: "noskov.io") with { Uid="noskov.io" },
            ]),
        [44] = new(
            Gd: new() { Uid = "larionov.si", Login = "larionov.si", FullName = "Ларионов Станислав Игоревич", LastName = "Ларионов", FirstName = "Станислав", MiddleName = "Игоревич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Овсов Роман Андреевич", 50m, login: "ovsov.ra") with { Uid="ovsov.ra" },
                PersonData.CreateParticipant("Овсова Марина Андреевна", 50m, login: "ovsova.ma") with { Uid="ovsova.ma" },
            ]),
        [45] = new(
            Gd: new() { Uid = "mikhailov.as", Login = "mikhailov.as", FullName = "Михайлов Артём Сергеевич", LastName = "Михайлов", FirstName = "Артём", MiddleName = "Сергеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Павлов Святослав Викторович", 60m, login: "pavlov.sv") with { Uid="pavlov.sv" },
                PersonData.CreateParticipant("Павлова Елена Викторовна", 40m, login: "pavlova.ev") with { Uid="pavlova.ev" },
            ]),
        [46] = new(
            Gd: new() { Uid = "nechaev.dv", Login = "nechaev.dv", FullName = "Нечаев Данил Вадимович", LastName = "Нечаев", FirstName = "Данил", MiddleName = "Вадимович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Рогов Илья Николаевич", 60m, login: "rogov.in") with { Uid="rogov.in" },
                PersonData.CreateParticipant("Рогова Ксения Николаевна", 40m, login: "rogova.kn") with { Uid="rogova.kn" },
            ]),
        [47] = new(
            Gd: new() { Uid = "ovchinnikov.tr", Login = "ovchinnikov.tr", FullName = "Овчинников Тимур Романович", LastName = "Овчинников", FirstName = "Тимур", MiddleName = "Романович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Сафонов Аркадий Сергеевич", 60m, login: "safonov.as") with { Uid="safonov.as" },
                PersonData.CreateParticipant("Сафонова Дарья Сергеевна", 40m, login: "safonova.ds") with { Uid="safonova.ds" },
            ]),
        [48] = new(
            Gd: new() { Uid = "ponomarev.ip", Login = "ponomarev.ip", FullName = "Пономарёв Игорь Павлович", LastName = "Пономарёв", FirstName = "Игорь", MiddleName = "Павлович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Селезнёв Роман Олегович", 34m, login: "seleznev.ro") with { Uid="seleznev.ro" },
                PersonData.CreateParticipant("Селезнёва Вера Олеговна", 33m, login: "selezneva.vo") with { Uid="selezneva.vo" },
                PersonData.CreateParticipant("Селезнёв Максим Олегович", 33m, login: "seleznev.mo") with { Uid="seleznev.mo" },
            ]),
        [49] = new(
            Gd: new() { Uid = "ryabov.vs", Login = "ryabov.vs", FullName = "Рябов Владислав Сергеевич", LastName = "Рябов", FirstName = "Владислав", MiddleName = "Сергеевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Тарасов Станислав Игоревич", 50m, login: "tarasov.si") with { Uid="tarasov.si" },
                PersonData.CreateParticipant("Тарасова Алиса Игоревна", 50m, login: "tarasova.ai") with { Uid="tarasova.ai" },
            ]),
        [50] = new(
            Gd: new() { Uid = "savelyev.ro", Login = "savelyev.ro", FullName = "Савельев Ринат Олегович", LastName = "Савельев", FirstName = "Ринат", MiddleName = "Олегович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Уваров Пётр Вадимович", 60m, login: "uvarov.pv") with { Uid="uvarov.pv" },
                PersonData.CreateParticipant("Уварова Наталья Вадимовна", 40m, login: "uvarova.nv") with { Uid="uvarova.nv" },
            ]),

        // ════════════════════════════════════════════════════════════════
        // 7 моделей ЕИО: индивидуальный устав (51–57)
        // ════════════════════════════════════════════════════════════════

        // Модель 1: ГД — наёмный сотрудник (Type A, ГД не участник)
        [51] = new(
            Gd: new() { Uid = "tarasov.ev", Login = "tarasov.ev", FullName = "Тарасов Евгений Владимирович", LastName = "Тарасов", FirstName = "Евгений", MiddleName = "Владимирович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Андреев Павел Сергеевич", 60m, login: "andreev.ps") with { Uid="andreev.ps" },
                PersonData.CreateParticipant("Андреева Ольга Сергеевна", 40m, login: "andreeva.os") with { Uid="andreeva.os" },
            ]),

        // Модель 2: ГД — участник общества (Type A, ГД = участник)
        [52] = new(
            Gd: new() { Uid = "uvarov.di", Login = "uvarov.di", FullName = "Уваров Дмитрий Игоревич", LastName = "Уваров", FirstName = "Дмитрий", MiddleName = "Игоревич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipant("Борисов Алексей Николаевич", 50m, login: "borisov.an") with { Uid="borisov.an" },
                PersonData.CreateParticipant("Борисова Елена Николаевна", 50m, login: "borisova.en") with { Uid="borisova.en" },
            ]),

        // Модель 3: Управляющий — ИП (Type D)
        [53] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Волков Максим Андреевич", 60m, login: "volkov.ma") with { Uid="volkov.ma" },
                PersonData.CreateParticipant("Волкова Ирина Андреевна", 40m, login: "volkova.ia") with { Uid="volkova.ia" },
            ]),

        // Модель 4: Управляющая организация (Type E)
        [54] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Григорьев Денис Викторович", 50m, login: "grigoriev.dv") with { Uid="grigoriev.dv" },
                PersonData.CreateParticipant("Григорьева Анна Викторовна", 50m, login: "grigorieva.av") with { Uid="grigorieva.av" },
            ]),

        // Модель 5: Все участники — директора (Type B)
        [55] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Давыдов Роман Сергеевич", 50m, isDirector: true, login: "davydov.rs") with { Uid="davydov.rs" },
                PersonData.CreateParticipant("Давыдова Ксения Сергеевна", 50m, isDirector: true, login: "davydova.ks") with { Uid="davydova.ks" },
            ]),

        // Модель 6: Все участники совместно (Type C)
        [56] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Егоров Тимур Александрович", 60m, isDirector: true, login: "egorov.ta") with { Uid="egorov.ta" },
                PersonData.CreateParticipant("Егорова Виктория Александровна", 40m, isDirector: true, login: "egorova.va") with { Uid="egorova.va" },
            ]),

        // Модель 7: Несколько ЕИО (Type F)
        [57] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipant("Жданов Илья Павлович", 50m, isDirector: true, login: "zhdanov.ip") with { Uid="zhdanov.ip" },
                PersonData.CreateParticipant("Жданова Мария Павловна", 50m, isDirector: true, login: "zhdanova.mp") with { Uid="zhdanova.mp" },
            ]),

        // ── Вкладка «ГД»: тесты назначения генерального директора (58–63) ────
        // ГД — участник общества (1 участник)
        [58] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipantWithDul("Якушев Денис Игоревич", 100m,
                    "21", "4510", "123456", login: "yakushev.di") with { Uid = "yakushev.di" },
            ]),
        // ГД — участник общества (2 участника)
        [59] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipantWithDul("Абрамов Сергей Александрович", 60m,
                    "21", "4520", "234567", login: "abramov.sa") with { Uid = "abramov.sa" },
                PersonData.CreateParticipantWithDul("Абрамова Елена Петровна", 40m,
                    "21", "4530", "345678", login: "abramova.ep") with { Uid="abramova.ep" },
            ]),
        // ГД — сохранение с СНИЛС
        [60] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipantWithDul("Баранов Пётр Кириллович", 100m,
                    "21", "4540", "456789", login: "baranov.pk") with { Uid = "baranov.pk" },
            ]),
        // ExecBody=B — вкладка ГД НЕ отображается
        [61] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipantWithDul("Виноградов Максим Андреевич", 50m,
                    "21", "4550", "567890", login: "vinogradov.ma") with { Uid = "vinogradov.ma" },
                PersonData.CreateParticipantWithDul("Виноградова Ольга Игоревна", 50m,
                    "21", "4560", "678901", login: "vinogradova.oi") with { Uid="vinogradova.oi" },
            ]),
        // ExecBody=C — вкладка ГД НЕ отображается
        [62] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipantWithDul("Громов Никита Владимирович", 50m,
                    "21", "4570", "789012", login: "gromov.nv") with { Uid = "gromov.nv" },
                PersonData.CreateParticipantWithDul("Громова Анна Сергеевна", 50m,
                    "21", "4580", "890123", login: "gromova.as") with { Uid="gromova.as" },
            ]),
        // Типовой устав с ExecBody=A — вкладка ГД отображается
        [63] = new(
            Gd: null,
            Participants:
            [
                PersonData.CreateParticipantWithDul("Демидов Олег Игоревич", 100m,
                    "21", "4590", "901234", login: "demidov.oi") with { Uid = "demidov.oi" },
            ]),

        // ════════════════════════════════════════════════════════════════
        // Первичный ввод состава СД (64–66): индивидуальный устав, ExecBody A
        // ════════════════════════════════════════════════════════════════

        // Вариант 1: только Председатель СД (2 участника)
        [64] = new(
            Gd: new() { Uid = "ershov.di", Login = "ershov.di", FullName = "Ершов Денис Игоревич", LastName = "Ершов", FirstName = "Денис", MiddleName = "Игоревич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipantWithDul("Ершов Денис Игоревич", 60m,
                    "21", "4610", "111111", login: "ershov.di") with { Uid = "ershov.di" },
                PersonData.CreateParticipantWithDul("Ершова Анна Сергеевна", 40m,
                    "21", "4620", "222222", login: "ershova.as") with { Uid="ershova.as" },
            ]),
        // Вариант 2: Председатель + Зам. председателя (3 участника)
        [65] = new(
            Gd: new() { Uid = "zhukov.sa", Login = "zhukov.sa", FullName = "Жуков Станислав Андреевич", LastName = "Жуков", FirstName = "Станислав", MiddleName = "Андреевич", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipantWithDul("Жуков Станислав Андреевич", 50m,
                    "21", "4630", "333333", login: "zhukov.sa") with { Uid = "zhukov.sa" },
                PersonData.CreateParticipantWithDul("Жукова Мария Петровна", 30m,
                    "21", "4640", "444444", login: "zhukova.mp") with { Uid="zhukova.mp" },
                PersonData.CreateParticipantWithDul("Жуков Алексей Иванович", 20m,
                    "21", "4650", "555555", login: "zhukov.ai") with { Uid="zhukov.ai" },
            ]),
        // Вариант 3: Председатель + Секретарь (2 участника)
        [66] = new(
            Gd: new() { Uid = "zimin.pk", Login = "zimin.pk", FullName = "Зимин Пётр Кириллович", LastName = "Зимин", FirstName = "Пётр", MiddleName = "Кириллович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipantWithDul("Зимин Пётр Кириллович", 70m,
                    "21", "4660", "666666", login: "zimin.pk") with { Uid = "zimin.pk" },
                PersonData.CreateParticipantWithDul("Зимина Ольга Дмитриевна", 30m,
                    "21", "4670", "777777", login: "zimina.od") with { Uid="zimina.od" },
            ]),
        // ── Требование участника о созыве ВОСУ (67) ────
        [67] = new(
            Gd: new() { Uid = "ivanov.tm", Login = "ivanov.tm", FullName = "Иванов Тимур Романович", LastName = "Иванов", FirstName = "Тимур", MiddleName = "Романович", Position = "Генеральный директор",
                         DulTypeCode = "21", DulSeries = "4515", DulNumber = "123456" },
            Participants:
            [
                PersonData.CreateParticipantWithDul("Петрова Мария Сергеевна", 40m,
                    "21", "4515", "234567", login: "petrova.ms") with { Uid="petrova.ms" },
            ]),
        // ── Изменение сведений участника / версионирование ДУЛ (68) ────
        [68] = new(
            Gd: new() { Uid = "kazakov.nv", Login = "kazakov.nv", FullName = "Казаков Николай Викторович", LastName = "Казаков", FirstName = "Николай", MiddleName = "Викторович", Position = "Генеральный директор" },
            Participants:
            [
                PersonData.CreateParticipantWithDul("Климов Алексей Петрович", 60m,
                    "21", "4600", "111222", isDirector: true, login: "klimov.ap") with { Uid = "klimov.ap" },
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

        /// <summary>Код типа ДУЛ (21 = паспорт РФ).</summary>
        public string? DulTypeCode { get; init; }

        /// <summary>Серия ДУЛ.</summary>
        public string? DulSeries { get; init; }

        /// <summary>Номер ДУЛ.</summary>
        public string? DulNumber { get; init; }

        /// <summary>Создать участника (без LDAP, только ФИО + доля + логин).</summary>
        public static PersonData CreateParticipant(string fullName, decimal sharePercent, bool isDirector = false, string login = "")
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new()
            {
                FullName = fullName,
                SharePercent = sharePercent,
                IsDirector = isDirector,
                Login = login,
                LastName = parts.Length > 0 ? parts[0] : string.Empty,
                FirstName = parts.Length > 1 ? parts[1] : string.Empty,
                MiddleName = parts.Length > 2 ? parts[2] : string.Empty,
            };
        }

        /// <summary>Создать участника с данными ДУЛ (паспорт).</summary>
        public static PersonData CreateParticipantWithDul(
            string fullName, decimal sharePercent,
            string dulTypeCode, string dulSeries, string dulNumber,
            bool isDirector = false, string login = "")
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new()
            {
                FullName = fullName,
                SharePercent = sharePercent,
                IsDirector = isDirector,
                Login = login,
                LastName = parts.Length > 0 ? parts[0] : string.Empty,
                FirstName = parts.Length > 1 ? parts[1] : string.Empty,
                MiddleName = parts.Length > 2 ? parts[2] : string.Empty,
                DulTypeCode = dulTypeCode,
                DulSeries = dulSeries,
                DulNumber = dulNumber,
            };
        }

        /// <summary>Создать администратора ЮЛ (LDAP-пользователь с ролью LE_ADMIN).</summary>
        public static PersonData CreateAdmin(string uid, string fullName, string lastName, string firstName, string middleName, string login) =>
            new() { Uid = uid, Login = login, FullName = fullName, LastName = lastName, FirstName = firstName, MiddleName = middleName, Position = "Администратор" };
    }

    /// <summary>Набор лиц для одного ЮЛ.</summary>
    public sealed record EntityPersons(PersonData? Gd, IReadOnlyList<PersonData> Participants);
}
