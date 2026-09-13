# База данных

Платформа «Цифровой Совет Директоров» использует PostgreSQL 16. Модель построена на принципах Database-First (BDR-002): каноническая схема — `tools/db/01_schema.sql`.

---

## Документация

| Раздел | Файл | Описание |
|--------|------|----------|
| ER-диаграмма | [database-schema.md](database-schema.md) | Mermaid ER-схема с ключами и связями |
| Описание таблиц | [database-tables.md](database-tables.md) | Все таблицы с полями, типами и ограничениями |

---

## Структура SQL-скриптов

```
tools/db/
├── 00_reset_schema.sql   # DROP всех объектов + применение 01_schema.sql
├── 01_schema.sql         # Каноническая схема БД (DDL)
├── 02_seed.sql           # Справочники ref_* и системные записи
└── 03_demo.sql           # Демо-данные для разработки
```

## Применение SQL-изменений

1. Поднять Postgres: `docker compose up -d postgres`
2. Применить скрипты по порядку:
   - Схема: `cat tools/db/01_schema.sql | docker exec -i fiducia-postgres psql -U fiducia -d fiducia -v ON_ERROR_STOP=1`
   - Справочники: `cat tools/db/02_seed.sql | docker exec -i fiducia-postgres psql -U fiducia -d fiducia -v ON_ERROR_STOP=1`
   - Демо-данные: `cat tools/db/03_demo.sql | docker exec -i fiducia-postgres psql -U fiducia -d fiducia -v ON_ERROR_STOP=1`
3. Проверить: `docker exec -it fiducia-postgres psql -U fiducia -d fiducia -c "\\dt"`
