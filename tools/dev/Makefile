SHELL := /bin/bash

PG := docker-compose exec -T postgres psql -U fiducia -d fiducia -v ON_ERROR_STOP=1 -f /dev/stdin

.PHONY: db-init db-recreate e2e-start e2e-stop e2e-test

db-init:
	@echo "Applying schema..." && $(PG) < tools/db/01_schema.sql
	@echo "Seeding base data..." && $(PG) < tools/db/02_seed.sql
	@echo "Loading demo data..." && $(PG) < tools/db/03_demo.sql

db-recreate:
	docker-compose down -v
	docker-compose up -d postgres
	$(MAKE) db-init

e2e-start:
	@dotnet run --project SamorodinkaTech.Fiducia.BoardPortal --urls "http://localhost:5000" & echo $$! > /tmp/bp.pid &
	@dotnet run --project SamorodinkaTech.Fiducia.AdminConsole --urls "http://localhost:5001" & echo $$! > /tmp/ap.pid &
	@sleep 5
	@curl -sI http://localhost:5000 | head -n1 || true
	@curl -sI http://localhost:5001 | head -n1 || true

e2e-stop:
	@kill `cat /tmp/bp.pid 2>/dev/null` 2>/dev/null || true
	@kill `cat /tmp/ap.pid 2>/dev/null` 2>/dev/null || true
	@rm -f /tmp/bp.pid /tmp/ap.pid

e2e-test: e2e-start
	@dotnet test tests/SamorodinkaTech.Fiducia.Tests.Functional || true
	@$(MAKE) e2e-stop
