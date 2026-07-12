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
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2','VOSA','Внеочередное общее собрание акционеров','ВОСА'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3','OOSU','Очередное общее собрание участников','ООСУ'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4','VOSU','Внеочередное общее собрание участников','ВОСУ')
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
INSERT INTO tpl_org_intents (id, code, name, description, sort_order, is_for_ao, is_for_llc, requires_board_of_directors) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'GOSA', 'Подготовка к ГОСА', 'Подготовка к годовому общему собранию акционеров', 1, true, null, true),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'BOARD_MEETING', 'Заседание Совета директоров', 'Стандартный цикл проведения заседания СД', 2, true, null, true)
ON CONFLICT DO NOTHING;

-- Этапы ГОСА (5 фаз)
INSERT INTO tpl_org_stages (id, intent_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Фаза 1: Запуск и предложения акционеров', 'Сбор предложений, постановка задач бухгалтерии, формирование повестки', 1, 0, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Фаза 2: Проверка отчётности', 'Ревизия, аудит, приём заключений', 2, 24, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Фаза 3: Решение СД о созыве и подготовка списка', 'Утверждение отчёта, решение о созыве, запрос и проверка списка акционеров', 3, 40, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Фаза 4: Подготовка и уведомление', 'Формирование повестки, сборка материалов, бюллетени, рассылка, уведомление нотариуса', 4, 82, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10', 'Фаза 5: ГОСА и завершение', 'Проведение собрания, протокол, ФНС, банк, регистратор, раскрытие', 5, 125, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb11', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Созыв заседания', 'Уведомление членов СД о дате и повестке', 1, 0, 'FIXED_DAYS', 5),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb12', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Проведение заседания', 'Обсуждение вопросов повестки', 2, 5, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb13', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Подписание протокола', 'Оформление и подписание протокола заседания', 3, 6, 'FIXED_DAYS', 3),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb14', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20', 'Завершение', 'Фиксация результатов заседания СД в системе, хранение документов, исполнение решений', 4, 9, 'FIXED_DAYS', 1)
ON CONFLICT DO NOTHING;

-- Задачи ГОСА (31 задача из gosa-gantt.md)
INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days, predecessor_offer_ids) VALUES
    ('cccccccc-cccc-cccc-cccc-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'Постановка задачи бухгалтерии', 'Постановка задачи бухгалтерии', 0, 'FIXED_DAYS', 1, NULL),
    ('cccccccc-cccc-cccc-cccc-000000000002', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'Подготовка годового отчёта и баланса', 'Подготовка годового отчёта и баланса', 3, 'FIXED_DAYS', 15, '["cccccccc-cccc-cccc-cccc-000000000001"]'),
    ('cccccccc-cccc-cccc-cccc-000000000003', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'Приём предложений акционеров в повестку (дедлайн)', 'Приём предложений акционеров в повестку (дедлайн)', 21, 'FIXED_DAYS', 0, NULL),
    ('cccccccc-cccc-cccc-cccc-000000000004', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'Заседание СД: рассмотрение предложений акционеров', 'Заседание СД: рассмотрение предложений акционеров', 24, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-000000000003"]'),
    ('cccccccc-cccc-cccc-cccc-000000000005', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Заключение договора на аудит', 'Заключение договора на аудит', -24, 'FIXED_DAYS', 3, NULL),
    ('cccccccc-cccc-cccc-cccc-000000000006', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Передача документов ревизору', 'Передача документов ревизору', 0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000002"]'),
    ('cccccccc-cccc-cccc-cccc-000000000007', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Передача документов аудитору', 'Передача документов аудитору', 0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000002","cccccccc-cccc-cccc-cccc-000000000005"]'),
    ('cccccccc-cccc-cccc-cccc-000000000008', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Проверка отчётности ревизором', 'Проверка отчётности ревизором', 1, 'FIXED_DAYS', 10, '["cccccccc-cccc-cccc-cccc-000000000006"]'),
    ('cccccccc-cccc-cccc-cccc-000000000009', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Проведение аудита', 'Проведение аудита', 1, 'FIXED_DAYS', 40, '["cccccccc-cccc-cccc-cccc-000000000007"]'),
    ('cccccccc-cccc-cccc-cccc-000000000010', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Приём заключения ревизора', 'Приём заключения ревизора', 15, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000008"]'),
    ('cccccccc-cccc-cccc-cccc-000000000011', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'Приём аудиторского заключения', 'Приём аудиторского заключения', 57, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000009"]'),
    ('cccccccc-cccc-cccc-cccc-000000000012', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Заседание СД: утверждение годового отчёта', 'Заседание СД: утверждение годового отчёта', 0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000010"]'),
    ('cccccccc-cccc-cccc-cccc-000000000013', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Заседание СД: решение о созыве ГОСА', 'Заседание СД: решение о созыве ГОСА', 42, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000011"]'),
    ('cccccccc-cccc-cccc-cccc-000000000014', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Запрос списка акционеров у регистратора', 'Запрос списка акционеров у регистратора', 43, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000013"]'),
    ('cccccccc-cccc-cccc-cccc-000000000015', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Получение быстрого списка от регистратора', 'Получение быстрого списка от регистратора', 44, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000014"]'),
    ('cccccccc-cccc-cccc-cccc-000000000016', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Получение полного списка (с раскрытием номинальных)', 'Получение полного списка (с раскрытием номинальных)', 47, 'FIXED_DAYS', 3, '["cccccccc-cccc-cccc-cccc-000000000015"]'),
    ('cccccccc-cccc-cccc-cccc-000000000017', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'Проверка списка (валидация)', 'Проверка списка (валидация)', 50, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000016"]'),
    ('cccccccc-cccc-cccc-cccc-000000000018', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Формирование повестки и проектов решений', 'Формирование повестки и проектов решений', 0, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-000000000013"]'),
    ('cccccccc-cccc-cccc-cccc-000000000019', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Запрос выписки из ЕИС у нотариуса', 'Запрос выписки из ЕИС у нотариуса', 12, 'FIXED_DAYS', 1, NULL),
    ('cccccccc-cccc-cccc-cccc-000000000020', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Сборка пакета материалов к ГОСА', 'Сборка пакета материалов к ГОСА', 9, 'FIXED_DAYS', 3, '["cccccccc-cccc-cccc-cccc-000000000017","cccccccc-cccc-cccc-cccc-000000000018"]'),
    ('cccccccc-cccc-cccc-cccc-000000000021', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Подготовка бюллетеней для голосования', 'Подготовка бюллетеней для голосования', 14, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-000000000020"]'),
    ('cccccccc-cccc-cccc-cccc-000000000022', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Рассылка уведомлений и бюллетеней акционерам', 'Рассылка уведомлений и бюллетеней акционерам', 19, 'FIXED_DAYS', 3, '["cccccccc-cccc-cccc-cccc-000000000017","cccccccc-cccc-cccc-cccc-000000000021"]'),
    ('cccccccc-cccc-cccc-cccc-000000000023', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Доступ к материалам для акционеров', 'Доступ к материалам для акционеров', 23, 'FIXED_DAYS', 0, '["cccccccc-cccc-cccc-cccc-000000000022"]'),
    ('cccccccc-cccc-cccc-cccc-000000000024', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Уведомление нотариуса о собрании', 'Уведомление нотариуса о собрании', 26, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000018"]'),
    ('cccccccc-cccc-cccc-cccc-000000000025', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04', 'Уведомление об изменении повестки (если есть доп. вопросы)', 'Уведомление об изменении повестки (если есть доп. вопросы)', 26, 'FIXED_DAYS', 1, NULL),
    ('cccccccc-cccc-cccc-cccc-000000000026', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Проведение ГОСА', 'Проведение ГОСА', 0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000022","cccccccc-cccc-cccc-cccc-000000000024","cccccccc-cccc-cccc-cccc-000000000025"]'),
    ('cccccccc-cccc-cccc-cccc-000000000027', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Оформление протокола ГОСА', 'Оформление протокола ГОСА', 1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000026"]'),
    ('cccccccc-cccc-cccc-cccc-000000000028', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Уведомление банка о смене директора', 'Уведомление банка о смене директора', 1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000026"]'),
    ('cccccccc-cccc-cccc-cccc-000000000029', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Уведомление регистратора об изменениях', 'Уведомление регистратора об изменениях', 1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000026"]'),
    ('cccccccc-cccc-cccc-cccc-000000000030', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Раскрытие протокола (ПАО)', 'Раскрытие протокола (ПАО)', 2, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000027"]'),
    ('cccccccc-cccc-cccc-cccc-000000000031', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05', 'Подача Р13014 в ФНС', 'Подача Р13014 в ФНС', 4, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-000000000027"]')
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_intents (id, code, name, description, sort_order, is_for_ao, is_for_llc, requires_board_of_directors) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'VOSA', 'Подготовка к ВОСА', 'Подготовка к внеочередному общему собранию акционеров', 3, true, null, true)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_stages (id, intent_id, name, description, start_offset_days, deadline_rule, deadline_days) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb21', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Выдвижение требования', 'Получение и проверка требования о созыве ВОСА', 0, 'FIXED_DAYS', 5),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb22', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Подготовка повестки', 'Формирование повестки дня ВОСА', 5, 'FIXED_DAYS', 5),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb23', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Созыв собрания', 'Уведомление акционеров и подготовка бюллетеней', 10, 'FIXED_DAYS', 20),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb24', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Проведение ВОСА', 'Регистрация, голосование, подведение итогов', 30, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb25', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30', 'Завершение', 'Фиксация результатов ВОСА в системе, хранение документов, исполнение решений', 31, 'FIXED_DAYS', 1)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days) VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc21', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb21', 'Проверка требования', 'Проверка легитимности требования о созыве ВОСА', 0, 'FIXED_DAYS', 3),
    ('cccccccc-cccc-cccc-cccc-cccccccccc22', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb21', 'Принятие решения о созыве', 'Решение СД о созыве либо отказе в созыве ВОСА', 3, 'FIXED_DAYS', 2),
    ('cccccccc-cccc-cccc-cccc-cccccccccc23', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb22', 'Утверждение вопросов', 'Утверждение перечня вопросов повестки дня', 0, 'FIXED_DAYS', 5),
    ('cccccccc-cccc-cccc-cccc-cccccccccc24', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb23', 'Рассылка уведомлений', 'Уведомление акционеров о проведении ВОСА', 0, 'FIXED_DAYS', 5),
    ('cccccccc-cccc-cccc-cccc-cccccccccc25', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb23', 'Формирование бюллетеней', 'Подготовка бюллетеней для голосования', 5, 'FIXED_DAYS', 5),
    ('cccccccc-cccc-cccc-cccc-cccccccccc26', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb24', 'Регистрация участников', 'Регистрация акционеров и проверка полномочий', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc27', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb24', 'Голосование и подсчёт', 'Проведение голосования и подсчёт голосов', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc28', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb25', 'Фиксация результатов', 'Фиксация итогов ВОСА в системе, хранение и исполнение решений', 0, 'FIXED_DAYS', 1)
ON CONFLICT DO NOTHING;

-- Задачи ВОСА (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30)
INSERT INTO tpl_org_tasks (id, offer_id, name, description, sort_order,
    
    require_notary_confirmation, require_all_sign_confirmation, require_committees,
    require_board_regulation, require_custom_charter, require_executive_body_a,
    require_board_of_directors, require_document_flow_legal_electronic) VALUES
    -- Стадия 1: Выдвижение требования / Проверка требования (cc21)
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee20', 'cccccccc-cccc-cccc-cccc-cccccccccc21', 'Приём и регистрация требования', 'Регистрация входящего требования о созыве ВОСА с датой получения', 1, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee21', 'cccccccc-cccc-cccc-cccc-cccccccccc21', 'Проверка доли инициатора (≥10%)', 'Сверка с реестром акционеров, подтверждение совокупной доли голосующих акций', 2, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee22', 'cccccccc-cccc-cccc-cccc-cccccccccc21', 'Проверка повестки на соответствие закону и уставу', 'Проверка вопросов на компетенцию ОСА и соответствие 208-ФЗ', 3, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Стадия 1: Выдвижение требования / Принятие решения (cc22)
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee23', 'cccccccc-cccc-cccc-cccc-cccccccccc22', 'Принятие решения о созыве или мотивированный отказ', 'Заседание СД: решение о созыве ВОСА либо отказ с указанием основания (5 р.д.)', 1, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Стадия 2: Подготовка повестки / Утверждение вопросов (cc23)
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee24', 'cccccccc-cccc-cccc-cccc-cccccccccc23', 'Определение даты и места ВОСА', 'Выбор даты в пределах 40 (75 — с избранием СД) дней с учётом 21-дневного уведомления', 1, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee25', 'cccccccc-cccc-cccc-cccc-cccccccccc23', 'Формирование повестки дня (из требования)', 'Составление перечня вопросов ровно по требованию инициатора', 2, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee26', 'cccccccc-cccc-cccc-cccc-cccccccccc23', 'Подготовка проектов решений', 'Подготовка проектов решений по каждому вопросу повестки', 3, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee27', 'cccccccc-cccc-cccc-cccc-cccccccccc23', 'Подготовка информационных материалов', 'Подготовка материалов по вопросам повестки (при необходимости)', 4, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee28', 'cccccccc-cccc-cccc-cccc-cccccccccc23', 'Сборка пакета материалов для акционеров', 'Формирование полного комплекта документов для отправки акционерам', 5, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Стадия 3: Созыв собрания / Рассылка уведомлений (cc24)
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee29', 'cccccccc-cccc-cccc-cccc-cccccccccc24', 'Запрос списка акционеров у регистратора', 'Подготовка и направление запроса регистратору на дату фиксации СД', 1, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee2A', 'cccccccc-cccc-cccc-cccc-cccccccccc24', 'Получение быстрого списка от регистратора', 'Получение списка прямых владельцев (след. раб. день)', 2, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee2B', 'cccccccc-cccc-cccc-cccc-cccccccccc24', 'Получение полного списка с раскрытием номинальных держателей', 'Раскрытие конечных владельцев за номинальными держателями (до 3 р.д.)', 3, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee2C', 'cccccccc-cccc-cccc-cccc-cccccccccc24', 'Проверка списка (валидация)', 'Сверка адресов, расчёт кворума, проверка паспортных данных', 4, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee2D', 'cccccccc-cccc-cccc-cccc-cccccccccc24', 'Рассылка уведомлений и бюллетеней акционерам', 'Заказное письмо за 21 кал. день до ВОСА', 5, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee2E', 'cccccccc-cccc-cccc-cccc-cccccccccc24', 'Уведомление нотариуса о собрании', 'Уведомление о дате/времени/месте собрания, передача документов', 6, TRUE,  NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Стадия 3: Созыв собрания / Формирование бюллетеней (cc25)
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee2F', 'cccccccc-cccc-cccc-cccc-cccccccccc25', 'Подготовка бюллетеней для голосования', 'Бюллетени «за»/«против»/«воздержался» по каждому вопросу повестки', 1, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Стадия 4: Проведение ВОСА / Регистрация участников (cc26)
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee30', 'cccccccc-cccc-cccc-cccc-cccccccccc26', 'Регистрация участников и проверка полномочий', 'Проверка доверенностей представителей, выдача бюллетеней', 1, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee31', 'cccccccc-cccc-cccc-cccc-cccccccccc26', 'Проверка кворума (>50%)', 'Подтверждение наличия кворума для правомочности собрания', 2, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Стадия 4: Проведение ВОСА / Голосование и подсчёт (cc27)
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee32', 'cccccccc-cccc-cccc-cccc-cccccccccc27', 'Проведение собрания: обсуждение и голосование', 'Обсуждение вопросов повестки, голосование, подсчёт, оглашение', 1, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee33', 'cccccccc-cccc-cccc-cccc-cccccccccc27', 'Оформление протокола ВОСА', 'Подготовка и подписание протокола председателем и секретарём', 2, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Стадия 5: Завершение / Фиксация результатов (cc28) — дополнительные задачи
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee34', 'cccccccc-cccc-cccc-cccc-cccccccccc28', 'Раскрытие протокола (ПАО)', 'Публикация протокола в ленте новостей (Интерфакс) в течение 4 р.д.', 4, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee35', 'cccccccc-cccc-cccc-cccc-cccccccccc28', 'Подача Р13014 в ФНС', 'Заявление по форме Р13014 в течение 7 р.д. (при смене директора/устава)', 5, NULL, NULL, NULL, NULL, NULL, TRUE,  TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee36', 'cccccccc-cccc-cccc-cccc-cccccccccc28', 'Уведомление банка о смене директора', 'Уведомление банка об изменении лица, имеющего право подписи', 6, NULL, NULL, NULL, NULL, NULL, TRUE,  TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee37', 'cccccccc-cccc-cccc-cccc-cccccccccc28', 'Уведомление регистратора об изменениях', 'Уведомление регистратора о смене директора или устава', 7, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee38', 'cccccccc-cccc-cccc-cccc-cccccccccc28', 'Рассылка копий протокола акционерам', 'Отправка копий протокола всем акционерам в разумный срок', 8, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_intents (id, code, name, description, sort_order, is_for_ao, is_for_llc, requires_board_of_directors) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'FIRST_BOARD', 'Подготовка и проведение первого СД', 'Подготовка и проведение первого заседания Совета директоров после избрания', 4, true, true, true)
ON CONFLICT DO NOTHING;

-- Этапы FIRST_BOARD
INSERT INTO tpl_org_stages (id, intent_id, name, description, start_offset_days, deadline_rule, deadline_days) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb31', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'Подготовка (до заседания)', 'Подготовка шаблона протокола, уведомление членов СД, сбор материалов', 0, 'FIXED_DAYS', 7),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'Проведение первого заседания', 'Открытие, голосование по избранию председателя, зама, секретаря, комитетов, закрытие', 7, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40', 'После заседания (результаты)', 'Получение УКЭП/МЧД, оформление и подписание протокола, рассылка, хранение', 8, 'FIXED_DAYS', 5)
ON CONFLICT DO NOTHING;

-- Оферы FIRST_BOARD
INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days) VALUES
    -- Stage 1: Подготовка
    ('cccccccc-cccc-cccc-cccc-cccccccccc31', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb31', 'Шаблон протокола и подготовка к избранию', 'Подготовка шаблона протокола и материалов к выборам', 0, 'FIXED_DAYS', 7),
    ('cccccccc-cccc-cccc-cccc-cccccccccc32', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb31', 'Уведомление и сбор материалов', 'Уведомление членов СД, сбор проектов решений, бюллетеней, мнений', 0, 'FIXED_DAYS', 7),
    -- Stage 2: Проведение заседания
    ('cccccccc-cccc-cccc-cccc-cccccccccc33', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Открытие заседания и проверка кворума', 'Открытие заседания, проверка кворума', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc34', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Избрание председателя СД', 'Голосование, подсчёт, объявление результатов', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc35', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Избрание заместителя председателя СД', 'Голосование, подсчёт, объявление результатов', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc36', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Избрание секретаря СД', 'Голосование, подсчёт, объявление результатов', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc37', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Формирование комитетов СД', 'Голосование, подсчёт, объявление результатов', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc38', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb32', 'Иные вопросы и закрытие заседания', 'Голосование по иным вопросам, подсчёт, объявление, закрытие', 0, 'FIXED_DAYS', 1),
    -- Stage 3: После заседания
    ('cccccccc-cccc-cccc-cccc-cccccccccc41', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Получение УКЭП', 'Получение УКЭП для председателя, заместителя, секретаря', 0, 'FIXED_DAYS', 5),
    ('cccccccc-cccc-cccc-cccc-cccccccccc42', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Оформление и проверка протокола', 'Оформление финального протокола, проверка УКЭП/МЧД', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc43', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Бумажное подписание протокола', 'Подписание бумажного протокола и внесение скана', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc44', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Электронное подписание протокола', 'Подписание протокола с использованием УКЭП', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc45', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33', 'Завершение', 'Рассылка, подготовка для регуляторов, хранение, исполнение', 0, 'FIXED_DAYS', 3)
ON CONFLICT DO NOTHING;

-- Задачи FIRST_BOARD (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa40)
INSERT INTO tpl_org_tasks (id, offer_id, name, description, sort_order, assigned_board_role_id,
    require_notary_confirmation, require_all_sign_confirmation, require_committees,
    require_board_regulation, require_custom_charter, require_executive_body_a,
    require_board_of_directors, require_document_flow_legal_electronic) VALUES
    -- Stage 1, Offer 1: Шаблон протокола и подготовка к избранию
    ('dddddddd-dddd-dddd-dddd-dddddddddd01', 'cccccccc-cccc-cccc-cccc-cccccccccc31', 'Подготовка шаблона протокола первого заседания', 'Создание шаблона протокола по ст. 68 п. 4 208-ФЗ', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd02', 'cccccccc-cccc-cccc-cccc-cccccccccc31', 'Подготовка к избранию председателя СД', 'Подготовка материалов к голосованию по председателю', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd03', 'cccccccc-cccc-cccc-cccc-cccccccccc31', 'Подготовка к избранию заместителя председателя СД', 'Подготовка материалов к голосованию по заместителю', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff2', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd04', 'cccccccc-cccc-cccc-cccc-cccccccccc31', 'Подготовка к избранию секретаря СД', 'Подготовка материалов к голосованию по секретарю', 4, 'ffffffff-ffff-ffff-ffff-fffffffffff6', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd05', 'cccccccc-cccc-cccc-cccc-cccccccccc31', 'Подготовка к формированию комитетов СД', 'Подготовка материалов к голосованию по комитетам', 5, NULL, NULL, NULL, TRUE, NULL, NULL, NULL, TRUE, NULL),
    -- Stage 1, Offer 2: Уведомление и сбор материалов
    ('dddddddd-dddd-dddd-dddd-dddddddddd06', 'cccccccc-cccc-cccc-cccc-cccccccccc32', 'Уведомление членов СД о дате, времени, месте и повестке заседания', 'Рассылка уведомлений по ст. 68 п. 1 208-ФЗ', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd07', 'cccccccc-cccc-cccc-cccc-cccccccccc32', 'Сбор и консолидация проектов решений по вопросам повестки', 'Сбор проектов документов от членов СД', 2, NULL, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd08', 'cccccccc-cccc-cccc-cccc-cccccccccc32', 'Подготовка и рассылка бюллетеней для голосования', 'При заочной или смешанной форме заседания', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd09', 'cccccccc-cccc-cccc-cccc-cccccccccc32', 'Сбор и обработка письменных мнений членов СД', 'Сбор письменных мнений отсутствующих членов', 4, NULL, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Stage 2, Offer 1: Открытие заседания и проверка кворума
    ('dddddddd-dddd-dddd-dddd-dddddddddd11', 'cccccccc-cccc-cccc-cccc-cccccccccc33', 'Открытие заседания', 'Открытие заседания председательствующим', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd12', 'cccccccc-cccc-cccc-cccc-cccccccccc33', 'Проверка кворума', 'Проверка кворума по ст. 68 п. 2 208-ФЗ', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1', TRUE,  NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Stage 2, Offer 2: Избрание председателя СД
    ('dddddddd-dddd-dddd-dddd-dddddddddd13', 'cccccccc-cccc-cccc-cccc-cccccccccc34', 'Голосование по избранию председателя СД', 'Голосование членов СД', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd14', 'cccccccc-cccc-cccc-cccc-cccccccccc34', 'Подсчёт голосов по избранию председателя СД', 'Подсчёт голосов председательствующим', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd15', 'cccccccc-cccc-cccc-cccc-cccccccccc34', 'Подведение итогов и объявление результатов', 'Объявление результатов голосования', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Stage 2, Offer 3: Избрание заместителя председателя СД
    ('dddddddd-dddd-dddd-dddd-dddddddddd16', 'cccccccc-cccc-cccc-cccc-cccccccccc35', 'Голосование по избранию заместителя председателя СД', 'Голосование членов СД', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd17', 'cccccccc-cccc-cccc-cccc-cccccccccc35', 'Подсчёт голосов по избранию заместителя председателя СД', 'Подсчёт голосов председательствующим', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd18', 'cccccccc-cccc-cccc-cccc-cccccccccc35', 'Подведение итогов и объявление результатов', 'Объявление результатов голосования', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Stage 2, Offer 4: Избрание секретаря СД
    ('dddddddd-dddd-dddd-dddd-dddddddddd19', 'cccccccc-cccc-cccc-cccc-cccccccccc36', 'Голосование по избранию секретаря СД', 'Голосование членов СД', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd20', 'cccccccc-cccc-cccc-cccc-cccccccccc36', 'Подсчёт голосов по избранию секретаря СД', 'Подсчёт голосов председательствующим', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd21', 'cccccccc-cccc-cccc-cccc-cccccccccc36', 'Подведение итогов и объявление результатов', 'Объявление результатов голосования', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Stage 2, Offer 5: Формирование комитетов СД
    ('dddddddd-dddd-dddd-dddd-dddddddddd22', 'cccccccc-cccc-cccc-cccc-cccccccccc37', 'Голосование по формированию комитетов СД', 'Голосование членов СД', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3', NULL, NULL, TRUE, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd23', 'cccccccc-cccc-cccc-cccc-cccccccccc37', 'Подсчёт голосов по формированию комитетов СД', 'Подсчёт голосов секретарём', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff6', NULL, NULL, TRUE, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd24', 'cccccccc-cccc-cccc-cccc-cccccccccc37', 'Подведение итогов и объявление результатов', 'Объявление результатов голосования', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, TRUE, NULL, NULL, NULL, TRUE, NULL),
    -- Stage 2, Offer 6: Иные вопросы и закрытие заседания
    ('dddddddd-dddd-dddd-dddd-dddddddddd25', 'cccccccc-cccc-cccc-cccc-cccccccccc38', 'Голосование по иным вопросам повестки', 'Голосование по дополнительным вопросам', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd26', 'cccccccc-cccc-cccc-cccc-cccccccccc38', 'Подсчёт голосов по иным вопросам повестки', 'Подсчёт голосов секретарём', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff6', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd27', 'cccccccc-cccc-cccc-cccc-cccccccccc38', 'Подведение итогов и объявление результатов', 'Объявление результатов голосования', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd28', 'cccccccc-cccc-cccc-cccc-cccccccccc38', 'Закрытие заседания', 'Закрытие заседания председательствующим', 4, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Stage 3, Offer 1: Получение УКЭП
    ('dddddddd-dddd-dddd-dddd-dddddddddd31', 'cccccccc-cccc-cccc-cccc-cccccccccc41', 'Получение УКЭП для избранного председателя СД', 'При ЮЗЭДО; 63-ФЗ', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff1', NULL, NULL, NULL, NULL, TRUE,  NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd32', 'cccccccc-cccc-cccc-cccc-cccccccccc41', 'Получение УКЭП для избранного заместителя председателя СД', 'При ЮЗЭДО; 63-ФЗ', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff2', NULL, NULL, NULL, NULL, TRUE,  NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd33', 'cccccccc-cccc-cccc-cccc-cccccccccc41', 'Получение УКЭП для избранного секретаря СД', 'При ЮЗЭДО; 63-ФЗ', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff6', NULL, NULL, NULL, NULL, TRUE,  NULL, TRUE, NULL),
    -- Stage 3, Offer 2: Оформление и проверка протокола
    ('dddddddd-dddd-dddd-dddd-dddddddddd37', 'cccccccc-cccc-cccc-cccc-cccccccccc42', 'Оформление финального протокола заседания', 'Составление протокола по ст. 68 п. 4 208-ФЗ', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd38', 'cccccccc-cccc-cccc-cccc-cccccccccc42', 'Проверка наличия УКЭП у всех подписантов', 'При ЮЗЭДО; 63-ФЗ', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff1', TRUE,  NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Stage 3, Offer 3: Бумажное подписание протокола
    ('dddddddd-dddd-dddd-dddd-dddddddddd41', 'cccccccc-cccc-cccc-cccc-cccccccccc43', 'Бумажное подписание протокола председателем', 'Подписание бумажного экземпляра', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff1', TRUE,  NULL, NULL, NULL, NULL, TRUE,  TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd42', 'cccccccc-cccc-cccc-cccc-cccccccccc43', 'Бумажное подписание протокола заместителем председателя', 'При отсутствии председателя', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff2', TRUE,  NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd43', 'cccccccc-cccc-cccc-cccc-cccccccccc43', 'Бумажное подписание протокола секретарем', 'При наличии права подписи', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff6', TRUE,  NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd44', 'cccccccc-cccc-cccc-cccc-cccccccccc43', 'Внесение электронного образа подписанного документа в систему', 'Сканирование и загрузка в систему', 4, NULL, TRUE,  NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    -- Stage 3, Offer 4: Электронное подписание протокола
    ('dddddddd-dddd-dddd-dddd-dddddddddd45', 'cccccccc-cccc-cccc-cccc-cccccccccc44', 'Подписание протокола председателем с использованием УКЭП', 'При ЮЗЭДО; ст. 68 п. 4 208-ФЗ', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff1', TRUE,  NULL, NULL, NULL, TRUE,  TRUE,  TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd46', 'cccccccc-cccc-cccc-cccc-cccccccccc44', 'Подписание протокола заместителем председателя с использованием УКЭП', 'При отсутствии председателя и ЮЗЭДО', 2, 'ffffffff-ffff-ffff-ffff-fffffffffff2', TRUE,  NULL, NULL, NULL, TRUE,  NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd47', 'cccccccc-cccc-cccc-cccc-cccccccccc44', 'Подписание протокола секретарем с использованием УКЭП', 'При наличии права подписи и ЮЗЭДО', 3, 'ffffffff-ffff-ffff-ffff-fffffffffff6', TRUE,  NULL, NULL, NULL, TRUE,  NULL, TRUE, NULL),
    -- Stage 3, Offer 5: Завершение
    ('dddddddd-dddd-dddd-dddd-dddddddddd48', 'cccccccc-cccc-cccc-cccc-cccccccccc45', 'Рассылка копий протокола и решений членам СД', 'Рассылка по ст. 68 п. 4 208-ФЗ', 1, 'ffffffff-ffff-ffff-ffff-fffffffffff3', NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd49', 'cccccccc-cccc-cccc-cccc-cccccccccc45', 'Обеспечение готовности пакета документов для регулирующих органов', 'Подготовка документов для ЦБ РФ и других органов', 2, NULL, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd50', 'cccccccc-cccc-cccc-cccc-cccccccccc45', 'Организация постоянного хранения полного комплекта документов заседания СД', 'Хранение по ст. 89 208-ФЗ', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd51', 'cccccccc-cccc-cccc-cccc-cccccccccc45', 'Организация исполнения принятых решений', 'Контроль исполнения по ст. 69 208-ФЗ', 4, NULL, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL)
ON CONFLICT DO NOTHING;

-- Задачи «Завершение» для ГОСА (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10)
INSERT INTO tpl_org_tasks (id, offer_id, name, description, sort_order) VALUES
    ('dddddddd-dddd-dddd-dddd-dddddddddd61', 'cccccccc-cccc-cccc-cccc-000000000031', 'Фиксация результатов ГОСА в системе', 'Внесение итогов голосования и решений ГОСА в систему', 1),
    ('dddddddd-dddd-dddd-dddd-dddddddddd62', 'cccccccc-cccc-cccc-cccc-000000000031', 'Организация хранения документов ГОСА', 'Формирование и передача полного комплекта документов ГОСА на постоянное хранение', 2),
    ('dddddddd-dddd-dddd-dddd-dddddddddd63', 'cccccccc-cccc-cccc-cccc-000000000031', 'Организация исполнения решений ГОСА', 'Контроль исполнения решений, принятых на ГОСА', 3)
ON CONFLICT DO NOTHING;

-- Задачи «Завершение» для Заседания СД (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20)
INSERT INTO tpl_org_tasks (id, offer_id, name, description, sort_order) VALUES
    ('dddddddd-dddd-dddd-dddd-dddddddddd71', 'cccccccc-cccc-cccc-cccc-cccccccccc16', 'Фиксация результатов заседания СД в системе', 'Внесение итогов голосования и решений заседания СД в систему', 1),
    ('dddddddd-dddd-dddd-dddd-dddddddddd72', 'cccccccc-cccc-cccc-cccc-cccccccccc16', 'Организация хранения документов заседания СД', 'Формирование и передача полного комплекта документов заседания на постоянное хранение', 2),
    ('dddddddd-dddd-dddd-dddd-dddddddddd73', 'cccccccc-cccc-cccc-cccc-cccccccccc16', 'Организация исполнения решений заседания СД', 'Контроль исполнения решений, принятых на заседании СД', 3)
ON CONFLICT DO NOTHING;

-- Задачи «Завершение» для ВОСА (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa30)
INSERT INTO tpl_org_tasks (id, offer_id, name, description, sort_order,
    
    require_notary_confirmation, require_all_sign_confirmation, require_committees,
    require_board_regulation, require_custom_charter, require_executive_body_a,
    require_board_of_directors, require_document_flow_legal_electronic) VALUES
    ('dddddddd-dddd-dddd-dddd-dddddddddd81', 'cccccccc-cccc-cccc-cccc-cccccccccc28', 'Фиксация результатов ВОСА в системе', 'Внесение итогов голосования и решений ВОСА в систему', 1, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd82', 'cccccccc-cccc-cccc-cccc-cccccccccc28', 'Организация хранения документов ВОСА', 'Формирование и передача полного комплекта документов ВОСА на постоянное хранение', 2, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd83', 'cccccccc-cccc-cccc-cccc-cccccccccc28', 'Организация исполнения решений ВОСА', 'Контроль исполнения решений, принятых на ВОСА', 3, NULL, NULL, NULL, NULL, NULL, NULL, TRUE, NULL)
ON CONFLICT DO NOTHING;

-- Оферы для шаблона «Заседание Совета директоров» (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa20)
INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days) VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc11', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb11', 'Уведомление членов СД', 'Подготовка и рассылка уведомлений о созыве', 0, 'FIXED_DAYS', 3),
    ('cccccccc-cccc-cccc-cccc-cccccccccc12', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb11', 'Сбор материалов', 'Сбор и подготовка материалов к заседанию', 3, 'FIXED_DAYS', 2),
    ('cccccccc-cccc-cccc-cccc-cccccccccc13', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb12', 'Обсуждение вопросов', 'Проведение обсуждения вопросов повестки', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc14', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb12', 'Голосование', 'Проведение голосования по каждому вопросу', 0, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc15', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb13', 'Оформление протокола', 'Подготовка текста протокола заседания', 0, 'FIXED_DAYS', 2),
    ('cccccccc-cccc-cccc-cccc-cccccccccc16', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb13', 'Подписание протокола', 'Подписание протокола председателем и секретарём', 2, 'FIXED_DAYS', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccc17', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb14', 'Фиксация результатов', 'Фиксация итогов заседания СД в системе, хранение и исполнение решений', 0, 'FIXED_DAYS', 1)
ON CONFLICT DO NOTHING;

-- Шаблон «Подготовка к ООСУ» (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50)
-- Каждый офер = одна задача (как в ГОСА/ВОСА). Предикаты управляют условным включением.
-- Сценарий А (с аудитом) — критический путь 53 р.д. Сценарий Б (без аудита) — 23 р.д.
INSERT INTO tpl_org_intents (id, code, name, description, sort_order, is_for_ao, is_for_llc, requires_board_of_directors) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'OOSU', 'Подготовка к ООСУ', 'Подготовка и проведение очередного общего собрания участников ООО (ст. 34 14-ФЗ)', 5, null, true, null)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_stages (id, intent_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb41', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Запуск', 'Постановка задачи бухгалтерии о подготовке годовой отчётности', 1, 0, 'FIXED_DAYS', 1),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb42', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Подготовка отчётности', 'Подготовка годового отчёта и баланса, формирование повестки ООСУ', 2, 1, 'FIXED_DAYS', 18),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Проверка отчётности', 'Ревизия и аудит отчётности, приём заключений', 3, 19, 'FIXED_DAYS', 40),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Подготовка к собранию', 'Сборка материалов, уведомление участников и нотариуса, доп. вопросы', 4, 59, 'FIXED_DAYS', 30),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb45', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Проведение ООСУ', 'Проведение собрания, оформление протокола', 5, 89, 'FIXED_DAYS', 2),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb46', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Регистрация изменений', 'Подача Р13014 в ФНС, уведомление банка о смене директора', 6, 91, 'FIXED_DAYS', 14),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb47', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa50', 'Завершение', 'Фиксация результатов, хранение, исполнение решений', 7, 105, 'FIXED_DAYS', 3)
ON CONFLICT DO NOTHING;

-- Оферы ООСУ: каждый офер — шаблон одной задачи, с predecessor_offer_ids и предикатами
-- Префиксы: Б=бухгалтерия, П=подготовка, Р=ревизор, А=аудит, Н=нотариус, Д=допвопросы, ОС=собрание
INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days,
    predecessor_offer_ids, require_notary_confirmation, require_executive_body_a,
    require_mandatory_audit, require_revision_commission) VALUES
    -- Фаза 1: Запуск
    ('cccccccc-cccc-cccc-cccc-cccccccccc91', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb41',
     'Постановка задачи бухгалтерии (Б1)',
     'Приказ/поручение о подготовке годовой отчётности',
     0, 'FIXED_DAYS', 1, NULL, NULL, NULL, NULL, NULL),
    -- Фаза 2: Подготовка отчётности
    ('cccccccc-cccc-cccc-cccc-cccccccccc92', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb42',
     'Подготовка годового отчёта и баланса (Б2)',
     'Формирование годового отчёта и бухгалтерского баланса',
     0, 'FIXED_DAYS', 15, '["cccccccc-cccc-cccc-cccc-cccccccccc91"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc93', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb42',
     'Формирование повестки ООСУ (П1)',
     'Формирование перечня вопросов с учётом результатов года',
     15, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc92"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc94', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb42',
     'Поиск аудитора на следующий год (П0)',
     'Запрос КП у 2–3 аудиторских организаций, проверка СРО, подготовка обоснования для ОСУ. Для гос. участия ≥25% — конкурс по 44-ФЗ/223-ФЗ',
     0, 'FIXED_DAYS', 10, NULL, NULL, NULL, NULL, NULL),
    -- Фаза 3: Проверка отчётности
    ('cccccccc-cccc-cccc-cccc-cccccccccc95', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Передача документов ревизору (Р1)',
     'Передача годового отчёта и баланса ревизионной комиссии (ревизору). ⚠️ Только при >15 участников',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc92"]', NULL, NULL, NULL, TRUE),
    ('cccccccc-cccc-cccc-cccc-cccccccccc96', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Проверка отчётности ревизором (Р2)',
     'Проверка отчётности ревизионной комиссией (ревизором). ⚠️ Только при >15 участников',
     1, 'FIXED_DAYS', 10, '["cccccccc-cccc-cccc-cccc-cccccccccc95"]', NULL, NULL, NULL, TRUE),
    ('cccccccc-cccc-cccc-cccc-cccccccccc97', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Приём заключения ревизора (Р3)',
     'Проверка полноты заключения, подписей, отсутствия оговорок. ⚠️ Только при >15 участников',
     11, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc96"]', NULL, NULL, NULL, TRUE),
    ('cccccccc-cccc-cccc-cccc-cccccccccc98', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Передача документов аудитору (А2)',
     'Передача отчётности аудитору для проверки. ⚠️ Только при обязательном аудите (выручка >800 млн или активы >400 млн)',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc92"]', NULL, NULL, TRUE, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc99', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Проведение аудита (А3)',
     'Проверка отчётности аудитором. ⚠️ Критический путь (35 р.д.). Только при обязательном аудите',
     1, 'FIXED_DAYS', 35, '["cccccccc-cccc-cccc-cccc-cccccccccc98"]', NULL, NULL, TRUE, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc9A', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb43',
     'Приём аудиторского заключения (А4)',
     'Проверка формы (приказ Минфина №46н), подписей, даты. ⚠️ Только при обязательном аудите',
     36, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc99"]', NULL, NULL, TRUE, NULL),
    -- Фаза 4: Подготовка к собранию
    ('cccccccc-cccc-cccc-cccc-cccccccccc9B', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Сборка пакета материалов (П2)',
     'Формирование полного пакета документов к ООСУ. Зависит от заключений ревизора и аудитора',
     0, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc97","cccccccc-cccc-cccc-cccc-cccccccccc9A"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc9C', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Запрос выписки из ЕИС у нотариуса (Н1)',
     'Запрос выписки из реестра списков участников ЕИС у нотариуса. ⚠️ Только при нотариальном удостоверении',
     0, 'FIXED_DAYS', 1, NULL, TRUE, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc9D', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Рассылка уведомлений и материалов участникам (П3)',
     'Уведомление о созыве ООСУ + повестка + пакет материалов за 30 кал. дней',
     2, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc9B"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc9E', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Уведомление нотариуса о собрании (П4)',
     'Уведомление нотариуса о дате/времени/месте собрания. ⚠️ Только при нотариальном удостоверении',
     4, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc93"]', TRUE, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc9F', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Приём доп. вопросов от участников — дедлайн (Д1)',
     'Дедлайн приёма дополнительных вопросов от участников (−15 кал. дней до ООСУ)',
     5, 'FIXED_DAYS', 0, NULL, NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA0', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Рассмотрение доп. вопросов (Д2)',
     'Рассмотрение и включение дополнительных вопросов в повестку (5 дней)',
     5, 'FIXED_DAYS', 5, '["cccccccc-cccc-cccc-cccc-cccccccccc9F"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA1', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44',
     'Уведомление об изменении повестки (Д3)',
     'Рассылка уведомлений об изменении повестки (−10 кал. дней до ООСУ)',
     10, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA0"]', NULL, NULL, NULL, NULL),
    -- Фаза 5: Проведение ООСУ и завершение
    ('cccccccc-cccc-cccc-cccc-ccccccccccA2', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb45',
     'Проведение ООСУ (ОС1)',
     'Проведение собрания: регистрация, кворум, обсуждение, голосование, принятие решений',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc9D","cccccccc-cccc-cccc-cccc-cccccccccc9E","cccccccc-cccc-cccc-cccc-ccccccccccA1"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA3', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb45',
     'Оформление протокола (ОС2)',
     'Оформление протокола ООСУ, подписание, нотариальное удостоверение (если применимо)',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA2"]', NULL, NULL, NULL, NULL),
    -- Регистрация изменений: только при смене директора или изменении устава
    ('cccccccc-cccc-cccc-cccc-ccccccccccA4', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb46',
     'Подача Р13014 в ФНС (ОС3)',
     'Заявление по форме Р13014 в течение 7 р.д. после ООСУ. ⚠️ Только при смене директора или изменении устава',
     0, 'FIXED_DAYS', 7, '["cccccccc-cccc-cccc-cccc-ccccccccccA3"]', NULL, TRUE, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA5', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb46',
     'Уведомление банка о смене директора (ОС4)',
     'Уведомление банка об изменении лица, имеющего право подписи. ⚠️ Только при смене директора',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA2"]', NULL, TRUE, NULL, NULL),
    -- Завершение: фиксация результатов
    ('cccccccc-cccc-cccc-cccc-ccccccccccA6', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb47',
     'Фиксация результатов ООСУ в системе',
     'Внесение итогов голосования и решений ООСУ в систему',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA3"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA7', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb47',
     'Организация хранения документов ООСУ',
     'Формирование и передача полного комплекта документов ООСУ на постоянное хранение',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA6"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-ccccccccccA8', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb47',
     'Организация исполнения решений ООСУ',
     'Контроль исполнения решений, принятых на ООСУ',
     2, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-ccccccccccA7"]', NULL, NULL, NULL, NULL)
ON CONFLICT DO NOTHING;

-- Шаблон «Подготовка к ВОСУ» (aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60)
-- Каждый офер = одна задача (как в ГОСА/ВОСА). Предикаты управляют условным включением.
INSERT INTO tpl_org_intents (id, code, name, description, sort_order, is_for_ao, is_for_llc, requires_board_of_directors) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'VOSU', 'Подготовка к ВОСУ', 'Подготовка и проведение внеочередного общего собрания участников ООО (ст. 35 14-ФЗ)', 6, null, true, null)
ON CONFLICT DO NOTHING;

INSERT INTO tpl_org_stages (id, intent_id, name, description, sort_order, start_offset_days, deadline_rule, deadline_days) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb51', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Получение требования', 'Приём и проверка требования о созыве ВОСУ, принятие решения', 1, 0, 'FIXED_DAYS', 5),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Подготовка', 'Определение даты, формирование повестки, подготовка материалов', 2, 5, 'FIXED_DAYS', 10),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Уведомление участников', 'Рассылка уведомлений, приём доп. вопросов, изменение повестки', 3, 15, 'FIXED_DAYS', 20),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb54', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Проведение ВОСУ', 'Регистрация, обсуждение, голосование, подсчёт', 4, 35, 'FIXED_DAYS', 2),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Оформление результатов', 'Составление протокола, нотариальное удостоверение, регистрация, рассылка', 5, 37, 'FIXED_DAYS', 10),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb56', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa60', 'Завершение', 'Фиксация результатов, хранение, исполнение решений', 6, 47, 'FIXED_DAYS', 3)
ON CONFLICT DO NOTHING;

-- Оферы ВОСУ: каждый офер — шаблон одной задачи, с predecessor_offer_ids и предикатами
INSERT INTO tpl_org_offers (id, stage_id, name, description, start_offset_days, deadline_rule, deadline_days,
    predecessor_offer_ids, require_notary_confirmation, require_executive_body_a,
    require_mandatory_audit, require_revision_commission) VALUES
    -- Фаза 1: Получение требования
    ('cccccccc-cccc-cccc-cccc-cccccccccc71', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb51',
     'Приём и регистрация требования',
     'Регистрация входящего требования о созыве ВОСУ с датой получения',
     0, 'FIXED_DAYS', 1, NULL, NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc72', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb51',
     'Проверка доли инициатора (≥10%)',
     'Сверка с реестром участников, подтверждение совокупной доли',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc71"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc73', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb51',
     'Проверка повестки на соответствие закону и уставу',
     'Проверка вопросов на компетенцию ОСУ и соответствие 14-ФЗ',
     2, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc72"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc74', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb51',
     'Принятие решения о созыве или мотивированный отказ',
     'Оформление решения гендиректора (5-дневный срок). В случае отказа — завершение процесса',
     3, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc73"]', NULL, NULL, NULL, NULL),
    -- Фаза 2: Подготовка
    ('cccccccc-cccc-cccc-cccc-cccccccccc75', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52',
     'Определение даты и места ВОСУ',
     'Выбор даты в пределах 45 дней (75 — с избранием СД) с учётом 30-дневного уведомления',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc74"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc76', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52',
     'Формирование повестки дня',
     'Составление перечня вопросов ровно по требованию инициатора',
     1, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc75"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc77', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52',
     'Подготовка проектов решений',
     'Подготовка проектов решений по каждому вопросу повестки',
     1, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc76"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc78', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52',
     'Подготовка информационных материалов',
     'Подготовка материалов по вопросам повестки (при необходимости)',
     3, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc77"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc79', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb52',
     'Сборка пакета для рассылки',
     'Формирование полного комплекта документов для отправки участникам',
     5, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc78"]', NULL, NULL, NULL, NULL),
    -- Фаза 3: Уведомление участников
    ('cccccccc-cccc-cccc-cccc-cccccccccc7A', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53',
     'Подготовка уведомления о созыве',
     'Составление текста уведомления о созыве ВОСУ с повесткой и материалами',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc79"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc7B', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53',
     'Отправка уведомлений участникам',
     'Рассылка заказными письмами или способом по уставу за 30 дней до ВОСУ',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc7A"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc7C', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53',
     'Приём доп. вопросов от участников — дедлайн',
     'Дедлайн приёма дополнительных вопросов от участников (−15 кал. дней до ВОСУ)',
     2, 'FIXED_DAYS', 0, NULL, NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc7D', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53',
     'Рассмотрение и включение доп. вопросов в повестку',
     'Рассмотрение доп. вопросов, включение в повестку (5-дневный срок)',
     2, 'FIXED_DAYS', 5, '["cccccccc-cccc-cccc-cccc-cccccccccc7C"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc7E', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb53',
     'Уведомление об изменении повестки',
     'Рассылка уведомлений об изменении повестки (−10 кал. дней до ВОСУ)',
     7, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc7D"]', NULL, NULL, NULL, NULL),
    -- Фаза 4: Проведение ВОСУ
    ('cccccccc-cccc-cccc-cccc-cccccccccc7F', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb54',
     'Регистрация участников и проверка полномочий',
     'Регистрация участников, проверка доверенностей представителей',
     0, 'FIXED_DAYS', 1, NULL, NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc80', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb54',
     'Проведение собрания: обсуждение и голосование',
     'Обсуждение вопросов повестки, голосование, подсчёт, оглашение результатов',
     0, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc7B","cccccccc-cccc-cccc-cccc-cccccccccc7E","cccccccc-cccc-cccc-cccc-cccccccccc7F"]', NULL, NULL, NULL, NULL),
    -- Фаза 5: Оформление результатов
    ('cccccccc-cccc-cccc-cccc-cccccccccc81', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55',
     'Составление протокола ВОСУ',
     'Подготовка и подписание протокола председателем и секретарём',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc80"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc82', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55',
     'Нотариальное удостоверение протокола',
     'Заверение протокола у нотариуса. ⚠️ Только при нотариальном удостоверении (если не заменено уставом на подписание всеми)',
     1, 'FIXED_DAYS', 2, '["cccccccc-cccc-cccc-cccc-cccccccccc81"]', TRUE, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc83', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55',
     'Подача Р13014 в ФНС',
     'Заявление по форме Р13014 в течение 7 р.д. ⚠️ Только при смене директора или изменении устава',
     1, 'FIXED_DAYS', 7, '["cccccccc-cccc-cccc-cccc-cccccccccc81"]', NULL, TRUE, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc84', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55',
     'Рассылка копий протокола участникам',
     'Отправка копий протокола всем участникам в разумный срок',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc81"]', NULL, NULL, NULL, NULL),
    -- Фаза 6: Завершение
    ('cccccccc-cccc-cccc-cccc-cccccccccc85', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb56',
     'Фиксация результатов ВОСУ в системе',
     'Внесение итогов голосования и решений ВОСУ в систему',
     0, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc81"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc86', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb56',
     'Организация хранения документов ВОСУ',
     'Формирование и передача полного комплекта документов ВОСУ на постоянное хранение',
     1, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc85"]', NULL, NULL, NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc87', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb56',
     'Организация исполнения решений ВОСУ',
     'Контроль исполнения решений, принятых на ВОСУ',
     2, 'FIXED_DAYS', 1, '["cccccccc-cccc-cccc-cccc-cccccccccc86"]', NULL, NULL, NULL, NULL)
ON CONFLICT DO NOTHING;

-- Типовые уставы ООО (Приказ Минэкономразвития № 411 от 01.08.2018)
-- Номера 01–09 с ведущим нулём в соответствии с форматом ФНС (Р11001, Р13014)
INSERT INTO ref_standard_charter (id, number, exit_allowed, transfer_to_participants_without_consent, transfer_to_third_parties_without_consent, preemptive_right, inheritance_without_consent, executive_body, decision_confirmation_by_all_sign) VALUES
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01', '01', FALSE, TRUE,  FALSE, TRUE, TRUE, 'A', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02', '02', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'A', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03', '03', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'A', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee04', '04', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'A', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee05', '05', FALSE, FALSE, FALSE, TRUE, TRUE, 'A', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee06', '06', FALSE, FALSE, FALSE, TRUE, TRUE, 'A', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee07', '07', FALSE, TRUE,  FALSE, TRUE, TRUE, 'B', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee08', '08', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'B', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee09', '09', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'B', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee10', '10', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'B', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee11', '11', FALSE, FALSE, FALSE, TRUE, TRUE, 'B', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee12', '12', FALSE, FALSE, FALSE, TRUE, TRUE, 'B', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee13', '13', FALSE, TRUE,  FALSE, TRUE, TRUE, 'C', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee14', '14', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'C', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee15', '15', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'C', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee16', '16', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'C', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee17', '17', FALSE, FALSE, FALSE, TRUE, TRUE, 'C', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee18', '18', FALSE, FALSE, FALSE, TRUE, TRUE, 'C', FALSE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee19', '19', FALSE, TRUE,  FALSE, TRUE, TRUE, 'A', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee20', '20', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'A', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee21', '21', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'A', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee22', '22', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'A', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee23', '23', FALSE, FALSE, FALSE, TRUE, TRUE, 'A', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee24', '24', FALSE, FALSE, FALSE, TRUE, TRUE, 'A', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee25', '25', FALSE, TRUE,  FALSE, TRUE, TRUE, 'B', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee26', '26', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'B', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee27', '27', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'B', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee28', '28', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'B', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee29', '29', FALSE, FALSE, FALSE, TRUE, TRUE, 'B', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee30', '30', FALSE, FALSE, FALSE, TRUE, TRUE, 'B', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee31', '31', FALSE, TRUE,  FALSE, TRUE, TRUE, 'C', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee32', '32', TRUE,  TRUE,  FALSE, TRUE, TRUE, 'C', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee33', '33', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'C', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee34', '34', FALSE, TRUE,  TRUE,  TRUE, TRUE, 'C', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee35', '35', FALSE, FALSE, FALSE, TRUE, TRUE, 'C', TRUE),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee36', '36', FALSE, FALSE, FALSE, TRUE, TRUE, 'C', TRUE)
ON CONFLICT (number) DO NOTHING;


-- ============================================================================
-- Миграция: повышение задач (tpl_org_tasks) до оферов (tpl_org_offers)
-- Каждая задача становится отдельным офером — шаблоном одной будущей задачи.
-- Старые оферы, имевшие дочерние задачи, удаляются.
-- ============================================================================

-- Шаг 1: создать новые оферы из задач
INSERT INTO tpl_org_offers (id, stage_id, name, description,
    assigned_role_id, assigned_board_role_id,
    require_notary_confirmation, require_all_sign_confirmation,
    require_committees, require_board_regulation,
    require_custom_charter, require_executive_body_a,
    require_board_of_directors, require_document_flow_legal_electronic)
SELECT t.id, o.stage_id, t.name, t.description,
    t.assigned_role_id, t.assigned_board_role_id,
    t.require_notary_confirmation, t.require_all_sign_confirmation,
    t.require_committees, t.require_board_regulation,
    t.require_custom_charter, t.require_executive_body_a,
    t.require_board_of_directors, t.require_document_flow_legal_electronic
FROM tpl_org_tasks t
JOIN tpl_org_offers o ON o.id = t.offer_id
ON CONFLICT (id) DO NOTHING;

-- Шаг 2: удалить старые оферы, которые были родителями задач
DELETE FROM tpl_org_offers WHERE id IN (SELECT DISTINCT offer_id FROM tpl_org_tasks);

-- Шаг 3: удалить таблицу задач (больше не нужна)
DROP TABLE IF EXISTS tpl_org_tasks CASCADE;
