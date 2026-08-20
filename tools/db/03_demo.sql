-- ============================================================================
-- Тестовые физические лица (persons)
-- ============================================================================
INSERT INTO persons (id, last_name, first_name, middle_name, email, phone, inn, created_at, created_by) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01', 'Иванов', 'Иван', 'Иванович', 'ivanov@fiducia.local', '+79001112233', '770123456789', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', 'Петрова', 'Мария', 'Сергеевна', 'petrova@fiducia.local', '+79002223344', '770234567890', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03', 'Сидоров', 'Алексей', 'Петрович', 'sidorov@fiducia.local', '+79003334455', '770345678901', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa04', 'Козлова', 'Елена', 'Дмитриевна', 'kozlova@fiducia.local', '+79004445566', '770456789012', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa05', 'Новиков', 'Дмитрий', 'Александрович', 'novikov@fiducia.local', '+79005556677', '770567890123', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa06', 'Волков', 'Сергей', 'Андреевич', 'ceo@fiducia.local', '+79006667788', '770678901234', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- Тестовые пользователи (привязаны к ФЛ)
-- ============================================================================
INSERT INTO users (id, person_id, last_name, first_name, email, phone, is_external, created_at, created_by, is_system) VALUES
    ('11111111-1111-1111-1111-111111111112', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01', 'Иванов', 'Иван', 'ivanov@fiducia.local', '+79001112233', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE),
    ('11111111-1111-1111-1111-111111111113', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', 'Петрова', 'Мария', 'petrova@fiducia.local', '+79002223344', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE),
    ('11111111-1111-1111-1111-111111111114', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03', 'Сидоров', 'Алексей', 'sidorov@fiducia.local', '+79003334455', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE),
    ('11111111-1111-1111-1111-111111111115', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa04', 'Козлова', 'Елена', 'kozlova@fiducia.local', '+79004445566', TRUE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE),
    ('11111111-1111-1111-1111-111111111116', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa05', 'Новиков', 'Дмитрий', 'novikov@fiducia.local', '+79005556677', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE),
    ('11111111-1111-1111-1111-111111111117', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa06', 'Волков', 'Сергей', 'ceo@fiducia.local', '+79006667788', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE)
ON CONFLICT (id) DO NOTHING;

-- Роли тестовых пользователей
INSERT INTO user_roles (id, user_id, role_id) VALUES
    ('aaaa0000-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111112', '11111111-1111-1111-1111-111111111111'), -- Иванов = SYS_ADMIN
    ('aaaa0000-0000-0000-0000-000000000002', '11111111-1111-1111-1111-111111111113', '22222222-2222-2222-2222-222222222222'), -- Петрова = SECRETARY
    ('aaaa0000-0000-0000-0000-000000000003', '11111111-1111-1111-1111-111111111114', '55555555-5555-5555-5555-555555555555'), -- Сидоров = EXTERNAL_DIRECTOR
    ('aaaa0000-0000-0000-0000-000000000004', '11111111-1111-1111-1111-111111111115', '55555555-5555-5555-5555-555555555555'), -- Козлова = EXTERNAL_DIRECTOR
    ('aaaa0000-0000-0000-0000-000000000005', '11111111-1111-1111-1111-111111111116', '33333333-3333-3333-3333-333333333333'), -- Новиков = CHAIR_BOARD
    ('aaaa0000-0000-0000-0000-000000000006', '11111111-1111-1111-1111-111111111117', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaad4')  -- Волков = CEO (Генеральный директор)
ON CONFLICT (user_id, role_id) DO NOTHING;

-- ============================================================================
-- Тестовый участник ООО
-- ============================================================================
INSERT INTO persons (id, last_name, first_name, middle_name, email, phone, inn, created_at, created_by) VALUES
    ('bbbb0000-0000-0000-0000-000000000001', 'Соколова', 'Анна', 'Викторовна', 'participant@test.ru', '+79009998877', '770999887766', CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000')
ON CONFLICT (id) DO NOTHING;

INSERT INTO users (id, person_id, last_name, first_name, email, phone, is_external, created_at, created_by, is_system) VALUES
    ('bbbb0000-0000-0000-0000-000000000002', 'bbbb0000-0000-0000-0000-000000000001', 'Соколова', 'Анна', 'participant@test.ru', '+79009998877', FALSE, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', FALSE)
ON CONFLICT (id) DO NOTHING;

INSERT INTO user_roles (id, user_id, role_id) VALUES
    ('bbbb0000-0000-0000-0000-000000000010', 'bbbb0000-0000-0000-0000-000000000002', '99999999-9999-9999-9999-999999999999')
ON CONFLICT (user_id, role_id) DO NOTHING;

-- ============================================================================
-- Демонстрационные данные СПАРК для ООО (ext_spark_company, ext_spark_manager,
-- ext_spark_founder)
-- ============================================================================

BEGIN;

-- ----------------------------------------------------------------------------
-- 1. ООО «Яндекс» (ИНН 7736207543) — 1 ЮЛ + 1 ФЛ
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name, legal_address, status, registration_date, employees_count, fetched_at)
VALUES (gen_random_uuid(), '7736207543', '1027700229193', 'ООО «Яндекс»', 'ООО «Яндекс»',
    '12300', 'ООО', '119021, г. Москва, ул. Льва Толстого, д. 16',
    'Действующее', '2000-01-19', 25000, NOW());

INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7736207543', 'Кудрин Максим Львович', 'Генеральный директор', '780401234567', '2023-05-15', NOW());

INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship,
    head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES
    (gen_random_uuid(), '7736207543',
     'АО «Яндекс.Технологии»', '9705012345', '1207700420500',
     'Россия', FALSE,
     NULL, NULL, NULL,
     NULL, NULL, FALSE, NULL,
     900000.00, 90.00, '2019-12-18', NULL, NOW()),
    (gen_random_uuid(), '7736207543',
     NULL, NULL, NULL, NULL, FALSE,
     'Волож Аркадий Юрьевич', '772401234567', 'Мальта',
     1, 5, FALSE, NULL,
     100000.00, 10.00, '2000-01-19', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 2. ООО «Вайлдберриз» (ИНН 7721546864) — 1 ЮЛ + 2 ФЛ + 1 выбывший ФЛ
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name, legal_address, status, registration_date, employees_count, fetched_at)
VALUES (gen_random_uuid(), '7721546864', '1067746062449', 'ООО «Вайлдберриз»', 'ООО «Вайлдберриз»',
    '12300', 'ООО', '142181, Московская обл., г. Подольск, д. Коледино',
    'Действующее', '2004-04-20', 48000, NOW());

INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7721546864', 'Бакальчук Татьяна Владимировна', 'Генеральный директор', '501201234567', '2004-01-16', NOW());

INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship,
    head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES
    (gen_random_uuid(), '7721546864',
     'ООО «ВБ Холдинг»', '7721999001', '1247700012345',
     'Россия', FALSE,
     NULL, NULL, NULL,
     NULL, NULL, FALSE, NULL,
     750000.00, 75.00, '2020-03-15', NULL, NOW()),
    (gen_random_uuid(), '7721546864',
     NULL, NULL, NULL, NULL, FALSE,
     'Бакальчук Татьяна Владимировна', '501201234567', 'Россия',
     1, 1, TRUE, '304770000300015',
     200000.00, 20.00, '2004-04-20', NULL, NOW()),
    (gen_random_uuid(), '7721546864',
     NULL, NULL, NULL, NULL, FALSE,
     'Бакальчук Владислав Сергеевич', '501208765432', 'Россия',
     0, 1, FALSE, NULL,
     10000.00, 1.00, '2004-04-20', NULL, NOW()),
    (gen_random_uuid(), '7721546864',
     NULL, NULL, NULL, NULL, FALSE,
     'Ревякин Сергей Владимирович', '501209876543', 'Россия',
     4, 7, FALSE, NULL,
     150000.00, 15.00, '2004-04-20', '2019-06-30', NOW());

-- ----------------------------------------------------------------------------
-- 3. ООО «Озон» (ИНН 7703382710) — 1 ЮЛ + 2 ФЛ + 1 выбывший ФЛ
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name, legal_address, status, registration_date, employees_count, fetched_at)
VALUES (gen_random_uuid(), '7703382710', '1027739013283', 'ООО «Интернет Решения»', 'ООО «Интернет Решения»',
    '12300', 'ООО', '123112, г. Москва, Пресненская наб., д. 10',
    'Действующее', '1998-05-06', 45000, NOW());

INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7703382710', 'Шульгин Александр Сергеевич', 'Генеральный директор', '772912345678', '2017-12-01', NOW());

INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship,
    head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES
    (gen_random_uuid(), '7703382710',
     'ООО «Озон Холдинг»', '7704356731', '5147746215061',
     'Россия', FALSE,
     NULL, NULL, NULL,
     NULL, NULL, FALSE, NULL,
     999900.00, 99.99, '2014-05-20', NULL, NOW()),
    (gen_random_uuid(), '7703382710',
     NULL, NULL, NULL, NULL, FALSE,
     'Минаев Алексей Владимирович', '771501234567', 'Россия',
     2, 1, FALSE, NULL,
     50.00, 0.005, '2013-02-10', NULL, NOW()),
    (gen_random_uuid(), '7703382710',
     NULL, NULL, NULL, NULL, FALSE,
     'Петрова Елена Игоревна', '773212345678', 'Россия',
     0, 0, FALSE, NULL,
     50.00, 0.005, '2013-02-10', NULL, NOW()),
    (gen_random_uuid(), '7703382710',
     NULL, NULL, NULL, NULL, FALSE,
     'Агафонов Дмитрий Павлович', '771801234567', 'Россия',
     4, 3, TRUE, '304770000200016',
     50000.00, 5.00, '2004-09-01', '2014-05-19', NOW());

COMMIT;

-- ============================================================================
-- Демонстрационные данные СПАРК для регистраторов (ext_spark_company,
-- ext_spark_manager, ext_spark_founder)
-- ============================================================================

BEGIN;

-- ----------------------------------------------------------------------------
-- 4. АО «НРК - Р.О.С.Т.» (ИНН 7726030449) — регистратор
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7726030449', '1027739216757',
    'Акционерное общество «Независимая регистраторская компания Р.О.С.Т.»',
    'АО «НРК - Р.О.С.Т.»', '12247', 'ПАО',
    '107076, г. Москва, ул. Стромынка, д. 18, к. 5Б',
    'Действующее', '2002-10-10', NOW());

INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7726030449', 'Иванов Сергей Петрович', 'Генеральный директор', '772501234567', '2015-03-20', NOW());

INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship,
    head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7726030449',
    'АО «НРК - Р.О.С.Т.»', NULL, NULL,
    'Россия', FALSE,
    NULL, NULL, NULL,
    NULL, NULL, FALSE, NULL,
    NULL, 100.00, '2002-10-10', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 5. АО «Реестр» (ИНН 7704028206) — регистратор
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7704028206', '1027700047275',
    'Акционерное общество «Реестр»',
    'АО «Реестр»', '12247', 'ПАО',
    '129090, г. Москва, пер. Большой Балканский, д. 20, стр. 1',
    'Действующее', '2002-06-18', NOW());

INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7704028206', 'Петров Алексей Николаевич', 'Генеральный директор', '770401234567', '2010-07-15', NOW());

INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship,
    head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7704028206',
    'АО «Реестр»', NULL, NULL,
    'Россия', FALSE,
    NULL, NULL, NULL,
    NULL, NULL, FALSE, NULL,
    NULL, 100.00, '2002-06-18', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 6. ООО «Реестр-РН» (ИНН 7705397301) — регистратор
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7705397301', '1027700172818',
    'Общество с ограниченной ответственностью «Реестр-РН»',
    'ООО «Реестр-РН»', '12300', 'ООО',
    '115093, г. Москва, пер. 1-й Щипковский, д. 20',
    'Действующее', '2002-07-11', NOW());

INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7705397301', 'Сидоров Владимир Алексеевич', 'Генеральный директор', '770501234567', '2012-04-10', NOW());

-- ----------------------------------------------------------------------------
-- 7. АО «ВРК» (ИНН 6661049239)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '6661049239', NULL,
    'Акционерное общество "Ведение реестров компаний"',
    'АО "ВРК"', '12247', 'ПАО',
    '620014, г. Екатеринбург', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '6661049239', 'Козлов Андрей Викторович', 'Генеральный директор', '666101234567', '2010-01-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '6661049239', 'АО "ВРК"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 8. АО "Индустрия-РЕЕСТР" (ИНН 3302021034)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '3302021034', NULL,
    'Акционерное общество "Индустрия-РЕЕСТР"',
    'АО "Индустрия-РЕЕСТР"', '12247', 'ПАО',
    '107113, г. Москва', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '3302021034', 'Михайлов Дмитрий Сергеевич', 'Генеральный директор', '330201234567', '2012-06-15', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '3302021034', 'АО "Индустрия-РЕЕСТР"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 9. АО "Новый регистратор" (ИНН 7719263354)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7719263354', NULL,
    'Акционерное общество "Новый регистратор"',
    'АО "Новый регистратор"', '12247', 'ПАО',
    '107996, г. Москва, ул. Буженинова, д. 30', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7719263354', 'Новиков Павел Александрович', 'Генеральный директор', '771901234567', '2014-09-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7719263354', 'АО "Новый регистратор"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 10. АО "ПРЦ" (ИНН 3821010220)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '3821010220', NULL,
    'Акционерное общество "Профессиональный регистрационный центр"',
    'АО "ПРЦ"', '12247', 'ПАО',
    '117452, г. Москва, пр-кт Балаклавский', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '3821010220', 'Кузнецов Игорь Михайлович', 'Генеральный директор', '382101234567', '2011-03-10', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '3821010220', 'АО "ПРЦ"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 11. АО "РДЦ ПАРИТЕТ" (ИНН 7723103642)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7723103642', NULL,
    'Акционерное общество "РДЦ ПАРИТЕТ"',
    'АО "РДЦ ПАРИТЕТ"', '12247', 'ПАО',
    '115114, г. Москва', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7723103642', 'Соколов Виктор Петрович', 'Генеральный директор', '772301234567', '2013-07-20', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7723103642', 'АО "РДЦ ПАРИТЕТ"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 12. АО "РЕГИСТРАТОР ИНТРАКО" (ИНН 5903027161)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '5903027161', NULL,
    'Акционерное общество "Регистратор Интрако"',
    'АО "РЕГИСТРАТОР ИНТРАКО"', '12247', 'ПАО',
    '614000, г. Пермь, ул. Ленина', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '5903027161', 'Волков Сергей Николаевич', 'Генеральный директор', '590301234567', '2015-01-15', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '5903027161', 'АО "РЕГИСТРАТОР ИНТРАКО"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 13. АО "Регистратор-Капитал" (ИНН 6659035711)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '6659035711', NULL,
    'Акционерное общество "Регистратор-Капитал"',
    'АО "Регистратор-Капитал"', '12247', 'ПАО',
    '620041, г. Екатеринбург', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '6659035711', 'Лебедев Олег Владимирович', 'Генеральный директор', '665901234567', '2016-05-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '6659035711', 'АО "Регистратор-Капитал"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 14. АО "СТАТУС" (ИНН 7707179242)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7707179242', NULL,
    'Акционерное общество "Регистраторское общество "СТАТУС"',
    'АО "СТАТУС"', '12247', 'ПАО',
    '109052, г. Москва, ул. Новоховловская', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7707179242', 'Фёдоров Артём Леонидович', 'Генеральный директор', '770701234567', '2014-11-10', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7707179242', 'АО "СТАТУС"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 15. АО РК "Центр-Инвест" (ИНН 7726050935)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7726050935', NULL,
    'Акционерное общество "Регистрационная Компания Центр-Инвест"',
    'АО РК "Центр-Инвест"', '12247', 'ПАО',
    '107023, г. Москва, пер. Мажоров', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7726050935', 'Белов Александр Юрьевич', 'Генеральный директор', '772601234567', '2013-02-20', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7726050935', 'АО РК "Центр-Инвест"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 16. АО РСР "ЯФЦ" (ИНН 1435001668)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '1435001668', NULL,
    'Акционерное общество "Республиканский специализированный регистратор "Якутский Фондовый Центр"',
    'АО РСР "ЯФЦ"', '12247', 'ПАО',
    '677018, г. Якутск', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '1435001668', 'Попов Николай Алексеевич', 'Генеральный директор', '143501234567', '2011-08-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '1435001668', 'АО РСР "ЯФЦ"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 17. АО "РТ-Регистратор" (ИНН 5407175878)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '5407175878', NULL,
    'Акционерное общество "РТ-Регистратор"',
    'АО "РТ-Регистратор"', '12247', 'ПАО',
    '119049, г. Москва, ул. Донская', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '5407175878', 'Морозов Сергей Иванович', 'Генеральный директор', '540701234567', '2017-04-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '5407175878', 'АО "РТ-Регистратор"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 18. АО "Сервис-Реестр" (ИНН 8605006147)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '8605006147', NULL,
    'Акционерное общество "Сервис-Реестр"',
    'АО "Сервис-Реестр"', '12247', 'ПАО',
    '107045, г. Москва, ул. Сретенка', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '8605006147', 'Козлов Денис Андреевич', 'Генеральный директор', '860501234567', '2018-06-15', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '8605006147', 'АО "Сервис-Реестр"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 19. АО "СРК «КОМПАС»" (ИНН 4217027573)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '4217027573', NULL,
    'акционерное общество "Специализированный Регистратор "КОМПАС"',
    'АО "СРК"', '12247', 'ПАО',
    '654005, г. Новокузнецк', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '4217027573', 'Тарасов Олег Николаевич', 'Генеральный директор', '421701234567', '2015-09-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '4217027573', 'АО "СРК «КОМПАС»"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 20. АО «Агентство «РНР»» (ИНН 7107039003)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7107039003', NULL,
    'Акционерное общество «Агентство «Региональный независимый регистратор»',
    'АО «Агентство «РНР»»', '12247', 'ПАО',
    '398017, г. Липецк', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7107039003', 'Захаров Игорь Сергеевич', 'Генеральный директор', '710701234567', '2014-03-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7107039003', 'АО «Агентство «РНР»»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 21. АО «Вторая линия» (ИНН 9714072529)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '9714072529', NULL,
    'Акционерное общество «Вторая линия»',
    'АО «Вторая линия»', '12247', 'ПАО',
    '125057, г. Москва', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '9714072529', 'Семёнов Алексей Викторович', 'Генеральный директор', '971401234567', '2016-07-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '9714072529', 'АО «Вторая линия»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 22. АО «МРЦ» (ИНН 1901003859)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '1901003859', NULL,
    'АКЦИОНЕРНОЕ ОБЩЕСТВО «МЕЖРЕГИОНАЛЬНЫЙ РЕГИСТРАТОРСКИЙ ЦЕНТР»',
    'АО «МРЦ»', '12247', 'ПАО',
    '101000, г. Москва, пер. Подсосенский', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '1901003859', 'Орлов Максим Андреевич', 'Генеральный директор', '190101234567', '2012-01-15', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '1901003859', 'АО «МРЦ»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 23. АО «Реестр-Протон» (ИНН 9702074105)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '9702074105', NULL,
    'Акционерное общество «Реестр-Протон»',
    'АО «Реестр-Протон»', '12247', 'ПАО',
    '129110, г. Москва', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '9702074105', 'Николаев Дмитрий Олегович', 'Генеральный директор', '970201234567', '2017-02-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '9702074105', 'АО «Реестр-Протон»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 24. АО «СДК «Сириус»» (ИНН 9703197607)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '9703197607', NULL,
    'Акционерное общество «Специализированная депозитарная компания «Сириус»',
    'АО «СДК «Сириус»»', '12247', 'ПАО',
    '123100, г. Москва', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '9703197607', 'Васильев Андрей Николаевич', 'Генеральный директор', '970301234567', '2015-05-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '9703197607', 'АО «СДК «Сириус»»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 25. АО «ДРАГА» (ИНН 7704011964)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7704011964', NULL,
    'Акционерное общество «Специализированный регистратор - Держатель реестров акционеров газовой промышленности»',
    'АО «ДРАГА»', '12247', 'ПАО',
    '190098, г. Санкт-Петербург', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7704011964', 'Громов Сергей Петрович', 'Генеральный директор', '770401234568', '2013-08-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7704011964', 'АО «ДРАГА»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 26. АО «ФРК» (ИНН 9718273177)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '9718273177', NULL,
    'Акционерное общество «Фондовая регистрационная компания»',
    'АО «ФРК»', '12247', 'ПАО',
    '107076, г. Москва', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '9718273177', 'Романов Кирилл Александрович', 'Генеральный директор', '971801234567', '2016-01-15', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '9718273177', 'АО «ФРК»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 27. АО ВТБ Регистратор (ИНН 5610083568)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '5610083568', NULL,
    'Акционерное общество ВТБ Регистратор',
    'АО ВТБ Регистратор', '12247', 'ПАО',
    '127015, г. Москва, ул. Правды', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '5610083568', 'Ковалёв Дмитрий Сергеевич', 'Генеральный директор', '561001234567', '2014-04-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '5610083568', 'АО ВТБ Регистратор', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 28. ООО "ЕАР" (ИНН 1660055801)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '1660055801', NULL,
    'Общество с ограниченной ответственностью "Евроазиатский Регистратор"',
    'ООО "ЕАР"', '12300', 'ООО',
    '420097, г. Казань', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '1660055801', 'Хасанов Ринат Маратович', 'Генеральный директор', '166001234567', '2015-06-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '1660055801', 'ООО "ЕАР"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 29. ООО "Оборонрегистр" (ИНН 7731513346)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7731513346', NULL,
    'Общество с ограниченной ответственностью "Оборонрегистр"',
    'ООО "Оборонрегистр"', '12300', 'ООО',
    '105066, г. Москва, ул. Старая Басманная', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7731513346', 'Шестаков Виктор Алексеевич', 'Генеральный директор', '773101234567', '2013-10-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7731513346', 'ООО "Оборонрегистр"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 30. ООО "ПАРТНЁР" (ИНН 3528218586)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '3528218586', NULL,
    'Общество с ограниченной ответственностью "ПАРТНЁР"',
    'ООО "ПАРТНЁР"', '12300', 'ООО',
    '162606, г. Череповец', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '3528218586', 'Беляев Олег Сергеевич', 'Генеральный директор', '352801234567', '2016-08-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '3528218586', 'ООО "ПАРТНЁР"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 31. ООО "Регистратор "Гарант" (ИНН 7703802628)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7703802628', NULL,
    'Общество с ограниченной ответственностью "Регистратор "Гарант"',
    'ООО "Регистратор "Гарант"', '12300', 'ООО',
    '123100, г. Москва', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7703802628', 'Зайцев Алексей Дмитриевич', 'Генеральный директор', '770301234567', '2014-12-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7703802628', 'ООО "Регистратор "Гарант"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 32. ООО "ЦУР" (ИНН 7842521215)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7842521215', NULL,
    'Общество с ограниченной ответственностью "Центр учета и регистрации"',
    'ООО "ЦУР"', '12300', 'ООО',
    '191124, г. Санкт-Петербург', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7842521215', 'Павлов Николай Сергеевич', 'Генеральный директор', '784201234567', '2015-03-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7842521215', 'ООО "ЦУР"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 33. ООО "ЮРР" (ИНН 6166032022)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '6166032022', NULL,
    'Общество с ограниченной ответственностью "Южно-Региональный регистратор"',
    'ООО "ЮРР"', '12300', 'ООО',
    '344029, г. Ростов-на-Дону', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '6166032022', 'Егоров Дмитрий Викторович', 'Генеральный директор', '616601234567', '2017-01-15', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '6166032022', 'ООО "ЮРР"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 34. ООО «Московский Фондовый Центр» (ИНН 7708822233)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7708822233', NULL,
    'Общество с ограниченной ответственностью «Московский Фондовый Центр»',
    'ООО «МФЦ»', '12300', 'ООО',
    '107078, г. Москва, пер. Орликов', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7708822233', 'Савельев Андрей Олегович', 'Генеральный директор', '770801234567', '2016-05-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7708822233', 'ООО «МФЦ»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 35. ООО «РБРУ СД» (ИНН 9704154155)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '9704154155', NULL,
    'Общество с ограниченной ответственностью «РБРУ Специализированный депозитарий»',
    'ООО «РБРУ СД»', '12300', 'ООО',
    '119002, г. Москва', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '9704154155', 'Кузьмин Сергей Александрович', 'Генеральный директор', '970401234567', '2018-02-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '9704154155', 'ООО «РБРУ СД»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 36. ООО «ТЕМИОН» (ИНН 7730337754)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '7730337754', NULL,
    'ОБЩЕСТВО С ОГРАНИЧЕННОЙ ОТВЕТСТВЕННОСТЬЮ «ТЕМИОН»',
    'ООО «ТЕМИОН»', '12300', 'ООО',
    '121096, г. Москва', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7730337754', 'Матвеев Игорь Сергеевич', 'Генеральный директор', '773001234567', '2014-07-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '7730337754', 'ООО «ТЕМИОН»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 37. ООО СР "Реком" (ИНН 3128060841)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '3128060841', NULL,
    'Общество с ограниченной ответственностью Специализированный регистратор "Реком"',
    'ООО СР "Реком"', '12300', 'ООО',
    '309502, г. Старый Оскол', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '3128060841', 'Громов Игорь Валерьевич', 'Генеральный директор', '312801234567', '2015-09-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '3128060841', 'ООО СР "Реком"', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- 38. АО «Сургутинвестнефть» (ИНН 8602039063)
-- ----------------------------------------------------------------------------
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name, okopf_code, okopf_name,
    legal_address, status, registration_date, fetched_at)
VALUES (gen_random_uuid(), '8602039063', NULL,
    'Акционерное общество "Сургутинвестнефть"',
    'АО "Сургутинвестнефть"', '12247', 'ПАО',
    '628415, г. Сургут', 'Действующее', '2002-01-01', NOW());
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '8602039063', 'Тарасов Алексей Николаевич', 'Генеральный директор', '860201234567', '2016-03-01', NOW());
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, head_of_other, founder_of_other, is_entrepreneur, ogrnip,
    share_amount, share_percent, entry_date, exit_date, fetched_at)
VALUES (gen_random_uuid(), '8602039063', 'АО «Сургутинвестнефть»', NULL, NULL, 'Россия', FALSE,
    NULL, NULL, NULL, NULL, NULL, FALSE, NULL, NULL, 100.00, '2002-01-01', NULL, NOW());

COMMIT;

-- ============================================================================
-- Тестовая страница TrueConf — заседание СД, вопросы, ответы
-- ============================================================================

-- Заседание СД
CREATE TABLE IF NOT EXISTS trueconf_test_meeting (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title                   VARCHAR(200) NOT NULL,
    description             TEXT,
    trueconf_conference_id  VARCHAR(100),
    trueconf_join_link      TEXT,
    conference_state        VARCHAR(50),
    started_at              TIMESTAMPTZ,
    ended_at                TIMESTAMPTZ,
    all_members_voted       BOOLEAN NOT NULL DEFAULT FALSE,
    decision_accepted       BOOLEAN,
    status                  VARCHAR(20) NOT NULL DEFAULT 'PREPARING',
    created_at              TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_trueconf_test_meeting_status ON trueconf_test_meeting(status);
CREATE INDEX IF NOT EXISTS ix_trueconf_test_meeting_conference ON trueconf_test_meeting(trueconf_conference_id);

-- Вопросы повестки
CREATE TABLE IF NOT EXISTS trueconf_test_question (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meeting_id          UUID NOT NULL REFERENCES trueconf_test_meeting(id) ON DELETE RESTRICT,
    sequence_number     INT NOT NULL,
    question_text       TEXT NOT NULL,
    proposed_resolution TEXT NOT NULL DEFAULT '',
    trueconf_poll_id    VARCHAR(100),
    poll_state          VARCHAR(20),
    status              VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    created_at          TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_trueconf_test_question_meeting ON trueconf_test_question(meeting_id);
CREATE INDEX IF NOT EXISTS ix_trueconf_test_question_poll ON trueconf_test_question(trueconf_poll_id);

-- Ответы (голоса)
CREATE TABLE IF NOT EXISTS trueconf_test_answer (
    id              SERIAL PRIMARY KEY,
    question_id     UUID NOT NULL REFERENCES trueconf_test_question(id) ON DELETE RESTRICT,
    user_name       VARCHAR(100) NOT NULL,
    vote_value      VARCHAR(20) NOT NULL,
    voted_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_trueconf_test_answer_question ON trueconf_test_answer(question_id);

-- ============================================================================
-- Демонстрационные данные ЦБ РФ (ext_cbr_finorg_organization, ext_cbr_finorg_license)
-- Регистраторы (VidID=4) и информационные агентства (FoType=IA)
-- Источник: SOAP-сервис cbr.ru/FO_ZoomWS/FinOrg.asmx
-- ============================================================================

-- ----------------------------------------------------------------------------
-- РЕГИСТРАТОРЫ (VidID=4) — ключевые участники рынка
-- ----------------------------------------------------------------------------

-- 1. АО «НРК - Р.О.С.Т.» — крупнейший регистратор
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name, eng_name,
    address, phones, email, okato, region, fo_types, status, is_sro_member, is_rss,
    is_npo, is_asv, reg_number, bic, bank_status, registration_date, has_branches,
    fund_value, web_sites, error, fetched_at)
VALUES (gen_random_uuid(), '7726030449', 1315037838974, '1027739216757',
    'Акционерное общество «Независимая регистраторская компания Р.О.С.Т.»',
    'АО «НРК - Р.О.С.Т.»', NULL,
    '107076, Г.МОСКВА, УЛ. СТРОМЫНКА, Д. 18, К. 5Б, ПОМЕЩ. IX',
    '+7 (495) 780-73-63', 'info@rrost.ru', 45, 'город Москва', 'PT,SD,OIP',
    'Active', true, false, false, false, NULL, NULL, NULL, NULL, false,
    NULL, 'www.rrost.ru', NULL, NOW());

INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, number, name, start_date, end_date, fetched_at)
VALUES
    (gen_random_uuid(), '7726030449', 4, 'Деятельность по ведению реестра владельцев ценных бумаг',
     '045-13976-000001', 'Ведение реестра владельцев ценных бумаг', '2002-12-03', NULL, NOW()),
    (gen_random_uuid(), '7726030449', 7, 'Депозитарная деятельность',
     '045-14179-000100', 'Депозитарная', '2023-04-04', NULL, NOW()),
    (gen_random_uuid(), '7726030449', 9, 'Деятельность специализированного депозитария инвестиционных фондов',
     '22-000-0-00127', 'Спецдепозитарная', '2023-07-06', NULL, NOW()),
    (gen_random_uuid(), '7726030449', 26, 'Деятельность по организации привлечения инвестиций',
     NULL, NULL, '2020-06-02', NULL, NOW());

-- 2. АО ВТБ Регистратор
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, phones, email, okato, region, fo_types, status, is_sro_member, web_sites, fetched_at)
VALUES (gen_random_uuid(), '5610083568', NULL, NULL,
    'Акционерное общество ВТБ Регистратор',
    'АО ВТБ Регистратор',
    '127015, Г. МОСКВА, УЛ. ПРАВДЫ, Д. 23', NULL, 'oip@vtbreg.ru',
    45, 'город Москва', 'PT', 'Active', true, 'www.vtbreg.com', NOW());

INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, number, name, start_date, end_date, fetched_at)
VALUES (gen_random_uuid(), '5610083568', 4, 'Деятельность по ведению реестра владельцев ценных бумаг',
     NULL, 'Ведение реестра', '2002-01-01', NULL, NOW());

-- 3. АО «Реестр»
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, phones, email, okato, region, fo_types, status, is_sro_member,
    fund_value, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7704028206', NULL, '1027700047275',
    'Акционерное общество «Реестр»',
    'АО «Реестр»',
    '129090, Г.МОСКВА, ПЕР. БОЛЬШОЙ БАЛКАНСКИЙ, Д. 20, СТР. 1',
    '8 (495) 617-01-01', 'reestr@aoreestr.ru',
    45, 'город Москва', 'PT,SD,OIP', 'Active', true,
    NULL, 'www.aoreestr.ru', NOW());

INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, number, name, start_date, end_date, fetched_at)
VALUES
    (gen_random_uuid(), '7704028206', 4, 'Деятельность по ведению реестра владельцев ценных бумаг',
     '045-13960-000001', 'Ведение реестра владельцев ценных бумаг', '2002-09-13', NULL, NOW()),
    (gen_random_uuid(), '7704028206', 7, 'Депозитарная деятельность',
     '045-14294-000100', 'Депозитарная', '2026-03-26', NULL, NOW()),
    (gen_random_uuid(), '7704028206', 9, 'Деятельность специализированного депозитария инвестиционных фондов',
     '22-000-0-00136', 'Спецдепозитарная', '2026-05-22', NULL, NOW());

-- 4. ООО «Реестр-РН»
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, email, okato, region, fo_types, status, is_sro_member, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7705397301', NULL, '1027700172818',
    'Общество с ограниченной ответственностью «Реестр-РН»',
    'ООО «Реестр-РН»',
    '115093, Г.МОСКВА, ПЕР. 1-Й ЩИПКОВСКИЙ, Д. 20',
    'support@reestrrn.ru', 45, 'город Москва', 'PT,OIP', 'Active', false,
    'www.reestrrn.ru', NOW());

INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, number, name, start_date, end_date, fetched_at)
VALUES
    (gen_random_uuid(), '7705397301', 4, 'Деятельность по ведению реестра владельцев ценных бумаг',
     '10-000-1-00330', 'Ведение реестра', '2004-12-16', NULL, NOW()),
    (gen_random_uuid(), '7705397301', 26, 'Деятельность по организации привлечения инвестиций',
     NULL, NULL, '2024-12-23', NULL, NOW());

-- 5. АО «ДРАГА» (Санкт-Петербург)
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, email, okato, region, fo_types, status, is_sro_member, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7704011964', NULL, NULL,
    'Акционерное общество «Специализированный регистратор - Держатель реестров акционеров газовой промышленности»',
    'АО «ДРАГА»',
    '190098, Г.САНКТ-ПЕТЕРБУРГ', 'info@draga.ru',
    NULL, 'город Санкт-Петербург', 'PT', 'Active', false,
    'draga.ru', NOW());

INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, number, name, start_date, end_date, fetched_at)
VALUES (gen_random_uuid(), '7704011964', 4, 'Деятельность по ведению реестра владельцев ценных бумаг',
     NULL, 'Ведение реестра', '2002-01-01', NULL, NOW());

-- 6. АО «АЭИ «ПРАЙМ» — информационное агентство
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, email, okato, region, fo_types, status, is_sro_member, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7703119309', NULL, NULL,
    'Акционерное общество «Агентство экономической информации «ПРАЙМ»',
    'АО «АЭИ «ПРАЙМ»',
    '125009, Г.МОСКВА', 'info@prime-interfax.ru',
    45, 'город Москва', 'IA', 'Active', false,
    'prime-interfax.ru', NOW());

-- 7. АО «СТАТУС»
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, email, okato, region, fo_types, status, is_sro_member, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7707179242', NULL, NULL,
    'Акционерное общество «Регистраторское общество «СТАТУС»',
    'АО «СТАТУС»',
    '109052, Г.МОСКВА, УЛ НОВОХОХЛОВСКАЯ', 'office@rostatus.ru',
    45, 'город Москва', 'PT', 'Active', true,
    'www.rostatus.ru', NOW());

INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, number, name, start_date, end_date, fetched_at)
VALUES (gen_random_uuid(), '7707179242', 4, 'Деятельность по ведению реестра владельцев ценных бумаг',
     NULL, 'Ведение реестра', '2002-01-01', NULL, NOW());

-- 8. АО «МРЦ»
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, email, okato, region, fo_types, status, is_sro_member, web_sites, fetched_at)
VALUES (gen_random_uuid(), '1901003859', NULL, NULL,
    'АКЦИОНЕРНОЕ ОБЩЕСТВО «МЕЖРЕГИОНАЛЬНЫЙ РЕГИСТРАТОРСКИЙ ЦЕНТР»',
    'АО «МРЦ»',
    '101000, Г.МОСКВА, ПЕР ПОДСОСЕНСКИЙ', 'info@mrz.ru',
    45, 'город Москва', 'PT', 'Active', false,
    'www.mrz.ru', NOW());

INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, number, name, start_date, end_date, fetched_at)
VALUES (gen_random_uuid(), '1901003859', 4, 'Деятельность по ведению реестра владельцев ценных бумаг',
     NULL, 'Ведение реестра', '2002-01-01', NULL, NOW());

-- 9. АО «РДЦ ПАРИТЕТ»
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, email, okato, region, fo_types, status, is_sro_member, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7723103642', NULL, NULL,
    'Акционерное общество «РДЦ ПАРИТЕТ»',
    'АО «РДЦ ПАРИТЕТ»',
    '115114, Г.МОСКВА', 'office@paritet.ru',
    45, 'город Москва', 'PT', 'Active', false,
    'www.paritet.ru', NOW());

INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, number, name, start_date, end_date, fetched_at)
VALUES (gen_random_uuid(), '7723103642', 4, 'Деятельность по ведению реестра владельцев ценных бумаг',
     NULL, 'Ведение реестра', '2002-01-01', NULL, NOW());

-- ----------------------------------------------------------------------------
-- ИНФОРМАЦИОННЫЕ АГЕНТСТВА (FoType=IA) — аккредитованные ИА
-- ----------------------------------------------------------------------------

-- 10. АНО «АЗИПИ»
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, email, okato, region, fo_types, status, is_sro_member, fetched_at)
VALUES (gen_random_uuid(), '7724252260', NULL, NULL,
    'Автономная некоммерческая организация «Агентство по стандартизации, информатизации и правовому обеспечению»',
    'АНО «АЗИПИ»',
    '125009, Г.МОСКВА', NULL,
    45, 'город Москва', 'IA', 'Active', false, NOW());

-- 11. ЗАО «Анализ, Консультации и Маркетинг»
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, email, okato, region, fo_types, status, is_sro_member, fetched_at)
VALUES (gen_random_uuid(), '7733014180', NULL, NULL,
    'Закрытое акционерное общество «Анализ, Консультации и Маркетинг»',
    'ЗАО «АКМ»',
    '125009, Г.МОСКВА', NULL,
    45, 'город Москва', 'IA', 'Active', false, NOW());

-- 12. ООО «Интерфакс – ЦРКИ»
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, email, okato, region, fo_types, status, is_sro_member, web_sites, fetched_at)
VALUES (gen_random_uuid(), '9710006645', NULL, NULL,
    'Общество с ограниченной ответственностью «Интерфакс – Центр раскрытия корпоративной информации»',
    'ООО «Интерфакс – ЦРКИ»',
    '125009, Г.МОСКВА', NULL,
    45, 'город Москва', 'IA', 'Active', false,
    'www.interfax.ru', NOW());

-- 13. ООО «СКРИН»
INSERT INTO ext_cbr_finorg_organization (id, inn, cbr_id, ogrn, full_name, short_name,
    address, email, okato, region, fo_types, status, is_sro_member, fetched_at)
VALUES (gen_random_uuid(), '9719011583', NULL, NULL,
    'Общество с ограниченной ответственностью «СКРИН»',
    'ООО «СКРИН»',
    '125009, Г.МОСКВА', NULL,
    45, 'город Москва', 'IA', 'Active', false, NOW());

-- ============================================================================
-- Дополнительные регистраторы (недостающие из документации)
-- Источник: docs/laws/article-registrars.md
-- ============================================================================

-- 14. АО «Реестр-Протон» (ИНН 9702074105)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '9702074105', 'Акционерное общество «Реестр-Протон»', 'АО «Реестр-Протон»',
    '129110, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.reestr-proton.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '9702074105', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 15. АО «СДК «Сириус»» (ИНН 9703197607)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '9703197607', 'Акционерное общество «Специализированная депозитарная компания «Сириус»', 'АО «СДК «Сириус»»',
    '123100, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.sdksirius.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '9703197607', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 16. АО «Сургутинвестнефть» (ИНН 8602039063)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '8602039063', 'Акционерное общество «Сургутинвестнефть»', 'АО «Сургутинвестнефть»',
    '628400, г. Сургут', NULL, 'Ханты-Мансийский автономный округ', 'PT', 'Active', 'www.sineft.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '8602039063', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 17. ООО «ЕАР» (ИНН 1660055801)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '1660055801', 'Общество с ограниченной ответственностью «Евроазиатский Регистратор»', 'ООО «ЕАР»',
    '420097, г. Казань', NULL, 'Республика Татарстан', 'PT', 'Active', 'www.earc.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '1660055801', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 18. ООО «Оборонрегистр» (ИНН 7731513346)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7731513346', 'Общество с ограниченной ответственностью «Оборонрегистр»', 'ООО «Оборонрегистр»',
    '105066, г. Москва, ул. Старая Басманная', NULL, 'город Москва', 'PT', 'Active', 'www.oboronregistr.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7731513346', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 19. ООО «ПАРТНЁР» (ИНН 3528218586)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '3528218586', 'Общество с ограниченной ответственностью «ПАРТНЁР»', 'ООО «ПАРТНЁР»',
    '162606, г. Череповец', NULL, 'Вологодская область', 'PT', 'Active', 'www.partner-reestr.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '3528218586', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 20. ООО «Регистратор "Гарант"» (ИНН 7703802628)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7703802628', 'Общество с ограниченной ответственностью «Регистратор "Гарант"»', 'ООО «Регистратор "Гарант"»',
    '123100, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.invest.reggarant.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7703802628', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 21. АО РК «Центр-Инвест» (ИНН 7726050935)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7726050935', 'Акционерное общество «Регистрационная Компания Центр-Инвест»', 'АО РК «Центр-Инвест»',
    '107023, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.centr-invest.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7726050935', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 22. АО «Новый регистратор» (ИНН 7719263354)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7719263354', 'Акционерное общество «Новый регистратор»', 'АО «Новый регистратор»',
    '107996, г. Москва, ул. Буженинова, д. 30', NULL, 'город Москва', 'PT', 'Active', 'www.newreg.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7719263354', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 23. АО «Индустрия-РЕЕСТР» (ИНН 3302021034)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '3302021034', 'Акционерное общество «Индустрия-РЕЕСТР»', 'АО «Индустрия-РЕЕСТР»',
    '107113, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.industria-reestr.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '3302021034', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 24. АО «РДЦ ПАРИТЕТ» (ИНН 7723103642)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7723103642', 'Акционерное общество «РДЦ ПАРИТЕТ»', 'АО «РДЦ ПАРИТЕТ»',
    '115114, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.paritet.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7723103642', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 25. АО «РЕГИСТРАТОР ИНТРАКО» (ИНН 5903027161)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '5903027161', 'Акционерное общество «Регистратор Интрако»', 'АО «РЕГИСТРАТОР ИНТРАКО»',
    '614000, г. Пермь', NULL, 'Пермский край', 'PT', 'Active', 'www.intraco.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '5903027161', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 26. АО «Регистратор-Капитал» (ИНН 6659035711)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '6659035711', 'Акционерное общество «Регистратор-Капитал»', 'АО «Регистратор-Капитал»',
    '620041, г. Екатеринбург', NULL, 'Свердловская область', 'PT', 'Active', 'www.regkap.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '6659035711', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 27. АО «ВРК» (ИНН 6661049239)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '6661049239', 'Акционерное общество «Ведение реестров компаний»', 'АО «ВРК»',
    '620014, г. Екатеринбург', NULL, 'Свердловская область', 'PT', 'Active', 'www.vrk.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '6661049239', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 28. АО «ПРЦ» (ИНН 3821010220)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '3821010220', 'Акционерное общество «Профессиональный регистрационный центр»', 'АО «ПРЦ»',
    '117452, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.profrc.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '3821010220', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 29. АО «Сервис-Реестр» (ИНН 8605006147)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '8605006147', 'Акционерное общество «Сервис-Реестр»', 'АО «Сервис-Реестр»',
    '107045, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.servis-reestr.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '8605006147', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 30. АО «СРК «КОМПАС»» (ИНН 4217027573)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '4217027573', 'Акционерное общество «Специализированный Регистратор "КОМПАС"»', 'АО «СРК «КОМПАС»»',
    '654005, г. Новокузнецк', NULL, 'Кемеровская область', 'PT', 'Active', 'www.in-ko.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '4217027573', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 31. АО «РТ-Регистратор» (ИНН 5407175878)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '5407175878', 'Акционерное общество «РТ-Регистратор»', 'АО «РТ-Регистратор»',
    '119049, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.rtreg.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '5407175878', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 32. АО РСР «ЯФЦ» (ИНН 1435001668)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '1435001668', 'Акционерное общество «Республиканский специализированный регистратор "Якутский Фондовый Центр"»', 'АО РСР «ЯФЦ»',
    '677018, г. Якутск', NULL, 'Республика Саха', 'PT', 'Active', 'www.yfc.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '1435001668', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 33. АО «Агентство «РНР»» (ИНН 7107039003)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7107039003', 'Акционерное общество «Агентство "Региональный независимый регистратор"»', 'АО «Агентство «РНР»»',
    '398017, г. Липецк', NULL, 'Липецкая область', 'PT', 'Active', 'www.a-rnr.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7107039003', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 34. АО «Вторая линия» (ИНН 9714072529)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '9714072529', 'Акционерное общество «Вторая линия»', 'АО «Вторая линия»',
    '125057, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.line2.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '9714072529', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 35. АО «ФРК» (ИНН 9718273177)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '9718273177', 'Акционерное общество «Фондовая регистрационная компания»', 'АО «ФРК»',
    '107076, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.frcreg.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '9718273177', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 36. АО «МРЦ» (ИНН 1901003859)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '1901003859', 'Акционерное общество «Межрегиональный регистрационный центр»', 'АО «МРЦ»',
    '101000, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.mrz.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '1901003859', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 37. АО «СТАТУС» (ИНН 7707179242)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7707179242', 'Акционерное общество «Регистраторское общество "СТАТУС"»', 'АО «СТАТУС»',
    '109052, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.rostatus.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7707179242', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 38. АО «ДРАГА» (ИНН 7704011964)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7704011964', 'Акционерное общество «Специализированный регистратор - Держатель реестров акционеров газовой промышленности»', 'АО «ДРАГА»',
    '190098, г. Санкт-Петербург', NULL, 'город Санкт-Петербург', 'PT', 'Active', 'www.draga.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7704011964', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 39. АО «АЭИ «ПРАЙМ»» (ИНН 7703119309)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7703119309', 'Акционерное общество «АЭИ "ПРАЙМ"»', 'АО «АЭИ «ПРАЙМ»»',
    '125009, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'prime-interfax.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7703119309', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 40. ООО «ЦУР» (ИНН 7842521215)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7842521215', 'Общество с ограниченной ответственностью «Центр учета и регистрации»', 'ООО «ЦУР»',
    '191124, г. Санкт-Петербург', NULL, 'город Санкт-Петербург', 'PT', 'Active', 'www.rrcentre.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7842521215', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 41. ООО «ЮРР» (ИНН 6166032022)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '6166032022', 'Общество с ограниченной ответственностью «Южно-Региональный регистратор»', 'ООО «ЮРР»',
    '344029, г. Ростов-на-Дону', NULL, 'Ростовская область', 'PT', 'Active', 'www.ug-rr.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '6166032022', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 42. ООО «Московский Фондовый Центр» (ИНН 7708822233)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7708822233', 'Общество с ограниченной ответственностью «Московский Фондовый Центр»', 'ООО «МФЦ»',
    '107078, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.srmfc.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7708822233', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 43. ООО «РБРУ СД» (ИНН 9704154155)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '9704154155', 'Общество с ограниченной ответственностью «РБРУ Специализированный депозитарий»', 'ООО «РБРУ СД»',
    '119002, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.rbru-depository.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '9704154155', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());

-- 44. ООО «ТЕМИОН» (ИНН 7730337754)
INSERT INTO ext_cbr_finorg_organization (id, inn, full_name, short_name, address, email, region, fo_types, status, web_sites, fetched_at)
VALUES (gen_random_uuid(), '7730337754', 'Общество с ограниченной ответственностью «ТЕМИОН»', 'ООО «ТЕМИОН»',
    '125009, г. Москва', NULL, 'город Москва', 'PT', 'Active', 'www.temion.ru', NOW());
INSERT INTO ext_cbr_finorg_license (id, organization_inn, vid_id, activity_name, name, start_date, fetched_at)
VALUES (gen_random_uuid(), '7730337754', 4, 'Деятельность по ведению реестра владельцев ценных бумаг', 'Ведение реестра', '2002-01-01', NOW());
