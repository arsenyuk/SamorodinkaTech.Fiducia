#!/bin/bash
# =============================================================================
# Запуск обоих порталов Fiducia
# =============================================================================
# Board Portal: http://localhost:5002/login   (5000 занят AirPlay)
# Admin Console: http://localhost:5001/login
# =============================================================================

set -euo pipefail
cd "$(dirname "$0")"

echo "=== Fiducia — запуск порталов ==="

# 1. PostgreSQL
if ! docker ps --filter "name=fiducia-postgres" --format "{{.Names}}" | grep -q fiducia-postgres; then
    echo "[1/3] Запуск PostgreSQL..."
    docker compose up -d postgres
    sleep 3
else
    echo "[1/3] PostgreSQL уже запущен"
fi

# 2. BoardPortal (порт 5002)
echo "[2/3] Запуск BoardPortal (порт 5002)..."
ASPNETCORE_URLS="http://localhost:5002" \
ASPNETCORE_ENVIRONMENT=Development \
dotnet run --project SamorodinkaTech.Fiducia.BoardPortal \
    --no-restore --no-launch-profile \
    > /tmp/fiducia-boardportal.log 2>&1 &
echo "  PID: $!"

# 3. AdminConsole (порт 5001)
echo "[3/3] Запуск AdminConsole (порт 5001)..."
ASPNETCORE_URLS="http://localhost:5001" \
ASPNETCORE_ENVIRONMENT=Development \
dotnet run --project SamorodinkaTech.Fiducia.AdminConsole \
    --no-restore --no-launch-profile \
    > /tmp/fiducia-adminconsole.log 2>&1 &
echo "  PID: $!"

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