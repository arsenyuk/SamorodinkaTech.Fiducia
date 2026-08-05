-- 03_demo.sql — демонстрационные данные (не для продакшна)

-- Фиксированные UUID для детерминированного состояния
-- Пользователь-администратор
INSERT INTO users (id, last_name, first_name, middle_name, email, phone, is_external, pep_agreement_signed, pep_signed_at)
VALUES ('99999999-9999-9999-9999-999999999999','Смирнов','Дмитрий','Олегович','admin@company.ru','+79005550000', FALSE, TRUE, CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;

-- Роль SYS_ADMIN → см. tools/db/02_seed.sql: '11111111-1111-1111-1111-111111111111'
INSERT INTO user_roles(id, user_id, role_id)
VALUES ('dddddddd-dddd-dddd-dddd-ddddddddddd0','99999999-9999-9999-9999-999999999999','11111111-1111-1111-1111-111111111111')
ON CONFLICT DO NOTHING;

-- Демо-пользователи для Board Portal (Basic) — чтобы Login отрисовывал dropdown
INSERT INTO users (id, last_name, first_name, middle_name, email, phone, is_external, pep_agreement_signed, pep_signed_at)
VALUES
    ('11111111-aaaa-bbbb-cccc-111111111111','Иванов','Иван','Иванович','ivanov@example.com','+79001001010', FALSE, TRUE, CURRENT_TIMESTAMP),
    ('22222222-aaaa-bbbb-cccc-222222222222','Петров','Пётр','Петрович','petrov@example.com','+79002002020', FALSE, TRUE, CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;

-- Демонстрационные ЮЛ (ПАО) — используют ref_okopf(code='12247') c фиксированным id
-- Предполагается, что в 02_seed.sql ref_okopf содержит запись:
-- id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', code='12247'
INSERT INTO legal_entities (id, name, short_name, inn, ogrn, okopf_id) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1','ПАО «Сбербанк России»','ПАО Сбербанк','7707083893','1027700132195','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2','ПАО «Газпром»','ПАО Газпром','7736050003','1027700070518','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3','ПАО «Лукойл»','ПАО ЛУКОЙЛ','7708004767','1027700035769','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1')
ON CONFLICT (id) DO NOTHING;

-- Демонстрационные ЮЛ (НАО) — ref_okopf(code='12267'), id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2'
INSERT INTO legal_entities (id, name, short_name, inn, ogrn, okopf_id) VALUES
    ('cccccccc-cccc-cccc-cccc-ccccccccccc1','НАО «Трансмашхолдинг»','Трансмашхолдинг','7701555535','1027739300682','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2'),
    ('cccccccc-cccc-cccc-cccc-ccccccccccc2','НАО «Национальная Медиа Группа»','НМГ','7842334933','1077847578434','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2')
ON CONFLICT (id) DO NOTHING;

-- Демонстрационные ЮЛ (ООО) — ref_okopf(code='12300'), id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4'
INSERT INTO legal_entities (id, name, short_name, inn, ogrn, okopf_id) VALUES
    ('dddddddd-dddd-dddd-dddd-ddddddddddd1','ООО «Яндекс»','Яндекс','7736207543','1027700229193','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4'),
    ('dddddddd-dddd-dddd-dddd-ddddddddddd2','ООО «Вайлдберриз»','Wildberries','7721546864','1067746062449','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4'),
    ('dddddddd-dddd-dddd-dddd-ddddddddddd3','ООО «Озон»','Ozon','7703382710','1027739013283','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4')
ON CONFLICT (id) DO NOTHING;

-- Демонстрационные ЮЛ (ФГУП) — ref_okopf(code='65241'), id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5'
INSERT INTO legal_entities (id, name, short_name, inn, ogrn, okopf_id) VALUES
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1','ФГУП «Почта России»','Почта России','7724261610','1037724007276','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2','ФГУП «Гознак»','Гознак','7813252159','1027810235689','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5')
ON CONFLICT (id) DO NOTHING;

-- Демонстрационные ЮЛ (ГУП) — ref_okopf(code='65242'), id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6'
INSERT INTO legal_entities (id, name, short_name, inn, ogrn, okopf_id) VALUES
    ('ffffffff-ffff-ffff-ffff-fffffffffff1','ГУП «Мосгортранс»','Мосгортранс','7705002600','1037739376223','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6'),
    ('ffffffff-ffff-ffff-ffff-fffffffffff2','ГУП «Московский метрополитен»','Московский метрополитен','7702038150','1027700096280','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6')
ON CONFLICT (id) DO NOTHING;

-- Демонстрационные ЮЛ (МУП) — ref_okopf(code='65243'), id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7'
INSERT INTO legal_entities (id, name, short_name, inn, ogrn, okopf_id) VALUES
    ('00000000-0000-0000-0000-000000000001','МУП «Водоканал» г. Екатеринбург','Водоканал Екб','6608001915','1036603485962','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7'),
    ('00000000-0000-0000-0000-000000000002','МУП «Горэлектротранс» г. Новосибирск','Горэлектротранс Нск','5406101424','1025401018557','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7')
ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- Демонстрационные данные СПАРК для ООО (ext_spark_company, ext_spark_manager,
-- ext_spark_founder)
-- ============================================================================

BEGIN;

-- Очищаем старые демо-данные для трёх ООО
DELETE FROM ext_spark_founder WHERE inn IN ('7736207543','7721546864','7703382710');
DELETE FROM ext_spark_manager WHERE inn IN ('7736207543','7721546864','7703382710');
DELETE FROM ext_spark_company WHERE inn IN ('7736207543','7721546864','7703382710');

-- ----------------------------------------------------------------------------
-- 1. ООО «Яндекс» (ИНН 7736207543)
-- ----------------------------------------------------------------------------

-- Карточка компании
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name,
    okopf_code, okopf_name, legal_address, status, registration_date,
    employees_count, fetched_at)
VALUES (gen_random_uuid(), '7736207543', '1027700229193',
    'Общество с ограниченной ответственностью «Яндекс»',
    'ООО «Яндекс»',
    '12300', 'Общество с ограниченной ответственностью (ООО)',
    '119021, г. Москва, ул. Льва Толстого, д. 16',
    'Действующее', '2000-01-19', 25000, NOW());

-- Руководитель
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7736207543',
    'Кудрин Максим Львович', 'Генеральный директор', '780401234567',
    '2023-05-15', NOW());

-- Учредители (1 ЮЛ + 1 ФЛ)
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, share_amount, share_percent,
    entry_date, exit_date, director_count, fetched_at)
VALUES
    (gen_random_uuid(), '7736207543',
     'Акционерное общество «Яндекс.Технологии»', '9705012345', '1207700420500',
     'Россия', FALSE,
     NULL, NULL, NULL,
     900000.00, 90.00,
     '2019-12-18', NULL, NULL, NOW()),
    (gen_random_uuid(), '7736207543',
     NULL, NULL, NULL, NULL, FALSE,
     'Волож Аркадий Юрьевич', '772401234567', 'Мальта',
     100000.00, 10.00,
     '2000-01-19', NULL, 3, NOW());

-- ----------------------------------------------------------------------------
-- 2. ООО «Вайлдберриз» (ИНН 7721546864)
-- ----------------------------------------------------------------------------

-- Карточка компании
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name,
    okopf_code, okopf_name, legal_address, status, registration_date,
    employees_count, fetched_at)
VALUES (gen_random_uuid(), '7721546864', '1067746062449',
    'Общество с ограниченной ответственностью «Вайлдберриз»',
    'ООО «Вайлдберриз»',
    '12300', 'Общество с ограниченной ответственностью (ООО)',
    '142181, Московская обл., г. Подольск, д. Коледино, тер. Индустриальная, д. 10',
    'Действующее', '2004-04-20', 48000, NOW());

-- Руководитель
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7721546864',
    'Бакальчук Татьяна Владимировна', 'Генеральный директор', '501201234567',
    '2004-01-16', NOW());

-- Учредители (2 ФЛ)
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, share_amount, share_percent,
    entry_date, exit_date, director_count, fetched_at)
VALUES
    (gen_random_uuid(), '7721546864',
     NULL, NULL, NULL, NULL, FALSE,
     'Бакальчук Татьяна Владимировна', '501201234567', 'Россия',
     990000.00, 99.00,
     '2004-04-20', NULL, 1, NOW()),
    (gen_random_uuid(), '7721546864',
     NULL, NULL, NULL, NULL, FALSE,
     'Бакальчук Владислав Сергеевич', '501208765432', 'Россия',
     10000.00, 1.00,
     '2004-04-20', NULL, 0, NOW());

-- ----------------------------------------------------------------------------
-- 3. ООО «Озон» (ИНН 7703382710)
-- ----------------------------------------------------------------------------

-- Карточка компании
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name,
    okopf_code, okopf_name, legal_address, status, registration_date,
    employees_count, fetched_at)
VALUES (gen_random_uuid(), '7703382710', '1027739013283',
    'Общество с ограниченной ответственностью «Интернет Решения»',
    'ООО «Интернет Решения»',
    '12300', 'Общество с ограниченной ответственностью (ООО)',
    '123112, г. Москва, Пресненская наб., д. 10, эт. 41, пом. I, ком. 6',
    'Действующее', '1998-05-06', 45000, NOW());

-- Руководитель
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn, start_date, fetched_at)
VALUES (gen_random_uuid(), '7703382710',
    'Шульгин Александр Сергеевич', 'Генеральный директор', '772912345678',
    '2017-12-01', NOW());

-- Учредители (1 ЮЛ + 2 действующих ФЛ + 1 выбывший ФЛ)
INSERT INTO ext_spark_founder (id, inn, name, founder_inn, founder_ogrn, country, is_foreign,
    full_name, person_inn, citizenship, share_amount, share_percent,
    entry_date, exit_date, director_count, fetched_at)
VALUES
    (gen_random_uuid(), '7703382710',
     'Общество с ограниченной ответственностью «Озон Холдинг»', '7704356731', '5147746215061',
     'Россия', FALSE,
     NULL, NULL, NULL,
     999900.00, 99.99,
     '2014-05-20', NULL, NULL, NOW()),
    (gen_random_uuid(), '7703382710',
     NULL, NULL, NULL, NULL, FALSE,
     'Минаев Алексей Владимирович', '771501234567', 'Россия',
     50.00, 0.005,
     '2013-02-10', NULL, 2, NOW()),
    (gen_random_uuid(), '7703382710',
     NULL, NULL, NULL, NULL, FALSE,
     'Петрова Елена Игоревна', '773212345678', 'Россия',
     50.00, 0.005,
     '2013-02-10', NULL, 0, NOW()),
    -- Выбывший учредитель: продал долю при входе стратегического инвестора
    (gen_random_uuid(), '7703382710',
     NULL, NULL, NULL, NULL, FALSE,
     'Агафонов Дмитрий Павлович', '771801234567', 'Россия',
     50000.00, 5.00,
     '2004-09-01', '2014-05-19', 4, NOW());

COMMIT;
