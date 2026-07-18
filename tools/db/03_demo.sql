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
