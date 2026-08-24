-- reset_schema.sql — Очистка БД и повторное применение схемы
-- ВНИМАНИЕ: Удаляет ВСЕ данные и объекты в БД!

-- Отключаем проверку внешних ключей для быстрого удаления
SET session_replication_role = 'replica';

-- Удаляем все таблицы (включая данные)
DO $$ DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
        EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(r.tablename) || ' CASCADE';
    END LOOP;
END $$;

-- Удаляем все последовательности
DO $$ DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT sequence_name FROM information_schema.sequences WHERE sequence_schema = 'public') LOOP
        EXECUTE 'DROP SEQUENCE IF EXISTS public.' || quote_ident(r.sequence_name) || ' CASCADE';
    END LOOP;
END $$;

-- Удаляем все индексы (они удаляются вместе с таблицами, но на всякий случай)
DO $$ DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT indexname FROM pg_indexes WHERE schemaname = 'public') LOOP
        EXECUTE 'DROP INDEX IF EXISTS public.' || quote_ident(r.indexname);
    END LOOP;
END $$;

-- Восстанавливаем обычный режим проверки внешних ключей
SET session_replication_role = 'origin';

-- Применяем схему из 01_schema.sql
--\i /Users/evgenij/Проекты/SamorodinkaTech.Fiducia/tools/db/01_schema.sql

-- Применяем seed-данные из 02_seed.sql (если нужно)
-- \i /Users/evgenij/Проекты/SamorodinkaTech.Fiducia/tools/db/02_seed.sql

-- Применяем демо-данные из 03_demo.sql (если нужно)
-- \i /Users/evgenij/Проекты/SamorodinkaTech.Fiducia/tools/db/03_demo.sql

-- Проверяем результат
SELECT 'Schema dropped successfully!' AS status;
