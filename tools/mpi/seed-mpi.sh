#!/bin/bash
# Seed MPI through ЕДИН API to trigger LDAP creation via webhooks.
# Usage: ./seed-mpi.sh [output_ldif_path]
#
# If output_ldif_path is provided, also writes generated LDIF for manual import.
# Environment: LDAP_HOST, LDAP_PORT, LDAP_BASE, LDAP_ADMIN, LDAP_PASSWORD

set -e

EDIN_API="http://localhost:5010/api"
LDAP_HOST="${LDAP_HOST:-localhost}"
LDAP_PORT="${LDAP_PORT:-389}"
LDAP_BASE="${LDAP_BASE:-dc=example,dc=com}"
LDAP_ADMIN="${LDAP_ADMIN:-cn=admin,dc=example,dc=com}"
LDAP_PASSWORD="${LDAP_PASSWORD:-admin}"

JSON_DIR="$(cd "$(dirname "$0")" && pwd)"
OUTPUT_LDIF="${1:-}"

echo "=== MPI Seed via ЕДИН ==="
echo "LDAP: ${LDAP_HOST}:${LDAP_PORT}"
echo "ЕДИН: ${EDIN_API}"
echo ""

# 1. Create identities in ЕДИН
echo "Step 1: Creating identities in ЕДИН..."
for f in "${JSON_DIR}"/person-*.json; do
    [ -f "$f" ] || continue
    name=$(basename "$f" .json | sed 's/^person-//')
    echo -n "  ${name}... "

    response=$(curl -s -w "\n%{http_code}" -X POST "${EDIN_API}/identities" \
        -H "Content-Type: application/json" \
        -d @"$f")

    http_code=$(echo "$response" | tail -1)
    body=$(echo "$response" | sed '$d')

    if [ "$http_code" = "201" ] || [ "$http_code" = "200" ]; then
        echo "OK"
    elif echo "$body" | grep -q "already exists"; then
        echo "exists"
    else
        echo "FAIL (${http_code}): ${body}"
    fi
done

# 2. Trigger resolve to create LDAP entries via webhook
echo ""
echo "Step 2: Triggering resolve (LDAP creation)..."
for f in "${JSON_DIR}"/person-*.json; do
    [ -f "$f" ] || continue
    name=$(basename "$f" .json | sed 's/^person-//')
    serial=$(python3 -c "import json; print(json.load(open('$f'))['identity']['documents'][0]['serial'])")
    number=$(python3 -c "import json; print(json.load(open('$f'))['identity']['documents'][0]['number'])")
    inn=$(python3 -c "import json; d=json.load(open('$f')); print(d['identity'].get('inn', ''))")

    echo -n "  ${name} (passport ${serial} ${number}, INN ${inn})... "

    response=$(curl -s -w "\n%{http_code}" -X POST "${EDIN_API}/resolve" \
        -H "Content-Type: application/json" \
        -d "{\"documentSerial\": \"${serial}\", \"documentNumber\": \"${number}\", \"inn\": \"${inn}\"}")

    http_code=$(echo "$response" | tail -1)
    body=$(echo "$response" | sed '$d')

    if [ "$http_code" = "200" ]; then
        master_id=$(echo "$body" | python3 -c "import sys,json; print(json.load(sys.stdin).get('masterId','?'))" 2>/dev/null || echo "?")
        echo "OK (masterId: ${master_id})"
    else
        echo "FAIL (${http_code}): ${body}"
    fi
done

# 3. Wait for LDAP sync
echo ""
echo "Step 3: Waiting for LDAP sync..."
sleep 3

# 4. Verify LDAP entries
echo "Step 4: Verifying LDAP entries..."
search_result=$(ldapsearch -x -H "ldap://${LDAP_HOST}:${LDAP_PORT}" \
    -D "${LDAP_ADMIN}" -w "${LDAP_PASSWORD}" \
    -b "${LDAP_BASE}" "(objectClass=*)" cn 2>/dev/null || echo "LDAP_SEARCH_FAILED")

if echo "$search_result" | grep -q "LDAP_SEARCH_FAILED"; then
    echo "  WARNING: LDAP search failed (ldapsearch may not be installed)"
elif echo "$search_result" | grep -q "mpi_master_id"; then
    echo "  OK: LDAP entries with mpi_master_id found"
else
    echo "  WARNING: No LDAP entries with mpi_master_id found"
fi

# 5. Export LDIF if requested
if [ -n "${OUTPUT_LDIF}" ]; then
    echo ""
    echo "Step 5: Exporting LDIF to ${OUTPUT_LDIF}..."
    ldapsearch -x -H "ldap://${LDAP_HOST}:${LDAP_PORT}" \
        -D "${LDAP_ADMIN}" -w "${LDAP_PASSWORD}" \
        -b "${LDAP_BASE}" "(mpi_master_id=*)" \
        dn cn givenName sn snils uid employeeType title \
        businessCategory company mobile telephoneNumber \
        userCertificate;binary \
        2>/dev/null | sed 's/^#.*LDIF Output//' | sed '/^$/N;/^\n$/d' > "${OUTPUT_LDIF}"
    echo "  Done: $(grep -c "^dn:" "${OUTPUT_LDIF}" 2>/dev/null || echo 0) entries exported"
fi

echo ""
echo "=== MPI Seed Complete ==="
