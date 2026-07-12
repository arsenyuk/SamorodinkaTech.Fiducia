-- 02_seed.sql — первичное наполнение (справочники и системные записи)

INSERT INTO ref_roles (id, role_code, role_name) VALUES
    ('11111111-1111-1111-1111-111111111111','SYS_ADMIN','Системный администратор'),
    ('22222222-2222-2222-2222-222222222222','CORP_SECRETARY','Корпоративный секретарь'),
    ('33333333-3333-3333-3333-333333333333','CHAIR_BOARD','Председатель СД'),
    ('44444444-4444-4444-4444-444444444444','MEMBER_BOARD','Член СД'),
    ('55555555-5555-5555-5555-555555555555','EXTERNAL_DIRECTOR','Внешний/Независимый директор'),
    ('66666666-6666-6666-6666-666666666666','SHAREHOLDER','Акционер'),
    ('77777777-7777-7777-7777-777777777777','COMMITTEE_CHAIR','Председатель комитета'),
    ('88888888-8888-8888-8888-888888888888','COMMITTEE_MEMBER','Член комитета')
ON CONFLICT (role_code) DO NOTHING;

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

-- Форма проведения ОСА
INSERT INTO ref_osa_form(id, code, name) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1','GOSA','Годовое общее собрание акционеров'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2','VOSA','Внеочередное общее собрание акционеров')
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
