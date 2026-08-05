-- =============================================================================
-- Пример данных СПАРК для ООО (legal_entities, ext_spark_company, 
-- ext_spark_manager, ext_spark_founder)
-- =============================================================================
-- Назначение: демонстрация заполнения кэша СПАРК и отображения на странице
-- «Участники» (Board Portal) и в Admin Console → Юридические лица.
-- =============================================================================

BEGIN;

-- 1. Юридическое лицо: предполагается, что ООО уже существует в legal_entities.
--    Пример работает с существующей записью (inn='7703382710', okopf_code='12300').
--    Если нужно создать новое ЮЛ — раскомментировать блок ниже.
/*
INSERT INTO legal_entities (id, name, short_name, inn, ogrn, okopf_id)
SELECT
    gen_random_uuid(),
    'Общество с ограниченной ответственностью «Интернет Решения»',
    'Ozon',
    '7703382710',
    '1027739520339',
    id
FROM ref_okopf
WHERE code = '12300'
ON CONFLICT (id) DO NOTHING;
*/

-- 2. Карточка компании из СПАРК (ext_spark_company)
INSERT INTO ext_spark_company (id, inn, ogrn, full_name, short_name,
    okopf_code, okopf_name, legal_address, status, registration_date,
    shareholders_count, employees_count, fetched_at)
VALUES (
    gen_random_uuid(),
    '7703382710',
    '1027739520339',
    'Общество с ограниченной ответственностью «Интернет Решения»',
    'ООО «Интернет Решения»',
    '12300',
    'Общество с ограниченной ответственностью (ООО)',
    '123112, г. Москва, Пресненская наб., д. 10, эт. 41, пом. I, ком. 6',
    'Действующее',
    '1998-05-06',
    NULL,     -- для ООО количество акционеров неприменимо
    45000,    -- сотрудников
    NOW()
)
ON CONFLICT DO NOTHING;

-- 3. Руководитель из СПАРК (ext_spark_manager)
INSERT INTO ext_spark_manager (id, inn, full_name, position, person_inn,
    start_date, fetched_at)
VALUES (
    gen_random_uuid(),
    '7703382710',
    'Шульгин Александр Сергеевич',
    'Генеральный директор',
    '772912345678',
    '2017-12-01',
    NOW()
)
ON CONFLICT DO NOTHING;

-- 4. Учредители (участники) из СПАРК (ext_spark_founder)
--    Состав: 1 ЮЛ (99,99%) + 2 ФЛ (миноритарии)
INSERT INTO ext_spark_founder (id, inn, name, founder_inn,
    full_name, person_inn, share_amount, share_percent, fetched_at)
VALUES
    -- Учредитель-ЮЛ: ООО «Озон Холдинг»
    (
        gen_random_uuid(),
        '7703382710',
        'Общество с ограниченной ответственностью «Озон Холдинг»',
        '7704356731',
        NULL,
        NULL,
        999900.00,
        99.99,
        NOW()
    ),
    -- Учредитель-ФЛ
    (
        gen_random_uuid(),
        '7703382710',
        NULL,
        NULL,
        'Минаев Алексей Владимирович',
        '771501234567',
        50.00,
        0.005,
        NOW()
    ),
    -- Учредитель-ФЛ
    (
        gen_random_uuid(),
        '7703382710',
        NULL,
        NULL,
        'Петрова Елена Игоревна',
        '773212345678',
        50.00,
        0.005,
        NOW()
    );

COMMIT;
