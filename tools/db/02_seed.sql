-- 02_seed.sql — первичное наполнение (справочники и системные записи)

-- ============================================================================
-- Системный пользователь (нулевой GUID) — создаётся первым,
-- используется как created_by для всех справочников ниже
-- ============================================================================
INSERT INTO users (id, last_name, first_name, email, phone, is_external, created_at, created_by, is_system)
VALUES (
    '00000000-0000-0000-0000-000000000000',
    'Системный',
    'Пользователь',
    'system@fiducia.local',
    '+00000000000',
    FALSE,
    CURRENT_TIMESTAMP,
    '00000000-0000-0000-0000-000000000000',
    TRUE
) ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- Тестовые физические лица (persons)
-- ============================================================================
INSERT INTO persons (id, last_name, first_name, middle_name, email, phone, inn, created_at, created_by) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01', 'Иванов', 'Иван', 'Иванович', 'ivanov@fiducia.local', '+79001112233', '770123456789', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', 'Петрова', 'Мария', 'Сергеевна', 'petrova@fiducia.local', '+79002223344', '770234567890', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03', 'Сидоров', 'Алексей', 'Петрович', 'sidorov@fiducia.local', '+79003334455', '770345678901', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa04', 'Козлова', 'Елена', 'Дмитриевна', 'kozlova@fiducia.local', '+79004445566', '770456789012', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa05', 'Новиков', 'Дмитрий', 'Александрович', 'novikov@fiducia.local', '+79005556677', '770567890123', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- Тестовые пользователи (привязаны к ФЛ)
-- ============================================================================
INSERT INTO users (id, person_id, last_name, first_name, email, phone, is_external, created_at, created_by, is_system) VALUES
    ('11111111-1111-1111-1111-111111111112', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01', 'Иванов', 'Иван', 'ivanov@fiducia.local', '+79001112233', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE),
    ('11111111-1111-1111-1111-111111111113', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', 'Петрова', 'Мария', 'petrova@fiducia.local', '+79002223344', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE),
    ('11111111-1111-1111-1111-111111111114', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03', 'Сидоров', 'Алексей', 'sidorov@fiducia.local', '+79003334455', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE),
    ('11111111-1111-1111-1111-111111111115', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa04', 'Козлова', 'Елена', 'kozlova@fiducia.local', '+79004445566', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE),
    ('11111111-1111-1111-1111-111111111116', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa05', 'Новиков', 'Дмитрий', 'novikov@fiducia.local', '+79005556677', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE)
ON CONFLICT (id) DO NOTHING;

INSERT INTO ref_roles (id, code, name, created_at, created_by, is_assignable) VALUES
    ('11111111-1111-1111-1111-111111111111','SYS_ADMIN','Системный администратор',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000',FALSE),
    ('22222222-2222-2222-2222-222222222222','SECRETARY','Секретарь',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000',TRUE),
    ('33333333-3333-3333-3333-333333333333','CHAIR_BOARD','Председатель СД',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000',FALSE),
    ('44444444-4444-4444-4444-444444444444','MEMBER_BOARD','Член СД',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000',FALSE),
    ('55555555-5555-5555-5555-555555555555','EXTERNAL_DIRECTOR','Внешний/Независимый директор',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000',FALSE),
    ('66666666-6666-6666-6666-666666666666','SHAREHOLDER','Акционер',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000',FALSE),
    ('77777777-7777-7777-7777-777777777777','COMMITTEE_CHAIR','Председатель комитета',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000',FALSE),
    ('88888888-8888-8888-8888-888888888888','COMMITTEE_MEMBER','Член комитета',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000',FALSE),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaad2','DEPUTY_CHAIR','Заместитель председателя СД',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000',FALSE),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaad3','LAWYER','Юрист',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000',TRUE)
ON CONFLICT (code) DO NOTHING;

-- Роли тестовых пользователей
INSERT INTO user_roles (id, user_id, role_id) VALUES
    ('aaaa0000-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111112', '11111111-1111-1111-1111-111111111111'), -- Иванов = SYS_ADMIN
    ('aaaa0000-0000-0000-0000-000000000002', '11111111-1111-1111-1111-111111111113', '22222222-2222-2222-2222-222222222222'), -- Петрова = CORP_SECRETARY
    ('aaaa0000-0000-0000-0000-000000000003', '11111111-1111-1111-1111-111111111114', '55555555-5555-5555-5555-555555555555'), -- Сидоров = EXTERNAL_DIRECTOR
    ('aaaa0000-0000-0000-0000-000000000004', '11111111-1111-1111-1111-111111111115', '55555555-5555-5555-5555-555555555555'), -- Козлова = EXTERNAL_DIRECTOR
    ('aaaa0000-0000-0000-0000-000000000005', '11111111-1111-1111-1111-111111111116', '33333333-3333-3333-3333-333333333333')  -- Новиков = CHAIR_BOARD
ON CONFLICT (user_id, role_id) DO NOTHING;

-- ОКОПФ (базовые записи)
INSERT INTO ref_okopf(id, code, name, created_at, created_by) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1','12247','Публичное акционерное общество (ПАО)',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2','12267','Непубличное акционерное общество (НАО)',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3','12260','Акционерное общество (АО)',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4','12300','Общество с ограниченной ответственностью (ООО)',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5','65241','Федеральное государственное унитарное предприятие (ФГУП)',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6','65242','Государственное унитарное предприятие субъекта РФ (ГУП)',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7','65243','Муниципальное унитарное предприятие (МУП)',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;

-- Месяцы
INSERT INTO ref_month(id, code, name, created_at, created_by) VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc01','01','Январь',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc02','02','Февраль',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc03','03','Март',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc04','04','Апрель',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc05','05','Май',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc06','06','Июнь',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc07','07','Июль',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc08','08','Август',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc09','09','Сентябрь',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc10','10','Октябрь',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc11','11','Ноябрь',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc12','12','Декабрь',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;

-- Форма проведения заседания СД
INSERT INTO ref_meeting_form(id, code, name, short_name, created_at, created_by) VALUES
    ('ffffffff-ffff-ffff-ffff-fffffffffff1','OCHN','Очное заседание (совместное присутствие)','Очное',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('ffffffff-ffff-ffff-ffff-fffffffffff2','ZAOCHN','Заочное голосование (опросным путём)','Заочное',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('ffffffff-ffff-ffff-ffff-fffffffffff3','MIXED','Смешанное (очное заседание + заочное голосование)','Смешанное',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;

-- Форма проведения ОСА
INSERT INTO ref_osa_form(id, code, name, short_name, created_at, created_by) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1','GOSA','Годовое общее собрание акционеров','ГОСА',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2','VOSA','Внеочередное общее собрание акционеров','ВОСА',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3','OOSU','Очередное общее собрание участников','ООСУ',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4','VOSU','Внеочередное общее собрание участников','ВОСУ',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;

-- Типы директоров
INSERT INTO ref_board_member_types(id, code, name, created_at, created_by) VALUES
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1','EXECUTIVE','Исполнительный директор',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2','NON_EXECUTIVE','Внешний директор',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee3','INDEPENDENT','Независимый директор',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee4','STAFF','Штатный сотрудник',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;

-- Должности в СД
INSERT INTO ref_board_roles(id, code, name, sort_order, created_at, created_by) VALUES
    ('ffffffff-ffff-ffff-ffff-fffffffffff1','CHAIR','Председатель СД',1,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('ffffffff-ffff-ffff-ffff-fffffffffff2','DEPUTY_CHAIR','Заместитель председателя',2,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('ffffffff-ffff-ffff-ffff-fffffffffff3','MEMBER','Член СД',3,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('ffffffff-ffff-ffff-ffff-fffffffffff4','TEMP_CHAIR','Временный председательствующий',4,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('ffffffff-ffff-ffff-ffff-fffffffffff5','TEMP_SECRETARY','Временный секретарь',5,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('ffffffff-ffff-ffff-ffff-fffffffffff6','SECRETARY','Секретарь СД',6,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;

-- Статусы Совета директоров
INSERT INTO ref_board_of_directors_statuses(id, code, name, created_at, created_by) VALUES
    ('99999999-9999-9999-9999-999999999991','DRAFT','Черновик',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('99999999-9999-9999-9999-999999999992','ACTIVE','Действующий',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('99999999-9999-9999-9999-999999999993','INACTIVE','Недействующий',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;

-- Справочник: сроки полномочий генерального директора ООО
INSERT INTO ref_gd_term(id, code, name, duration_years, sort_order, created_at, created_by) VALUES
    ('aaaa0000-0000-0000-0000-000000000001','1_YEAR','1 год',1,1,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaa0000-0000-0000-0000-000000000002','2_YEARS','2 года',2,2,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaa0000-0000-0000-0000-000000000003','3_YEARS','3 года',3,3,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaa0000-0000-0000-0000-000000000004','4_YEARS','4 года',4,4,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaa0000-0000-0000-0000-000000000005','5_YEARS','5 лет',5,5,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('aaaa0000-0000-0000-0000-000000000006','INDEFINITE','Бессрочно',NULL,6,CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;

-- Справочник: ref_board_member_appointment_statuses (статусы назначения членов СД)
INSERT INTO ref_board_member_appointment_statuses(id, code, name, created_at, created_by) VALUES
    ('6e6bcad9-c361-48a2-9f08-3f86dbab7dc0','ACTIVE','Действующий',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('6e6bcad9-c361-48a2-9f08-3f86dbab7dc1','EXPIRED','Истёк срок',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('6e6bcad9-c361-48a2-9f08-3f86dbab7dc6','RESIGNED','Сложил полномочия',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('6e6bcad9-c361-48a2-9f08-3f86dbab7dc3','DISMISSED','Отстранён',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('6e6bcad9-c361-48a2-9f08-3f86dbab7dc4','VACANT','Вакансия',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;

-- Справочник: ref_resignation_reasons (причины сложения полномочий)
INSERT INTO ref_resignation_reasons(id, code, name, created_at, created_by) VALUES
    ('7f1a3e5b-2d8c-4a6e-9b0f-1c3d5e7f9a11','OWN_WILL','По собственному желанию',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('7f1a3e5b-2d8c-4a6e-9b0f-1c3d5e7f9a12','TERM_EXPIRED','Истечение срока полномочий',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('7f1a3e5b-2d8c-4a6e-9b0f-1c3d5e7f9a13','ELECTION','Избрание на другой пост',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('7f1a3e5b-2d8c-4a6e-9b0f-1c3d5e7f9a14','HEALTH','По состоянию здоровья',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('7f1a3e5b-2d8c-4a6e-9b0f-1c3d5e7f9a15','RELOCATION','Переезд в другой регион',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000'),
    ('7f1a3e5b-2d8c-4a6e-9b0f-1c3d5e7f9a16','CONFLICT_OF_INTEREST','Конфликт интересов',CURRENT_TIMESTAMP,'00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;

-- Базовое наполнение: 10 комитетов Совета директоров
INSERT INTO committees (id, code, name, description, behavior_type, is_mandatory_for_public, is_active, created_at, created_by) VALUES
    ('10000000-0000-0000-0000-000000000001','AUDIT','По аудиту',
     'Контроль финансовой отчетности, оценка независимости и качества работы внешнего аудитора, взаимодействие с ревизионной комиссией и службой внутреннего аудита, мониторинг систем управления рисками и внутреннего контроля.',
     'CONTROL', TRUE, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('10000000-0000-0000-0000-000000000002','HR_N_REM','По кадрам и вознаграждениям',
     'Разработка политики вознаграждения для членов Совета директоров и исполнительных органов, определение критериев подбора кандидатов в органы управления, планирование преемственности.',
     'CONTROL', FALSE, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('10000000-0000-0000-0000-000000000003','STRATEGY','По стратегии',
     'Предварительное рассмотрение вопросов стратегического развития, контроль реализации долгосрочных целей, выработка рекомендаций по дивидендной политике.',
     'STRATEGIC', FALSE, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('10000000-0000-0000-0000-000000000004','FINANCE','По финансам',
     'Предварительное рассмотрение финансовых планов и бюджетов, мониторинг финансовых показателей, анализ инвестиционных проектов.',
     'CONTROL', FALSE, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('10000000-0000-0000-0000-000000000005','HSE','По охране труда, промышленной безопасности и экологии',
     'Контроль соблюдения требований охраны труда, промышленной безопасности и экологического законодательства.',
     'CONTROL', FALSE, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('10000000-0000-0000-0000-000000000006','CG','По корпоративному управлению',
     'Совершенствование практик корпоративного управления, контроль соблюдения этических норм, взаимодействие с акционерами.',
     'CONTROL', FALSE, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('10000000-0000-0000-0000-000000000007','RISK','По рискам',
     'Идентификация и мониторинг существенных рисков, разработка мер по их минимизации, контроль эффективности системы управления рисками.',
     'CONTROL', FALSE, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('10000000-0000-0000-0000-000000000008','INVEST','По инвестициям',
     'Рассмотрение и оценка инвестиционных проектов, контроль их реализации и эффективности.',
     'STRATEGIC', FALSE, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('10000000-0000-0000-0000-000000000009','CSR','По корпоративной социальной ответственности',
     'Разработка и контроль реализации политики в области КСО, устойчивого развития и взаимодействия с заинтересованными сторонами.',
     'STRATEGIC', FALSE, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('10000000-0000-0000-0000-00000000000A','REI','По надежности, энергоэффективности и инновациям',
     'Контроль надежности производственных мощностей, повышение энергоэффективности и внедрение инноваций.',
     'STRATEGIC', FALSE, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT (code) DO NOTHING;


-- Шаблоны организационных мероприятий
INSERT INTO tpl_org_intents (id, code, name, description, sort_order, is_for_ao, is_for_llc, requires_board_of_directors, created_at, created_by) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'GOSA', 'Подготовка к ГОСА', 'Подготовка к годовому общему собранию акционеров', 1, true, null, true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'BOARD_MEETING', 'Заседание Совета директоров', 'Стандартный цикл проведения заседания СД', 2, true, null, true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Этапы ГОСА (5 фаз)
INSERT INTO tpl_org_stages (id, intent_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days, created_at, created_by) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Фаза 1: Запуск и предложения акционеров', 'Сбор предложений, постановка задач бухгалтерии, формирование повестки', 1, 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Фаза 2: Проверка отчётности', 'Ревизия, аудит, приём заключений', 2, 24, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Фаза 3: Решение СД о созыве и подготовка списка', 'Утверждение отчёта, решение о созыве, запрос и проверка списка акционеров', 3, 40, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Фаза 4: Подготовка и уведомление', 'Формирование повестки, сборка материалов, бюллетени, рассылка, уведомление нотариуса', 4, 82, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Фаза 5: ГОСА и завершение', 'Проведение собрания, протокол, ФНС, банк, регистратор, раскрытие', 5, 125, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb11', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Созыв заседания', 'Уведомление членов СД о дате и повестке', 1, 0, 'FIXED_DAYS', 5, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb12', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Проведение заседания', 'Обсуждение вопросов повестки', 2, 5, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb13', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Подписание протокола', 'Оформление и подписание протокола заседания', 3, 6, 'FIXED_DAYS', 3, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb14', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Завершение', 'Фиксация результатов заседания СД в системе, хранение документов, исполнение решений', 4, 9, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Задачи ГОСА (31 задача из gosa-gantt.md)
INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days, predecessor_offer_ids, created_at, created_by) VALUES
    ('cccccccc-cccc-cccc-cccc-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'Постановка задачи бухгалтерии', 'Постановка задачи бухгалтерии', 0, 'FIXED_DAYS', 1, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000002', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'Подготовка годового отчёта и баланса', 'Подготовка годового отчёта и баланса', 3, 'FIXED_DAYS', 15, '["cccccccc-cccc-cccc-cccc-000000000001"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000003', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'Приём предложений акционеров в повестку (дедлайн)', 'Приём предложений акционеров в повестку (дедлайн)', 21, 'FIXED_DAYS', 0, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000004', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'Заседание СД: рассмотрение предложений акционеров', 'Заседание СД: рассмотрение предложений акционеров', 24, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-000000000003"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000005', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Заключение договора на аудит', 'Заключение договора на аудит', -24, 'FIXED_DAYS', 3, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000006', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Передача документов ревизору', 'Передача документов ревизору', 0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000002"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000007', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Передача документов аудитору', 'Передача документов аудитору', 0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000002","cccccccc-cccc-cccc-cccc-000000000005"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000008', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Проверка отчётности ревизором', 'Проверка отчётности ревизором', 1, 'FIXED_DAYS', 10, '["cccccccc-cccc-cccc-cccc-000000000006"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000009', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Проведение аудита', 'Проведение аудита', 1, 'FIXED_DAYS', 40, '["cccccccc-cccc-cccc-cccc-000000000007"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000010', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Приём заключения ревизора', 'Приём заключения ревизора', 15, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000008"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000011', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Приём аудиторского заключения', 'Приём аудиторского заключения', 57, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000009"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000012', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Заседание СД: утверждение годового отчёта', 'Заседание СД: утверждение годового отчёта', 0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000010"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000013', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Заседание СД: решение о созыве ГОСА', 'Заседание СД: решение о созыве ГОСА', 42, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000011"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000014', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Запрос списка акционеров у регистратора', 'Запрос списка акционеров у регистратора', 43, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000013"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000015', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Получение быстрого списка от регистратора', 'Получение быстрого списка от регистратора', 44, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000014"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000016', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Получение полного списка (с раскрытием номинальных)', 'Получение полного списка (с раскрытием номинальных)', 47, 'FIXED_DAYS', 3, '["cccccccc-cccc-cccc-cccc-000000000015"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000017', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Проверка списка (валидация)', 'Проверка списка (валидация)', 50, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000016"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000018', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Формирование повестки и проектов решений', 'Формирование повестки и проектов решений', 0, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-000000000013"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000019', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Запрос выписки из ЕИС у нотариуса', 'Запрос выписки из ЕИС у нотариуса', 12, 'FIXED_DAYS', 1, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000020', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Сборка пакета материалов к ГОСА', 'Сборка пакета материалов к ГОСА', 9, 'FIXED_DAYS', 3, '["cccccccc-cccc-cccc-cccc-000000000017","cccccccc-cccc-cccc-cccc-000000000018"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000021', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Подготовка бюллетеней для голосования', 'Подготовка бюллетеней для голосования', 14, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-000000000020"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000022', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Рассылка уведомлений и бюллетеней акционерам', 'Рассылка уведомлений и бюллетеней акционерам', 19, 'FIXED_DAYS', 3, '["cccccccc-cccc-cccc-cccc-000000000017","cccccccc-cccc-cccc-cccc-000000000021"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000023', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Доступ к материалам для акционеров', 'Доступ к материалам для акционеров', 23, 'FIXED_DAYS', 0, '["cccccccc-cccc-cccc-cccc-000000000022"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000024', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Уведомление нотариуса о собрании', 'Уведомление нотариуса о собрании', 26, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000018"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000025', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Уведомление об изменении повестки (если есть доп. вопросы)', 'Уведомление об изменении повестки (если есть доп. вопросы)', 26, 'FIXED_DAYS', 1, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000026', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Проведение ГОСА', 'Проведение ГОСА', 0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000022","cccccccc-cccc-cccc-cccc-000000000024","cccccccc-cccc-cccc-cccc-000000000025"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000027', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Оформление протокола ГОСА', 'Оформление протокола ГОСА', 1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000026"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000028', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Уведомление банка о смене директора', 'Уведомление банка о смене директора', 1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000026"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000029', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Уведомление регистратора об изменениях', 'Уведомление регистратора об изменениях', 1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000026"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000030', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Раскрытие протокола (ПАО)', 'Раскрытие протокола (ПАО)', 2, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000027"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-000000000031', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Подача Р13014 в ФНС', 'Подача Р13014 в ФНС', 4, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000027"]', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_intents (id, code, name, description, sort_order, is_for_ao, is_for_llc, requires_board_of_directors, created_at, created_by) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'VOSA', 'Подготовка к ВОСА', 'Подготовка к внеочередному общему собранию акционеров', 3, true, null, true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_stages (id, intent_id, name, description, start_offset_days, deadline_rule, deadline_days, created_at, created_by) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb21', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Выдвижение требования', 'Получение и проверка требования о созыве ВОСА', 0, 'FIXED_DAYS', 5, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb22', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Подготовка повестки', 'Формирование повестки дня ВОСА', 5, 'FIXED_DAYS', 5, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb23', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Созыв собрания', 'Уведомление акционеров и подготовка бюллетеней', 10, 'FIXED_DAYS', 20, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb24', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Проведение ВОСА', 'Регистрация, голосование, подведение итогов', 30, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb25', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Завершение', 'Фиксация результатов ВОСА в системе, хранение документов, исполнение решений', 31, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days, created_at, created_by) VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc21', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb21', 'Проверка требования', 'Проверка легитимности требования о созыве ВОСА', 0, 'FIXED_DAYS', 3, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc22', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb21', 'Принятие решения о созыве', 'Решение СД о созыве либо отказе в созыве ВОСА', 3, 'FIXED_DAYS', 2, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc23', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb22', 'Утверждение вопросов', 'Утверждение перечня вопросов повестки дня', 0, 'FIXED_DAYS', 5, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc24', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb23', 'Рассылка уведомлений', 'Уведомление акционеров о проведении ВОСА', 0, 'FIXED_DAYS', 5, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc25', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb23', 'Формирование бюллетеней', 'Подготовка бюллетеней для голосования', 5, 'FIXED_DAYS', 5, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc26', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb24', 'Регистрация участников', 'Регистрация акционеров и проверка полномочий', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc27', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb24', 'Голосование и подсчёт', 'Проведение голосования и подсчёт голосов', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc28', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb25', 'Фиксация результатов', 'Фиксация итогов ВОСА в системе, хранение и исполнение решений', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Задачи ВОСА (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30)

INSERT INTO tpl_org_intents (id, code, name, description, sort_order, is_for_ao, is_for_llc, requires_board_of_directors, created_at, created_by) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'FIRST_BOARD', 'Подготовка и проведение первого СД', 'Подготовка и проведение первого заседания Совета директоров после избрания', 4, true, true, true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Этапы FIRST_BOARD
INSERT INTO tpl_org_stages (id, intent_id, name, description, start_offset_days, deadline_rule, deadline_days, created_at, created_by) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb31', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'Подготовка (до заседания)', 'Подготовка шаблона протокола, уведомление членов СД, сбор материалов', 0, 'FIXED_DAYS', 7, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'Проведение первого заседания', 'Открытие, голосование по избранию председателя, зама, секретаря, комитетов, закрытие', 7, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'После заседания (результаты)', 'Получение УКЭП/МЧД, оформление и подписание протокола, рассылка, хранение', 8, 'FIXED_DAYS', 5, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Оферы FIRST_BOARD
INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days, created_at, created_by) VALUES
    -- Stage 1: Подготовка
    ('cccccccc-cccc-cccc-cccc-cccccccccc31', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb31', 'Шаблон протокола и подготовка к избранию', 'Подготовка шаблона протокола и материалов к выборам', 0, 'FIXED_DAYS', 7, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc32', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb31', 'Уведомление и сбор материалов', 'Уведомление членов СД, сбор проектов решений, бюллетеней, мнений', 0, 'FIXED_DAYS', 7, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Stage 2: Проведение заседания
    ('cccccccc-cccc-cccc-cccc-cccccccccc33', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Открытие заседания и проверка кворума', 'Открытие заседания, проверка кворума', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc34', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Избрание председателя СД', 'Голосование, подсчёт, объявление результатов', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc35', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Избрание заместителя председателя СД', 'Голосование, подсчёт, объявление результатов', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc36', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Избрание секретаря СД', 'Голосование, подсчёт, объявление результатов', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc37', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Формирование комитетов СД', 'Голосование, подсчёт, объявление результатов', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc38', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Иные вопросы и закрытие заседания', 'Голосование по иным вопросам, подсчёт, объявление, закрытие', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Stage 3: После заседания
    ('cccccccc-cccc-cccc-cccc-cccccccccc41', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Получение УКЭП', 'Получение УКЭП для председателя, заместителя, секретаря', 0, 'FIXED_DAYS', 5, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc42', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Оформление и проверка протокола', 'Оформление финального протокола, проверка УКЭП/МЧД', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc43', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Бумажное подписание протокола', 'Подписание бумажного протокола и внесение скана', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc44', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Электронное подписание протокола', 'Подписание протокола с использованием УКЭП', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc45', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Завершение', 'Рассылка, подготовка для регуляторов, хранение, исполнение', 0, 'FIXED_DAYS', 3, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Задачи FIRST_BOARD (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40)

-- Задачи «Завершение» для ГОСА (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10)

-- Задачи «Завершение» для Заседания СД (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20)

-- Задачи «Завершение» для ВОСА (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30)

-- Оферы для шаблона «Заседание Совета директоров» (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20)
INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days, created_at, created_by) VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc11', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb11', 'Уведомление членов СД', 'Подготовка и рассылка уведомлений о созыве', 0, 'FIXED_DAYS', 3, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc12', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb11', 'Сбор материалов', 'Сбор и подготовка материалов к заседанию', 3, 'FIXED_DAYS', 2, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc13', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb12', 'Обсуждение вопросов', 'Проведение обсуждения вопросов повестки', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc14', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb12', 'Голосование', 'Проведение голосования по каждому вопросу', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc15', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb13', 'Оформление протокола', 'Подготовка текста протокола заседания', 0, 'FIXED_DAYS', 2, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc16', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb13', 'Подписание протокола', 'Подписание протокола председателем и секретарём', 2, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc17', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb14', 'Фиксация результатов', 'Фиксация итогов заседания СД в системе, хранение и исполнение решений', 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Шаблон «Подготовка к ООСУ» (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50)
-- Каждый офер = одна задача (как в ГОСА/ВОСА). Предикаты управляют условным включением.
-- Сценарий А (с аудитом) — критический путь 53 р.д. Сценарий Б (без аудита) — 23 р.д.
INSERT INTO tpl_org_intents (id, code, name, description, sort_order, is_for_ao, is_for_llc, requires_board_of_directors, created_at, created_by) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'OOSU', 'Подготовка к ООСУ', 'Подготовка и проведение очередного общего собрания участников ООО (ст. 34 14-ФЗ)', 5, null, true, null, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_stages (id, intent_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days, created_at, created_by) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb41', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Запуск', 'Постановка задачи бухгалтерии о подготовке годовой отчётности', 1, 0, 'FIXED_DAYS', 1, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb42', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Подготовка отчётности', 'Подготовка годового отчёта и баланса, формирование повестки ООСУ', 2, 1, 'FIXED_DAYS', 18, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Проверка отчётности', 'Ревизия и аудит отчётности, приём заключений', 3, 19, 'FIXED_DAYS', 40, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Подготовка к собранию', 'Сборка материалов, уведомление участников и нотариуса, доп. вопросы', 4, 59, 'FIXED_DAYS', 30, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb45', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Проведение ООСУ', 'Проведение собрания, оформление протокола', 5, 89, 'FIXED_DAYS', 2, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb46', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Регистрация изменений', 'Подача Р13014 в ФНС, уведомление банка о смене директора', 6, 91, 'FIXED_DAYS', 14, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb47', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Завершение', 'Фиксация результатов, хранение, исполнение решений', 7, 105, 'FIXED_DAYS', 3, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Оферы ООСУ: каждый офер — шаблон одной задачи, с predecessor_offer_ids и предикатами
-- Префиксы: Б=бухгалтерия, П=подготовка, Р=ревизор, А=аудит, Н=нотариус, Д=допвопросы, ОС=собрание
INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days,
    predecessor_offer_ids, require_notary_confirmation, require_executive_body_a,
    require_mandatory_audit, require_revision_commission, created_at, created_by) VALUES
    -- Фаза 1: Запуск
    ('cccccccc-cccc-cccc-cccc-cccccccccc91', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb41',
     'Постановка задачи бухгалтерии',
     'Приказ/поручение о подготовке годовой отчётности (Б1)',
     0, 'FIXED_DAYS', 1, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Фаза 2: Подготовка отчётности
    ('cccccccc-cccc-cccc-cccc-cccccccccc92', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb42',
     'Подготовка годового отчёта и баланса',
     'Формирование годового отчёта и бухгалтерского баланса (Б2)',
     0, 'FIXED_DAYS', 15, '["cccccccc-cccc-cccc-cccc-cccccccccc91"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc93', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb42',
     'Формирование повестки ООСУ',
     'Формирование перечня вопросов с учётом результатов года (П1)',
     15, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc92"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc94', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb42',
     'Поиск аудитора на следующий год',
     'Запрос КП у 2–3 аудиторских организаций, проверка СРО, подготовка обоснования для ОСУ. Для гос. участия ≥25% — конкурс по 44-ФЗ/223-ФЗ (П0)',
     0, 'FIXED_DAYS', 10, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Фаза 3: Проверка отчётности
    ('cccccccc-cccc-cccc-cccc-cccccccccc95', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Передача документов ревизору',
     'Передача годового отчёта и баланса ревизионной комиссии (ревизору). ⚠️ Только при >15 участников (Р1)',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc92"]', NULL, NULL, NULL, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc96', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Проверка отчётности ревизором',
     'Проверка отчётности ревизионной комиссией (ревизором). ⚠️ Только при >15 участников (Р2)',
     1, 'FIXED_DAYS', 10, '["cccccccc-cccc-cccc-cccc-cccccccccc95"]', NULL, NULL, NULL, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc97', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Приём заключения ревизора',
     'Проверка полноты заключения, подписей, отсутствия оговорок. ⚠️ Только при >15 участников (Р3)',
     11, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc96"]', NULL, NULL, NULL, TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc98', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Передача документов аудитору',
     'Передача отчётности аудитору для проверки. ⚠️ Только при обязательном аудите (выручка >800 млн или активы >400 млн) (А2)',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc92"]', NULL, NULL, TRUE, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc99', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Проведение аудита',
     'Проверка отчётности аудитором. ⚠️ Критический путь (35 р.д.). Только при обязательном аудите (А3)',
     1, 'FIXED_DAYS', 35, '["cccccccc-cccc-cccc-cccc-cccccccccc98"]', NULL, NULL, TRUE, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc9A', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Приём аудиторского заключения',
     'Проверка формы (приказ Минфина №46н), подписей, даты. ⚠️ Только при обязательном аудите (А4)',
     36, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc99"]', NULL, NULL, TRUE, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Фаза 4: Подготовка к собранию
    ('cccccccc-cccc-cccc-cccc-cccccccccc9B', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Сборка пакета материалов',
     'Формирование полного пакета документов к ООСУ. Зависит от заключений ревизора и аудитора (П2)',
     0, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc97","cccccccc-cccc-cccc-cccc-cccccccccc9A"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc9C', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Запрос выписки из ЕИС у нотариуса',
     'Запрос выписки из реестра списков участников ЕИС у нотариуса. ⚠️ Только при нотариальном удостоверении (Н1)',
     0, 'FIXED_DAYS', 1, NULL, TRUE, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc9D', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Рассылка уведомлений и материалов участникам',
     'Уведомление о созыве ООСУ + повестка + пакет материалов за 30 кал. дней (П3)',
     2, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc9B"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc9E', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Уведомление нотариуса о собрании',
     'Уведомление нотариуса о дате/времени/месте собрания. ⚠️ Только при нотариальном удостоверении (П4)',
     4, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc93"]', TRUE, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc9F', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Приём доп. вопросов от участников — дедлайн',
     'Дедлайн приёма дополнительных вопросов от участников (−15 кал. дней до ООСУ) (Д1)',
     5, 'FIXED_DAYS', 0, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA0', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Рассмотрение доп. вопросов',
     'Рассмотрение и включение дополнительных вопросов в повестку (5 дней) (Д2)',
     5, 'FIXED_DAYS', 5, '["cccccccc-cccc-cccc-cccc-cccccccccc9F"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA1', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Уведомление об изменении повестки',
     'Рассылка уведомлений об изменении повестки (−10 кал. дней до ООСУ) (Д3)',
     10, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA0"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Фаза 5: Проведение ООСУ и завершение
    ('cccccccc-cccc-cccc-cccc-ccccccccccA2', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb45',
     'Проведение ООСУ',
     'Проведение собрания: регистрация, кворум, обсуждение, голосование, принятие решений (ОС1)',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc9D","cccccccc-cccc-cccc-cccc-cccccccccc9E","cccccccc-cccc-cccc-cccc-ccccccccccA1"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA3', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb45',
     'Оформление протокола',
     'Оформление протокола ООСУ, подписание, нотариальное удостоверение (если применимо) (ОС2)',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA2"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Регистрация изменений: только при смене директора или изменении устава
    ('cccccccc-cccc-cccc-cccc-ccccccccccA4', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb46',
     'Подача Р13014 в ФНС',
     'Заявление по форме Р13014 в течение 7 р.д. после ООСУ. ⚠️ Только при смене директора или изменении устава (ОС3)',
     0, 'FIXED_DAYS', 7, '["cccccccc-cccc-cccc-cccc-ccccccccccA3"]', NULL, TRUE, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA5', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb46',
     'Уведомление банка о смене директора',
     'Уведомление банка об изменении лица, имеющего право подписи. ⚠️ Только при смене директора (ОС4)',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA2"]', NULL, TRUE, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Завершение: фиксация результатов
    ('cccccccc-cccc-cccc-cccc-ccccccccccA6', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb47',
     'Фиксация результатов ООСУ в системе',
     'Внесение итогов голосования и решений ООСУ в систему',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA3"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA7', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb47',
     'Организация хранения документов ООСУ',
     'Формирование и передача полного комплекта документов ООСУ на постоянное хранение',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA6"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA8', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb47',
     'Организация исполнения решений ООСУ',
     'Контроль исполнения решений, принятых на ООСУ',
     2, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA7"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Шаблон «Подготовка к ВОСУ» (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60)
-- Каждый офер = одна задача (как в ГОСА/ВОСА). Предикаты управляют условным включением.
INSERT INTO tpl_org_intents (id, code, name, description, sort_order, is_for_ao, is_for_llc, requires_board_of_directors, created_at, created_by) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'VOSU', 'Подготовка к ВОСУ', 'Подготовка и проведение внеочередного общего собрания участников ООО (ст. 35 14-ФЗ)', 6, null, true, null, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_stages (id, intent_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days, created_at, created_by) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb51', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Получение требования', 'Приём и проверка требования о созыве ВОСУ, принятие решения', 1, 0, 'FIXED_DAYS', 5, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Подготовка', 'Определение даты, формирование повестки, подготовка материалов', 2, 5, 'FIXED_DAYS', 10, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Уведомление участников', 'Рассылка уведомлений, приём доп. вопросов, изменение повестки', 3, 15, 'FIXED_DAYS', 20, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb54', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Проведение ВОСУ', 'Регистрация, обсуждение, голосование, подсчёт', 4, 35, 'FIXED_DAYS', 2, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Оформление результатов', 'Составление протокола, нотариальное удостоверение, регистрация, рассылка', 5, 37, 'FIXED_DAYS', 10, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb56', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Завершение', 'Фиксация результатов, хранение, исполнение решений', 6, 47, 'FIXED_DAYS', 3, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Оферы ВОСУ: каждый офер — шаблон одной задачи, с predecessor_offer_ids и предикатами
INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days,
    predecessor_offer_ids, require_notary_confirmation, require_executive_body_a,
    require_mandatory_audit, require_revision_commission, created_at, created_by) VALUES
    -- Фаза 1: Получение требования
    ('cccccccc-cccc-cccc-cccc-cccccccccc71', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb51',
     'Приём и регистрация требования',
     'Регистрация входящего требования о созыве ВОСУ с датой получения',
     0, 'FIXED_DAYS', 1, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc72', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb51',
     'Проверка доли инициатора (≥10%)',
     'Сверка с реестром участников, подтверждение совокупной доли',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc71"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc73', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb51',
     'Проверка повестки на соответствие закону и уставу',
     'Проверка вопросов на компетенцию ОСУ и соответствие 14-ФЗ',
     2, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc72"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc74', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb51',
     'Принятие решения о созыве или мотивированный отказ',
     'Оформление решения гендиректора (5-дневный срок). В случае отказа — завершение процесса',
     3, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc73"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Фаза 2: Подготовка
    ('cccccccc-cccc-cccc-cccc-cccccccccc75', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52',
     'Определение даты и места ВОСУ',
     'Выбор даты в пределах 45 дней (75 — с избранием СД) с учётом 30-дневного уведомления',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc74"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc76', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52',
     'Формирование повестки дня',
     'Составление перечня вопросов ровно по требованию инициатора',
     1, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc75"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc77', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52',
     'Подготовка проектов решений',
     'Подготовка проектов решений по каждому вопросу повестки',
     1, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc76"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc78', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52',
     'Подготовка информационных материалов',
     'Подготовка материалов по вопросам повестки (при необходимости)',
     3, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc77"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc79', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52',
     'Сборка пакета для рассылки',
     'Формирование полного комплекта документов для отправки участникам',
     5, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc78"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Фаза 3: Уведомление участников
    ('cccccccc-cccc-cccc-cccc-cccccccccc7A', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53',
     'Подготовка уведомления о созыве',
     'Составление текста уведомления о созыве ВОСУ с повесткой и материалами',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc79"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc7B', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53',
     'Отправка уведомлений участникам',
     'Рассылка заказными письмами или способом по уставу за 30 дней до ВОСУ',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc7A"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc7C', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53',
     'Приём доп. вопросов от участников — дедлайн',
     'Дедлайн приёма дополнительных вопросов от участников (−15 кал. дней до ВОСУ)',
     2, 'FIXED_DAYS', 0, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc7D', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53',
     'Рассмотрение и включение доп. вопросов в повестку',
     'Рассмотрение доп. вопросов, включение в повестку (5-дневный срок)',
     2, 'FIXED_DAYS', 5, '["cccccccc-cccc-cccc-cccc-cccccccccc7C"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc7E', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53',
     'Уведомление об изменении повестки',
     'Рассылка уведомлений об изменении повестки (−10 кал. дней до ВОСУ)',
     7, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc7D"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Фаза 4: Проведение ВОСУ
    ('cccccccc-cccc-cccc-cccc-cccccccccc7F', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb54',
     'Регистрация участников и проверка полномочий',
     'Регистрация участников, проверка доверенностей представителей',
     0, 'FIXED_DAYS', 1, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc80', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb54',
     'Проведение собрания: обсуждение и голосование',
     'Обсуждение вопросов повестки, голосование, подсчёт, оглашение результатов',
     0, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc7B","cccccccc-cccc-cccc-cccc-cccccccccc7E","cccccccc-cccc-cccc-cccc-cccccccccc7F"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Фаза 5: Оформление результатов
    ('cccccccc-cccc-cccc-cccc-cccccccccc81', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55',
     'Составление протокола ВОСУ',
     'Подготовка и подписание протокола председателем и секретарём',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc80"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc82', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55',
     'Нотариальное удостоверение протокола',
     'Заверение протокола у нотариуса. ⚠️ Только при нотариальном удостоверении (если не заменено уставом на подписание всеми)',
     1, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc81"]', TRUE, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc83', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55',
     'Подача Р13014 в ФНС',
     'Заявление по форме Р13014 в течение 7 р.д. ⚠️ Только при смене директора или изменении устава',
     1, 'FIXED_DAYS', 7, '["cccccccc-cccc-cccc-cccc-cccccccccc81"]', NULL, TRUE, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc84', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55',
     'Рассылка копий протокола участникам',
     'Отправка копий протокола всем участникам в разумный срок',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc81"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    -- Фаза 6: Завершение
    ('cccccccc-cccc-cccc-cccc-cccccccccc85', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb56',
     'Фиксация результатов ВОСУ в системе',
     'Внесение итогов голосования и решений ВОСУ в систему',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc81"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc86', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb56',
     'Организация хранения документов ВОСУ',
     'Формирование и передача полного комплекта документов ВОСУ на постоянное хранение',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc85"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc87', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb56',
     'Организация исполнения решений ВОСУ',
     'Контроль исполнения решений, принятых на ВОСУ',
     2, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc86"]', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Типовые уставы ООО (Приказ Минэкономразвития № 411 от 01.08.2018)
-- Номера 01–09 с ведущим нулём в соответствии с форматом ФНС (Р11001, Р13014)
INSERT INTO ref_standard_charter (id, number, exit_allowed, transfer_to_participants_without_consent, transfer_to_third_parties_without_consent, preemptive_right, inheritance_without_consent, executive_body, decision_confirmation_by_all_sign, created_at, created_by) VALUES
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01', '01', FALSE, TRUE,  FALSE, TRUE, TRUE, 'A', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02', '02', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'A', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03', '03', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'A', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee04', '04', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'A', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee05', '05', FALSE, FALSE, FALSE, TRUE, TRUE, 'A', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee06', '06', FALSE, FALSE, FALSE, TRUE, TRUE, 'A', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee07', '07', FALSE, TRUE,  FALSE, TRUE, TRUE, 'B', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee08', '08', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'B', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee09', '09', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'B', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee10', '10', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'B', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee11', '11', FALSE, FALSE, FALSE, TRUE, TRUE, 'B', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee12', '12', FALSE, FALSE, FALSE, TRUE, TRUE, 'B', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee13', '13', FALSE, TRUE,  FALSE, TRUE, TRUE, 'C', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee14', '14', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'C', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee15', '15', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'C', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee16', '16', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'C', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee17', '17', FALSE, FALSE, FALSE, TRUE, TRUE, 'C', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee18', '18', FALSE, FALSE, FALSE, TRUE, TRUE, 'C', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee19', '19', FALSE, TRUE,  FALSE, TRUE, TRUE, 'A', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee20', '20', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'A', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee21', '21', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'A', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee22', '22', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'A', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee23', '23', FALSE, FALSE, FALSE, TRUE, TRUE, 'A', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee24', '24', FALSE, FALSE, FALSE, TRUE, TRUE, 'A', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee25', '25', FALSE, TRUE,  FALSE, TRUE, TRUE, 'B', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee26', '26', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'B', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee27', '27', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'B', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee28', '28', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'B', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee29', '29', FALSE, FALSE, FALSE, TRUE, TRUE, 'B', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee30', '30', FALSE, FALSE, FALSE, TRUE, TRUE, 'B', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee31', '31', FALSE, TRUE,  FALSE, TRUE, TRUE, 'C', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee32', '32', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'C', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee33', '33', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'C', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee34', '34', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'C', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee35', '35', FALSE, FALSE, FALSE, TRUE, TRUE, 'C', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee36', '36', FALSE, FALSE, FALSE, TRUE, TRUE, 'C', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT (number) DO NOTHING;


-- ============================================================================
