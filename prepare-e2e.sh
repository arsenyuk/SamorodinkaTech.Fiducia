#!/bin/bash
# =============================================================================
# prepare-e2e.sh — Стандартная подготовка к запуску E2E-тестов
# =============================================================================
# Выполняет полный цикл подготовки:
#   1. Остановка порталов
#   2. Загрузка .env
#   3. Сборка решения
#   4. Запуск Docker-контейнеров
#   5. Ожидание готовности
#   6. Сброс БД Fiducia
#   7. Сброс БД mnemonios
#   8. Seed MPI
#   9. (опционально) Запуск порталов
#
# Опции:
#   --with-portals    Запустить порталы после подготовки
#   --skip-mnemonios  Пропустить сброс БД mnemonios
#   --skip-mpi        Пропустить seed MPI
#   --help            Справка
# =============================================================================

set -euo pipefail
cd "$(dirname "$0")"

# ── Опции ────────────────────────────────────────────────────────────────────
WITH_PORTALS=false
SKIP_MNEMONIOS=false
SKIP_MPI=false

for arg in "$@"; do
    case "$arg" in
        --with-portals)  WITH_PORTALS=true ;;
        --skip-mnemonios) SKIP_MNEMONIOS=true ;;
        --skip-mpi)      SKIP_MPI=true ;;
        --help)
            echo "Использование: ./prepare-e2e.sh [ОПЦИИ]"
            echo ""
            echo "Опции:"
            echo "  --with-portals    Запустить порталы после подготовки"
            echo "  --skip-mnemonios  Пропустить сброс БД mnemonios"
            echo "  --skip-mpi        Пропустить seed MPI"
            echo "  --help            Справка"
            exit 0
            ;;
        *)
            echo "❌ Неизвестная опция: $arg"
            exit 1
            ;;
    esac
done

# ── Шаг 1: Остановка порталов ────────────────────────────────────────────────
echo "=== [1/8] Остановка порталов ==="
pkill -f 'dotnet.*AdminConsole' 2>/dev/null || true
pkill -f 'dotnet.*BoardPortal' 2>/dev/null || true
sleep 2
echo "  Порталы остановлены."

# ── Шаг 2: Загрузка .env ─────────────────────────────────────────────────────
echo "=== [2/8] Загрузка .env ==="
if [ -f .env ]; then
    set -a
    source .env
    set +a
    echo "  Переменные окружения загружены."
else
    echo "  ⚠️  Файл .env не найден, используются значения по умолчанию."
fi

# ── Шаг 3: Сборка решения ────────────────────────────────────────────────────
echo "=== [3/8] Сборка решения ==="
if dotnet build; then
    echo "  ✅ Сборка завершена успешно."
else
    echo "  ❌ Ошибка сборки. Исправьте ошибки и повторите запуск."
    exit 1
fi

# ── Шаг 4: Запуск Docker-контейнеров ────────────────────────────────────────
echo "=== [4/8] Запуск Docker-контейнеров ==="

# PostgreSQL
if ! docker ps --filter "name=fiducia-postgres" --format "{{.Names}}" | grep -q fiducia-postgres; then
    echo "  Запуск PostgreSQL..."
    docker compose up -d postgres
else
    echo "  PostgreSQL уже запущен."
fi

# LDAP
if ! docker ps --filter "name=fiducia-ldap" --format "{{.Names}}" | grep -q fiducia-ldap; then
    echo "  Запуск LDAP..."
    docker compose -f docker-compose.ldap.yml up -d
else
    echo "  LDAP уже запущен."
fi

# ЕДИН (mnemonios)
if ! docker ps --filter "name=fiducia-mnemonios" --format "{{.Names}}" | grep -q fiducia-mnemonios; then
    echo "  Запуск ЕДИН (mnemonios)..."
    docker compose up -d mnemonios-postgres mnemonios
else
    echo "  ЕДИН (mnemonios) уже запущен."
fi

# ── Шаг 5: Ожидание готовности ──────────────────────────────────────────────
echo "=== [5/8] Ожидание готовности сервисов ==="

# PostgreSQL
echo -n "  PostgreSQL: "
for i in $(seq 1 30); do
    if docker exec fiducia-postgres pg_isready -U fiducia -d fiducia >/dev/null 2>&1; then
        echo "✅ готов"
        break
    fi
    if [ "$i" -eq 30 ]; then
        echo "❌ не готов после 30 попыток"
        exit 1
    fi
    sleep 2
done

# LDAP
echo -n "  LDAP: "
for i in $(seq 1 15); do
    if ldapsearch -x -H ldap://localhost -D "cn=admin,dc=fiducia,dc=local" -w admin -b "dc=fiducia,dc=local" "(objectClass=*)" dn >/dev/null 2>&1; then
        echo "✅ готов"
        break
    fi
    if [ "$i" -eq 15 ]; then
        echo "❌ не готов после 15 попыток"
        exit 1
    fi
    sleep 2
done

# ЕДИН (mnemonios)
echo -n "  ЕДИН (mnemonios): "
for i in $(seq 1 30); do
    if curl -s -o /dev/null -w "%{http_code}" http://localhost:5010/ 2>/dev/null | grep -qE "^[234]"; then
        echo "✅ готов"
        break
    fi
    if [ "$i" -eq 30 ]; then
        echo "❌ не готов после 30 попыток"
        exit 1
    fi
    sleep 2
done

# ── Шаг 6: Сброс БД Fiducia ────────────────────────────────────────────────
echo "=== [6/8] Сброс БД Fiducia ==="
docker exec -i fiducia-postgres psql -U fiducia -d fiducia < tools/db/00_reset_schema.sql >/dev/null 2>&1
docker exec -i fiducia-postgres psql -U fiducia -d fiducia < tools/db/01_schema.sql >/dev/null 2>&1
docker exec -i fiducia-postgres psql -U fiducia -d fiducia < tools/db/02_seed.sql >/dev/null 2>&1
echo "  ✅ БД Fiducia сброшена и заполнена seed-данными."

# ── Шаг 7: Сброс БД mnemonios ──────────────────────────────────────────────
if [ "$SKIP_MNEMONIOS" = false ]; then
    echo "=== [7/8] Сброс БД mnemonios ==="
    docker exec -i fiducia-mnemonios-postgres psql -U mnemonios -d mnemonios \
        -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;" >/dev/null 2>&1
    docker exec -i fiducia-mnemonios-postgres psql -U mnemonios -d mnemonios \
        < ../SamorodinkaTech.Mnemonios/tools/db/01_schema.sql >/dev/null 2>&1
    docker exec -i fiducia-mnemonios-postgres psql -U mnemonios -d mnemonios \
        < ../SamorodinkaTech.Mnemonios/tools/db/02_seed.sql >/dev/null 2>&1
    echo "  ✅ БД mnemonios сброшена и заполнена seed-данными."
else
    echo "=== [7/8] Пропуск сброса БД mnemonios (--skip-mnemonios) ==="
fi

# ── Шаг 8: Seed MPI ─────────────────────────────────────────────────────────
if [ "$SKIP_MPI" = false ]; then
    echo "=== [8/8] Seed MPI (ЕДИН API → LDAP) ==="
    if bash tools/mpi/seed-mpi.sh http://localhost:5010 localhost admin --output /tmp/generated-mpi.ldif; then
        echo "  ✅ Seed MPI завершён."
    else
        echo "  ⚠️  Seed MPI завершился с ошибкой. Проверьте логи mnemonios."
    fi
else
    echo "=== [8/8] Пропуск seed MPI (--skip-mpi) ==="
fi

# ── Шаг 9 (опционально): Запуск порталов ────────────────────────────────────
if [ "$WITH_PORTALS" = true ]; then
    echo ""
    echo "=== Запуск порталов ==="
    ./start.sh
fi

# ── Итог ─────────────────────────────────────────────────────────────────────
echo ""
echo "=========================================="
echo "✅ Подготовка к E2E-тестам завершена."
echo "=========================================="
echo ""
echo "Для запуска тестов:"
echo "  tests/SamorodinkaTech.Fiducia.Tests.Functional/bin/Debug/net9.0/SamorodinkaTech.Fiducia.Tests.Functional --filter \"<имя_теста>\""
echo ""
echo "Для запуска порталов:"
echo "  ./start.sh"
echo ""
echo "Остановка порталов:"
echo "  kill \$(pgrep -f 'dotnet.*Fiducia')"
