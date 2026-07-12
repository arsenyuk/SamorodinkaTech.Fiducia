-- 02_seed.sql — первичное наполнение (справочники и системные записи)

INSERT INTO ref_roles (id, code, name) VALUES
    ('11111111-1111-1111-1111-111111111111','SYS_ADMIN','Системный администратор'),
    ('22222222-2222-2222-2222-222222222222','CORP_SECRETARY','Корпоративный секретарь'),
    ('33333333-3333-3333-3333-333333333333','CHAIR_BOARD','Председатель СД'),
    ('44444444-4444-4444-4444-444444444444','MEMBER_BOARD','Член СД'),
    ('55555555-5555-5555-5555-555555555555','EXTERNAL_DIRECTOR','Внешний/Независимый директор'),
    ('66666666-6666-6666-6666-666666666666','SHAREHOLDER','Акционер'),
    ('77777777-7777-7777-7777-777777777777','COMMITTEE_CHAIR','Председатель комитета'),
    ('88888888-8888-8888-8888-888888888888','COMMITTEE_MEMBER','Член комитета')
ON CONFLICT (code) DO NOTHING;

-- ОКОПФ (базовые записи)
INSERT INTO ref_okopf(id, code, name) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1','12247','Публичное акционерное общество (ПАО)'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2','12267','Непубличное акционерное общество (НАО)'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3','12260','Акционерное общество (АО)'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4','12300','Общество с ограниченной ответственностью (ООО)'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5','65241','Федеральное государственное унитарное предприятие (ФГУП)'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6','65242','Государственное унитарное предприятие субъекта РФ (ГУП)'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7','65243','Муниципальное унитарное предприятие (МУП)')
ON CONFLICT (code) DO NOTHING;

-- Месяцы
INSERT INTO ref_month(id, code, name) VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc01','01','Январь'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc02','02','Февраль'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc03','03','Март'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc04','04','Апрель'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc05','05','Май'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc06','06','Июнь'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc07','07','Июль'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc08','08','Август'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc09','09','Сентябрь'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc10','10','Октябрь'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc11','11','Ноябрь'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc12','12','Декабрь')
ON CONFLICT (code) DO NOTHING;

-- Форма проведения заседания СД
INSERT INTO ref_meeting_form(id, code, name, short_name) VALUES
    ('ffffffff-ffff-ffff-ffff-fffffffffff1','OCHN','Очное заседание (совместное присутствие)','Очное'),
    ('ffffffff-ffff-ffff-ffff-fffffffffff2','ZAOCHN','Заочное голосование (опросным путём)','Заочное'),
    ('ffffffff-ffff-ffff-ffff-fffffffffff3','MIXED','Смешанное (очное заседание + заочное голосование)','Смешанное')
ON CONFLICT (code) DO NOTHING;

-- Форма проведения ОСА
INSERT INTO ref_osa_form(id, code, name, short_name) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1','GOSA','Годовое общее собрание акционеров','ГОСА'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2','VOSA','Внеочередное общее собрание акционеров','ВОСА')
ON CONFLICT (code) DO NOTHING;

-- Типы директоров
INSERT INTO ref_board_member_types(id, code, name) VALUES
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1','EXECUTIVE','Исполнительный директор'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2','NON_EXECUTIVE','Внешний директор'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee3','INDEPENDENT','Независимый директор'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee4','STAFF','Штатный сотрудник')
ON CONFLICT (code) DO NOTHING;

-- Должности в СД
INSERT INTO ref_board_roles(id, code, name, sort_order) VALUES
    ('ffffffff-ffff-ffff-ffff-fffffffffff1','CHAIR','Председатель СД',1),
    ('ffffffff-ffff-ffff-ffff-fffffffffff2','DEPUTY_CHAIR','Заместитель председателя',2),
    ('ffffffff-ffff-ffff-ffff-fffffffffff3','MEMBER','Член СД',3),
    ('ffffffff-ffff-ffff-ffff-fffffffffff4','TEMP_CHAIR','Временный председательствующий',4),
    ('ffffffff-ffff-ffff-ffff-fffffffffff5','TEMP_SECRETARY','Временный секретарь',5),
    ('ffffffff-ffff-ffff-ffff-fffffffffff6','SECRETARY','Секретарь СД',6)
ON CONFLICT (code) DO NOTHING;

-- Статусы Совета директоров
INSERT INTO ref_board_of_directors_statuses(id, code, name) VALUES
    ('99999999-9999-9999-9999-999999999991','DRAFT','Черновик'),
    ('99999999-9999-9999-9999-999999999992','ACTIVE','Действующий'),
    ('99999999-9999-9999-9999-999999999993','INACTIVE','Недействующий')
ON CONFLICT (code) DO NOTHING;

-- Базовое наполнение: 10 комитетов Совета директоров
INSERT INTO committees (id, code, name, description, behavior_type, is_mandatory_for_public, is_active, created_at) VALUES
    ('10000000-0000-0000-0000-000000000001','AUDIT','По аудиту',
     'Контроль финансовой отчетности, оценка независимости и качества работы внешнего аудитора, взаимодействие с ревизионной комиссией и службой внутреннего аудита, мониторинг систем управления рисками и внутреннего контроля.',
     'CONTROL', TRUE, TRUE, '2025-01-01T00:00:00Z'),
    ('10000000-0000-0000-0000-000000000002','HR_N_REM','По кадрам и вознаграждениям',
     'Разработка политики вознаграждения для членов Совета директоров и исполнительных органов, определение критериев подбора кандидатов в органы управления, планирование преемственности.',
     'CONTROL', FALSE, TRUE, '2025-01-01T00:00:00Z'),
    ('10000000-0000-0000-0000-000000000003','STRATEGY','По стратегии',
     'Предварительное рассмотрение вопросов стратегического развития, контроль реализации долгосрочных целей, выработка рекомендаций по дивидендной политике.',
     'STRATEGIC', FALSE, TRUE, '2025-01-01T00:00:00Z'),
    ('10000000-0000-0000-0000-000000000004','FINANCE','По финансам',
     'Предварительное рассмотрение финансовых планов и бюджетов, мониторинг финансовых показателей, анализ инвестиционных проектов.',
     'CONTROL', FALSE, TRUE, '2025-01-01T00:00:00Z'),
    ('10000000-0000-0000-0000-000000000005','HSE','По охране труда, промышленной безопасности и экологии',
     'Контроль соблюдения требований охраны труда, промышленной безопасности и экологического законодательства.',
     'CONTROL', FALSE, TRUE, '2025-01-01T00:00:00Z'),
    ('10000000-0000-0000-0000-000000000006','CG','По корпоративному управлению',
     'Совершенствование практик корпоративного управления, контроль соблюдения этических норм, взаимодействие с акционерами.',
     'CONTROL', FALSE, TRUE, '2025-01-01T00:00:00Z'),
    ('10000000-0000-0000-0000-000000000007','RISK','По рискам',
     'Идентификация и мониторинг существенных рисков, разработка мер по их минимизации, контроль эффективности системы управления рисками.',
     'CONTROL', FALSE, TRUE, '2025-01-01T00:00:00Z'),
    ('10000000-0000-0000-0000-000000000008','INVEST','По инвестициям',
     'Рассмотрение и оценка инвестиционных проектов, контроль их реализации и эффективности.',
     'STRATEGIC', FALSE, TRUE, '2025-01-01T00:00:00Z'),
    ('10000000-0000-0000-0000-000000000009','CSR','По корпоративной социальной ответственности',
     'Разработка и контроль реализации политики в области КСО, устойчивого развития и взаимодействия с заинтересованными сторонами.',
     'STRATEGIC', FALSE, TRUE, '2025-01-01T00:00:00Z'),
    ('10000000-0000-0000-0000-00000000000A','REI','По надежности, энергоэффективности и инновациям',
     'Контроль надежности производственных мощностей, повышение энергоэффективности и внедрение инноваций.',
     'STRATEGIC', FALSE, TRUE, '2025-01-01T00:00:00Z')
ON CONFLICT (code) DO NOTHING;


-- Шаблоны организационных мероприятий
INSERT INTO tpl_org_intents (id, name, description, sort_order) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'GOSA', 'Подготовка к ГОСА', 'Подготовка к годовому общему собранию акционеров', 1),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'BOARD_MEETING', 'Заседание Совета директоров', 'Стандартный цикл проведения заседания СД', 2)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_stages (id, intent_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Формирование повестки дня', 'Сбор и утверждение вопросов повестки ГОСА', 1, 0, 'FIXED_DAYS', 14),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Утверждение списка кандидатов в СД', 'Выдвижение и утверждение кандидатов в Совет директоров', 2, 14, 'FIXED_DAYS', 14),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Подготовка бюллетеней и уведомлений', 'Формирование документов для рассылки акционерам', 3, 28, 'FIXED_DAYS', 7),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Проведение ГОСА', 'Регистрация, голосование, подведение итогов', 4, 35, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb11', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Созыв заседания', 'Уведомление членов СД о дате и повестке', 1, 0, 'FIXED_DAYS', 5),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb12', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Проведение заседания', 'Обсуждение вопросов повестки', 2, 5, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb13', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Подписание протокола', 'Оформление и подписание протокола заседания', 3, 6, 'FIXED_DAYS', 3)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_offers (id, stage_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days) VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc01', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'Сбор предложений от акционеров', 'Приём предложений в повестку дня ГОСА', 1, 0, 'FIXED_DAYS', 7),
    ('cccccccc-cccc-cccc-cccc-cccccccccc02', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'Утверждение перечня вопросов', 'Формирование и утверждение окончательного перечня', 2, 7, 'FIXED_DAYS', 7),
    ('cccccccc-cccc-cccc-cccc-cccccccccc03', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Выдвижение кандидатов', 'Сбор заявок на выдвижение в Совет директоров', 1, 0, 'FIXED_DAYS', 7),
    ('cccccccc-cccc-cccc-cccc-cccccccccc04', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Согласование с комитетом по назначениям', 'Проверка кандидатов комитетом по назначениям', 2, 7, 'FIXED_DAYS', 7),
    ('cccccccc-cccc-cccc-cccc-cccccccccc05', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Формирование бюллетеней', 'Подготовка бюллетеней для голосования', 1, 0, 'FIXED_DAYS', 3),
    ('cccccccc-cccc-cccc-cccc-cccccccccc06', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Рассылка уведомлений акционерам', 'Отправка уведомлений о проведении ГОСА', 2, 3, 'FIXED_DAYS', 4),
    ('cccccccc-cccc-cccc-cccc-cccccccccc07', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Регистрация участников', 'Регистрация акционеров и проверка полномочий', 1, 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc08', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Голосование и подсчёт', 'Проведение голосования и подсчёт голосов', 2, 0, NULL, NULL)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_intents (id, name, description, sort_order) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'VOSA', 'Подготовка к ВОСА', 'Подготовка к внеочередному общему собранию акционеров', 3)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_stages (id, intent_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb21', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Выдвижение требования', 'Получение и проверка требования о созыве ВОСА', 1, 0, 'FIXED_DAYS', 5),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb22', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Подготовка повестки', 'Формирование повестки дня ВОСА', 2, 5, 'FIXED_DAYS', 5),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb23', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Созыв собрания', 'Уведомление акционеров и подготовка бюллетеней', 3, 10, 'FIXED_DAYS', 20),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb24', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Проведение ВОСА', 'Регистрация, голосование, подведение итогов', 4, 30, 'FIXED_DAYS', 1)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_offers (id, stage_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days) VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc21', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb21', 'Проверка требования', 'Проверка легитимности требования о созыве ВОСА', 1, 0, 'FIXED_DAYS', 3),
    ('cccccccc-cccc-cccc-cccc-cccccccccc22', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb21', 'Принятие решения о созыве', 'Решение СД о созыве либо отказе в созыве ВОСА', 2, 3, 'FIXED_DAYS', 2),
    ('cccccccc-cccc-cccc-cccc-cccccccccc23', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb22', 'Утверждение вопросов', 'Утверждение перечня вопросов повестки дня', 1, 0, 'FIXED_DAYS', 5),
    ('cccccccc-cccc-cccc-cccc-cccccccccc24', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb23', 'Рассылка уведомлений', 'Уведомление акционеров о проведении ВОСА', 1, 0, 'FIXED_DAYS', 5),
    ('cccccccc-cccc-cccc-cccc-cccccccccc25', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb23', 'Формирование бюллетеней', 'Подготовка бюллетеней для голосования', 2, 5, 'FIXED_DAYS', 5),
    ('cccccccc-cccc-cccc-cccc-cccccccccc26', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb24', 'Регистрация участников', 'Регистрация акционеров и проверка полномочий', 1, 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc27', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb24', 'Голосование и подсчёт', 'Проведение голосования и подсчёт голосов', 2, 0, NULL, NULL)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_intents (id, code, name, description, sort_order) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'FIRST_BOARD', 'Подготовка и проведение первого СД', 'Подготовка и проведение первого заседания Совета директоров после избрания', 4)
ON CONFLICT DO NOTHING;

-- Этапы FIRST_BOARD
INSERT INTO tpl_org_stages (id, intent_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb31', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'Подготовка (до заседания)', 'Подготовка шаблона протокола, уведомление членов СД, сбор материалов', 1, 0, 'FIXED_DAYS', 7),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'Проведение первого заседания', 'Открытие, голосование по избранию председателя, зама, секретаря, комитетов, закрытие', 2, 7, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'После заседания (результаты)', 'Получение УКЭП/МЧД, оформление и подписание протокола, рассылка, хранение', 3, 8, 'FIXED_DAYS', 5)
ON CONFLICT DO NOTHING;

-- Оферы FIRST_BOARD
INSERT INTO tpl_org_offers (id, stage_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days) VALUES
    -- Stage 1: Подготовка
    ('cccccccc-cccc-cccc-cccc-cccccccccc31', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb31', 'Шаблон протокола и подготовка к избранию', 'Подготовка шаблона протокола и материалов к выборам', 1, 0, 'FIXED_DAYS', 7),
    ('cccccccc-cccc-cccc-cccc-cccccccccc32', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb31', 'Уведомление и сбор материалов', 'Уведомление членов СД, сбор проектов решений, бюллетеней, мнений', 2, 0, 'FIXED_DAYS', 7),
    -- Stage 2: Проведение заседания
    ('cccccccc-cccc-cccc-cccc-cccccccccc33', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Открытие заседания и проверка кворума', 'Открытие заседания, проверка кворума', 1, 0, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc34', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Избрание председателя СД', 'Голосование, подсчёт, объявление результатов', 2, 0, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc35', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Избрание заместителя председателя СД', 'Голосование, подсчёт, объявление результатов', 3, 0, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc36', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Избрание секретаря СД', 'Голосование, подсчёт, объявление результатов', 4, 0, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc37', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Формирование комитетов СД', 'Голосование, подсчёт, объявление результатов', 5, 0, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc38', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Иные вопросы и закрытие заседания', 'Голосование по иным вопросам, подсчёт, объявление, закрытие', 6, 0, NULL, NULL),
    -- Stage 3: После заседания
    ('cccccccc-cccc-cccc-cccc-cccccccccc41', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Получение УКЭП', 'Получение УКЭП для председателя, заместителя, секретаря', 1, 0, 'FIXED_DAYS', 5),
    ('cccccccc-cccc-cccc-cccc-cccccccccc42', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Оформление и проверка протокола', 'Оформление финального протокола, проверка УКЭП/МЧД', 2, 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc43', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Бумажное подписание протокола', 'Подписание бумажного протокола и внесение скана', 3, 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc44', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Электронное подписание протокола', 'Подписание протокола с использованием УКЭП', 4, 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc45', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Завершение', 'Рассылка, подготовка для регуляторов, хранение, исполнение', 5, 0, 'FIXED_DAYS', 3)
ON CONFLICT DO NOTHING;

-- Задачи FIRST_BOARD
INSERT INTO tpl_org_tasks (id, offer_id, name, description, sort_order, assigned_board_role_id) VALUES
    -- Stage 1, Offer 1: Шаблон протокола и подготовка к избранию
    ('dddddddd-dddd-dddd-dddd-dddddddddd01', 'cccccccc-cccc-cccc-cccc-cccccccccc31', 'Подготовка шаблона протокола первого заседания', 'Создание шаблона протокола по ст. 68 п. 4 208-ФЗ', 1, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd02', 'cccccccc-cccc-cccc-cccc-cccccccccc31', 'Подготовка к избранию председателя СД', 'Подготовка материалов к голосованию по председателю', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd03', 'cccccccc-cccc-cccc-cccc-cccccccccc31', 'Подготовка к избранию заместителя председателя СД', 'Подготовка материалов к голосованию по заместителю', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff2'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd04', 'cccccccc-cccc-cccc-cccc-cccccccccc31', 'Подготовка к избранию секретаря СД', 'Подготовка материалов к голосованию по секретарю', 4, 'ffffffff-ffff-ffff-ffff-fffffffffff6'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd05', 'cccccccc-cccc-cccc-cccc-cccccccccc31', 'Подготовка к формированию комитетов СД', 'Подготовка материалов к голосованию по комитетам', 5, NULL),
    -- Stage 1, Offer 2: Уведомление и сбор материалов
    ('dddddddd-dddd-dddd-dddd-dddddddddd06', 'cccccccc-cccc-cccc-cccc-cccccccccc32', 'Уведомление членов СД о дате, времени, месте и повестке заседания', 'Рассылка уведомлений по ст. 68 п. 1 208-ФЗ', 1, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd07', 'cccccccc-cccc-cccc-cccc-cccccccccc32', 'Сбор и консолидация проектов решений по вопросам повестки', 'Сбор проектов документов от членов СД', 2, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd08', 'cccccccc-cccc-cccc-cccc-cccccccccc32', 'Подготовка и рассылка бюллетеней для голосования', 'При заочной или смешанной форме заседания', 3, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd09', 'cccccccc-cccc-cccc-cccc-cccccccccc32', 'Сбор и обработка письменных мнений членов СД', 'Сбор письменных мнений отсутствующих членов', 4, NULL),
    -- Stage 2, Offer 1: Открытие заседания и проверка кворума
    ('dddddddd-dddd-dddd-dddd-dddddddddd11', 'cccccccc-cccc-cccc-cccc-cccccccccc33', 'Открытие заседания', 'Открытие заседания председательствующим', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd12', 'cccccccc-cccc-cccc-cccc-cccccccccc33', 'Проверка кворума', 'Проверка кворума по ст. 68 п. 2 208-ФЗ', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    -- Stage 2, Offer 2: Избрание председателя СД
    ('dddddddd-dddd-dddd-dddd-dddddddddd13', 'cccccccc-cccc-cccc-cccc-cccccccccc34', 'Голосование по избранию председателя СД', 'Голосование членов СД', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd14', 'cccccccc-cccc-cccc-cccc-cccccccccc34', 'Подсчёт голосов по избранию председателя СД', 'Подсчёт голосов председательствующим', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd15', 'cccccccc-cccc-cccc-cccc-cccccccccc34', 'Подведение итогов и объявление результатов', 'Объявление результатов голосования', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    -- Stage 2, Offer 3: Избрание заместителя председателя СД
    ('dddddddd-dddd-dddd-dddd-dddddddddd16', 'cccccccc-cccc-cccc-cccc-cccccccccc35', 'Голосование по избранию заместителя председателя СД', 'Голосование членов СД', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd17', 'cccccccc-cccc-cccc-cccc-cccccccccc35', 'Подсчёт голосов по избранию заместителя председателя СД', 'Подсчёт голосов председательствующим', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd18', 'cccccccc-cccc-cccc-cccc-cccccccccc35', 'Подведение итогов и объявление результатов', 'Объявление результатов голосования', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    -- Stage 2, Offer 4: Избрание секретаря СД
    ('dddddddd-dddd-dddd-dddd-dddddddddd19', 'cccccccc-cccc-cccc-cccc-cccccccccc36', 'Голосование по избранию секретаря СД', 'Голосование членов СД', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd20', 'cccccccc-cccc-cccc-cccc-cccccccccc36', 'Подсчёт голосов по избранию секретаря СД', 'Подсчёт голосов председательствующим', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd21', 'cccccccc-cccc-cccc-cccc-cccccccccc36', 'Подведение итогов и объявление результатов', 'Объявление результатов голосования', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    -- Stage 2, Offer 5: Формирование комитетов СД
    ('dddddddd-dddd-dddd-dddd-dddddddddd22', 'cccccccc-cccc-cccc-cccc-cccccccccc37', 'Голосование по формированию комитетов СД', 'Голосование членов СД', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd23', 'cccccccc-cccc-cccc-cccc-cccccccccc37', 'Подсчёт голосов по формированию комитетов СД', 'Подсчёт голосов секретарём', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff6'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd24', 'cccccccc-cccc-cccc-cccc-cccccccccc37', 'Подведение итогов и объявление результатов', 'Объявление результатов голосования', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    -- Stage 2, Offer 6: Иные вопросы и закрытие заседания
    ('dddddddd-dddd-dddd-dddd-dddddddddd25', 'cccccccc-cccc-cccc-cccc-cccccccccc38', 'Голосование по иным вопросам повестки', 'Голосование по дополнительным вопросам', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd26', 'cccccccc-cccc-cccc-cccc-cccccccccc38', 'Подсчёт голосов по иным вопросам повестки', 'Подсчёт голосов секретарём', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff6'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd27', 'cccccccc-cccc-cccc-cccc-cccccccccc38', 'Подведение итогов и объявление результатов', 'Объявление результатов голосования', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd28', 'cccccccc-cccc-cccc-cccc-cccccccccc38', 'Закрытие заседания', 'Закрытие заседания председательствующим', 4, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    -- Stage 3, Offer 1: Получение УКЭП
    ('dddddddd-dddd-dddd-dddd-dddddddddd31', 'cccccccc-cccc-cccc-cccc-cccccccccc41', 'Получение УКЭП для избранного председателя СД', 'При ЮЗЭДО; 63-ФЗ', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd32', 'cccccccc-cccc-cccc-cccc-cccccccccc41', 'Получение УКЭП для избранного заместителя председателя СД', 'При ЮЗЭДО; 63-ФЗ', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff2'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd33', 'cccccccc-cccc-cccc-cccc-cccccccccc41', 'Получение УКЭП для избранного секретаря СД', 'При ЮЗЭДО; 63-ФЗ', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff6'),
    -- Stage 3, Offer 2: Оформление и проверка протокола
    ('dddddddd-dddd-dddd-dddd-dddddddddd37', 'cccccccc-cccc-cccc-cccc-cccccccccc42', 'Оформление финального протокола заседания', 'Составление протокола по ст. 68 п. 4 208-ФЗ', 1, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd38', 'cccccccc-cccc-cccc-cccc-cccccccccc42', 'Проверка наличия УКЭП у всех подписантов', 'При ЮЗЭДО; 63-ФЗ', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    -- Stage 3, Offer 3: Бумажное подписание протокола
    ('dddddddd-dddd-dddd-dddd-dddddddddd41', 'cccccccc-cccc-cccc-cccc-cccccccccc43', 'Бумажное подписание протокола председателем', 'Подписание бумажного экземпляра', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd42', 'cccccccc-cccc-cccc-cccc-cccccccccc43', 'Бумажное подписание протокола заместителем председателя', 'При отсутствии председателя', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff2'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd43', 'cccccccc-cccc-cccc-cccc-cccccccccc43', 'Бумажное подписание протокола секретарем', 'При наличии права подписи', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff6'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd44', 'cccccccc-cccc-cccc-cccc-cccccccccc43', 'Внесение электронного образа подписанного документа в систему', 'Сканирование и загрузка в систему', 4, NULL),
    -- Stage 3, Offer 4: Электронное подписание протокола
    ('dddddddd-dddd-dddd-dddd-dddddddddd45', 'cccccccc-cccc-cccc-cccc-cccccccccc44', 'Подписание протокола председателем с использованием УКЭП', 'При ЮЗЭДО; ст. 68 п. 4 208-ФЗ', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff1'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd46', 'cccccccc-cccc-cccc-cccc-cccccccccc44', 'Подписание протокола заместителем председателя с использованием УКЭП', 'При отсутствии председателя и ЮЗЭДО', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff2'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd47', 'cccccccc-cccc-cccc-cccc-cccccccccc44', 'Подписание протокола секретарем с использованием УКЭП', 'При наличии права подписи и ЮЗЭДО', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff6'),
    -- Stage 3, Offer 5: Завершение
    ('dddddddd-dddd-dddd-dddd-dddddddddd48', 'cccccccc-cccc-cccc-cccc-cccccccccc45', 'Рассылка копий протокола и решений членам СД', 'Рассылка по ст. 68 п. 4 208-ФЗ', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd49', 'cccccccc-cccc-cccc-cccc-cccccccccc45', 'Обеспечение готовности пакета документов для регулирующих органов', 'Подготовка документов для ЦБ РФ и других органов', 2, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd50', 'cccccccc-cccc-cccc-cccc-cccccccccc45', 'Организация постоянного хранения полного комплекта документов заседания СД', 'Хранение по ст. 89 208-ФЗ', 3, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd51', 'cccccccc-cccc-cccc-cccc-cccccccccc45', 'Организация исполнения принятых решений', 'Контроль исполнения по ст. 69 208-ФЗ', 4, NULL)
ON CONFLICT DO NOTHING;

-- Оферы для шаблона «Заседание Совета директоров» (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20)
INSERT INTO tpl_org_offers (id, stage_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days) VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc11', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb11', 'Уведомление членов СД', 'Подготовка и рассылка уведомлений о созыве', 1, 0, 'FIXED_DAYS', 3),
    ('cccccccc-cccc-cccc-cccc-cccccccccc12', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb11', 'Сбор материалов', 'Сбор и подготовка материалов к заседанию', 2, 3, 'FIXED_DAYS', 2),
    ('cccccccc-cccc-cccc-cccc-cccccccccc13', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb12', 'Обсуждение вопросов', 'Проведение обсуждения вопросов повестки', 1, 0, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc14', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb12', 'Голосование', 'Проведение голосования по каждому вопросу', 2, 0, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc15', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb13', 'Оформление протокола', 'Подготовка текста протокола заседания', 1, 0, 'FIXED_DAYS', 2),
    ('cccccccc-cccc-cccc-cccc-cccccccccc16', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb13', 'Подписание протокола', 'Подписание протокола председателем и секретарём', 2, 2, 'FIXED_DAYS', 1)
ON CONFLICT DO NOTHING;
