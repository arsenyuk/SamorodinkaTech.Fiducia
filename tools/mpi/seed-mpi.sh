#!/usr/bin/env bash
# =============================================================================
# seed-mpi.sh — Seed MPI MasterId через ЕДИН API → LDAP
# =============================================================================
# Запуск: bash tools/mpi/seed-mpi.sh [един_url] [ldap_host] [ldap_password]
# По умолчанию: http://localhost:5010, localhost, admin
# =============================================================================

set -euo pipefail

EDIN_URL="${1:-http://localhost:5010}"
LDAP_HOST="${2:-localhost}"
LDAP_PASSWORD="${3:-admin}"
LDAP_BASE="dc=fiducia,dc=local"
LDAP_BIND_DN="cn=admin,${LDAP_BASE}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PERSONS_FILE="${SCRIPT_DIR}/test-persons.json"
OUTPUT_LDIF="${SCRIPT_DIR}/generated-mpi.ldif"

# ── Проверка зависимостей ──────────────────────────────────────────────────
for cmd in curl jq ldapadd; do
    if ! command -v "$cmd" &>/dev/null; then
        echo "ОШИБКА: $cmd не найден. Установите: brew install $cmd" >&2
        exit 1
    fi
done

# ── Ожидание ЕДИН API ─────────────────────────────────────────────────────
echo "Ожидание ЕДИН API: ${EDIN_URL}"
for i in $(seq 1 30); do
    if curl -s -o /dev/null -w "%{http_code}" "${EDIN_URL}/persons/resolve" \
        -X POST -H "Content-Type: application/json" -d '{}' 2>/dev/null | grep -qE "^[234]"; then
        echo "ЕДИН API готов"
        break
    fi
    if [ "$i" -eq 30 ]; then
        echo "ОШИБКА: ЕДИН API не доступен после 30 попыток" >&2
        exit 1
    fi
    sleep 2
done

# ── Чтение тестовых лиц ───────────────────────────────────────────────────
PERSON_COUNT=$(jq length "$PERSONS_FILE")
echo "Обработка ${PERSON_COUNT} тестовых лиц..."

# ── Генерация LDIF ────────────────────────────────────────────────────────
echo "# Auto-generated MPI MasterId LDIF — $(date -u +%Y-%m-%dT%H:%M:%SZ)" > "$OUTPUT_LDIF"
echo "" >> "$OUTPUT_LDIF"

SUCCESS=0
FAIL=0

for i in $(seq 0 $((PERSON_COUNT - 1))); do
    LOGIN=$(jq -r ".[$i].login" "$PERSONS_FILE")
    LAST_NAME=$(jq -r ".[$i].lastName" "$PERSONS_FILE")
    FIRST_NAME=$(jq -r ".[$i].firstName" "$PERSONS_FILE")
    MIDDLE_NAME=$(jq -r ".[$i].middleName // empty" "$PERSONS_FILE")
    INN=$(jq -r ".[$i].inn // empty" "$PERSONS_FILE")
    SNILS=$(jq -r ".[$i].snils // empty" "$PERSONS_FILE")
    DUL_TYPE=$(jq -r ".[$i].dulType // empty" "$PERSONS_FILE")
    DUL_SERIES=$(jq -r ".[$i].dulSeries // empty" "$PERSONS_FILE")
    DUL_NUMBER=$(jq -r ".[$i].dulNumber // empty" "$PERSONS_FILE")
    LDAP_DN=$(jq -r ".[$i].ldapDn" "$PERSONS_FILE")

    echo -n "  [${LOGIN}] Resolve: ${LAST_NAME} ${FIRST_NAME}... "

    # Формируем Evidence
    EVIDENCE="{}"
    if [ -n "$INN" ] || [ -n "$SNILS" ] || [ -n "$DUL_TYPE" ]; then
        EVIDENCE="{"
        [ -n "$INN" ] && EVIDENCE="${EVIDENCE}\"inn\":\"${INN}\","
        [ -n "$SNILS" ] && EVIDENCE="${EVIDENCE}\"snils\":\"${SNILS}\","
        [ -n "$DUL_TYPE" ] && EVIDENCE="${EVIDENCE}\"dulType\":\"${DUL_TYPE}\","
        [ -n "$DUL_SERIES" ] && EVIDENCE="${EVIDENCE}\"dulSeries\":\"${DUL_SERIES}\","
        [ -n "$DUL_NUMBER" ] && EVIDENCE="${EVIDENCE}\"dulNumber\":\"${DUL_NUMBER}\","
        EVIDENCE="${EVIDENCE%,}}"
    fi

    REQUEST_BODY=$(cat <<EOF
{
    "lastName": "${LAST_NAME}",
    "firstName": "${FIRST_NAME}",
    "middleName": "${MIDDLE_NAME}",
    "evidence": ${EVIDENCE},
    "sourceSystemId": "fiducia",
    "externalPersonId": "test-${LOGIN}"
}
EOF
)

    RESPONSE=$(curl -s "${EDIN_URL}/persons/resolve" \
        -X POST \
        -H "Content-Type: application/json" \
        -d "$REQUEST_BODY" 2>/dev/null) || {
        echo "ОШИБКА HTTP"
        FAIL=$((FAIL + 1))
        continue
    }

    STATUS=$(echo "$RESPONSE" | jq -r '.status // "null"')
    MASTER_ID=$(echo "$RESPONSE" | jq -r '.masterId // "null"')

    if [ "$MASTER_ID" = "null" ] || [ -z "$MASTER_ID" ]; then
        echo "СТАТУС=${STATUS} (MasterId не получен)"
        FAIL=$((FAIL + 1))
        continue
    fi

    echo "MasterId=${MASTER_ID}"

    # Записываем в LDIF — добавляем objectClass extensibleObject + mpiMasterId
    cat >> "$OUTPUT_LDIF" <<EOF
# ${LOGIN}: ${LAST_NAME} ${FIRST_NAME} ${MIDDLE_NAME}
dn: ${LDAP_DN}
changetype: modify
add: objectClass
objectClass: extensibleObject
-
add: mpiMasterId
mpiMasterId: ${MASTER_ID}

EOF

    SUCCESS=$((SUCCESS + 1))
done

echo ""
echo "Результат: ${SUCCESS} успешно, ${FAIL} ошибок"
echo "LDIF-файл: ${OUTPUT_LDIF}"

if [ "$SUCCESS" -eq 0 ]; then
    echo "ОШИБКА: ни один MasterId не получен" >&2
    exit 1
fi

# ── Импорт LDIF в LDAP ────────────────────────────────────────────────────
echo ""
echo "Импорт LDIF в LDAP (${LDAP_HOST})..."
ldapadd -x -H "ldap://${LDAP_HOST}" \
    -D "${LDAP_BIND_DN}" \
    -w "${LDAP_PASSWORD}" \
    -f "${OUTPUT_LDIF}" || {
    echo "ПРЕДУПРЕЖДЕНИЕ: некоторые записи могли не импортироваться (повторный запуск OK)"
}

echo ""
echo "Проверка: ldapsearch mpiMasterId=*"
ldapsearch -x -H "ldap://${LDAP_HOST}" \
    -b "${LDAP_BASE}" \
    -D "${LDAP_BIND_DN}" \
    -w "${LDAP_PASSWORD}" \
    "(mpiMasterId=*)" \
    mpiMasterId dn 2>/dev/null || echo "(ldapsearch недоступен — проверьте вручную)"

echo ""
echo "Seed MPI завершён."
