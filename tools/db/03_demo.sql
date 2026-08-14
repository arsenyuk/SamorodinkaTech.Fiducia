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
