#!/bin/bash
# =============================================================================
# Пересборка + запуск обоих порталов (для VS Code Tasks)
# Держит терминал活ым через tail -f, чтобы процессы не убивались.
# =============================================================================
set -euo pipefail
cd "$(dirname "$0")"

echo "=== Остановка предыдущих процессов ==="
kill $(pgrep -f 'dotnet.*Fiducia') 2>/dev/null || true
sleep 1

echo "=== Сборка решения ==="
dotnet build -c Debug --nologo -v q

# Загрузка .env
if [ -f .env ]; then
    while IFS= read -r line; do
        [[ "$line" =~ ^#.*$ || -z "$line" ]] && continue
        export "$line"
    done < .env
fi

# PostgreSQL
if ! docker ps --filter "name=fiducia-postgres" --format "{{.Names}}" | grep -q fiducia-postgres; then
    echo "=== Запуск PostgreSQL ==="
    docker compose up -d postgres
    sleep 3
else
    echo "=== PostgreSQL уже запущен ==="
fi

echo "=== Запуск BoardPortal (порт 5002) ==="
ASPNETCORE_URLS="http://localhost:5002" \
ASPNETCORE_ENVIRONMENT=Development \
dotnet run --project SamorodinkaTech.Fiducia.BoardPortal \
    --no-restore --no-launch-profile \
    > /tmp/fiducia-boardportal.log 2>&1 &

echo "=== Запуск AdminConsole (порт 5001) ==="
ASPNETCORE_URLS="http://localhost:5001" \
ASPNETCORE_ENVIRONMENT=Development \
dotnet run --project SamorodinkaTech.Fiducia.AdminConsole \
    --no-restore --no-launch-profile \
    > /tmp/fiducia-adminconsole.log 2>&1 &

sleep 5

# Проверка
curl -sI http://localhost:5002 > /dev/null 2>&1 && echo "✅ Board Portal:  http://localhost:5002/login" || echo "❌ Board Portal не ответил"
curl -sI http://localhost:5001 > /dev/null 2>&1 && echo "✅ Admin Console: http://localhost:5001/login" || echo "❌ Admin Console не ответил"

echo ""
echo "Логи:"
echo "  tail -f /tmp/fiducia-boardportal.log"
echo "  tail -f /tmp/fiducia-adminconsole.log"
echo ""
echo "Остановка:  kill \$(pgrep -f 'dotnet.*Fiducia')"
echo ""
echo "--- Tail логов (Ctrl+C для остановки) ---"

# Держим терминал活ым — tail обеих логов
tail -f /tmp/fiducia-boardportal.log /tmp/fiducia-adminconsole.log
