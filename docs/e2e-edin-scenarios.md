# E2E-тест: Сценарии ЕДИН-интеграции

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

Два сквозных сценария ЕДИН-интеграции: (1) полный цикл создания ЮЛ → привязка ЕДИН → роль CEO, (2) дедупликация через ЕДИН — одинаковые ПДн дают тот же MasterId.

---

## Сценарий 1: Создание ЮЛ и привязка ЕДИН

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 1 | Логин в Admin Console | SYS_ADMIN (`v.vasilyeva`) |
| 2 | Создание ЮЛ (наименование, ИНН, ОКОПФ=12300) | SYS_ADMIN |
| 3 | Назначение LE_ADMIN (`nechaev.va`) | SYS_ADMIN |
| 4 | Логин LE_ADMIN в Board Portal | LE_ADMIN |
| 5 | Регистрация участника с ПДн (паспорт, ИНН, доля 100%) | LE_ADMIN |
| 6 | Ожидание ЕДИН-привязки → MasterId | Автоматически |

### Шаги

```
// Step 1-3: Admin Console
AuthHelper.LoginAsAdminAsync(adminPage, "v.vasilyeva")
AdminConsoleHelper.CreateLegalEntityAsync(adminPage, name, inn)
AdminConsoleHelper.SetOkopfAsync(adminPage, "12300")
AdminConsoleHelper.AddEmployeeAsync(adminPage, "nechaev.va", "LE_ADMIN")

// Step 4-5: Board Portal
AuthHelper.LoginAsBoardUserAsync(boardPage, "nechaev.va")
BoardPortalHelper.AddParticipantWithPersonalDataAsync(boardPage,
    dulTypeCode: "21", dulSeries: "4515", dulNumber: "111222",
    personInn: "770888999000", sharePercent: 100, ...)

// Step 6: ЕДИН binding
EdinTestHelper.WaitForEdinBindingAsync(boardPage, participantId, 15)
var masterId = await EdinTestHelper.GetParticipantMpiMasterIdAsync(boardPage, participantId)
masterId.Should().NotBeNullOrWhiteSpace()
```

---

## Сценарий 2: Дедупликация через ЕДИН

### Бизнес-условие

ГД вводит участника с теми же ПДн (паспорт, ИНН). ЕДИН возвращает тот же MasterId → привязка к существующей УЗ → роль PARTICIPANT назначается автоматически.

### Дополнительные шаги (после шагов 1–6 из сценария 1)

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 7 | Регистрация второго участника с теми же ПДн | LE_ADMIN (`sobolev.dn`) |
| 8 | Проверка: masterId второго = masterId первого | Автоматическая (assert) |

### Шаги

```
// Step 7: Второй участник с теми же ПДн
BoardPortalHelper.AddParticipantWithPersonalDataAsync(boardPage,
    dulTypeCode: "21", dulSeries: "4516", dulNumber: "222333",
    personInn: "770999111000", sharePercent: 100, ...)

// Step 8: Проверка дедупликации
var masterId2 = await EdinTestHelper.GetParticipantMpiMasterIdAsync(boardPage, participantId2)
masterId2.Should().Be(masterId1)  // одинаковые ПДн → одинаковый MasterId
```

---

## Действующие лица

| Роль в системе | Логин | Сценарий |
|----------------|-------|----------|
| SYS_ADMIN | `v.vasilyeva` | 1, 2 |
| LE_ADMIN | `nechaev.va` | 1 |
| LE_ADMIN | `sobolev.dn` | 2 |

---

## Известные проблемы

### 1. mnemonios: отсутствие библиотеки `libgssapi_krb5.so.2`

**Симптом:** `ЕДИН binding не завершился за 15 сек` — тест падает, хотя контейнер `fiducia-mnemonios` запущен.

**Корневая причина:** Базовый образ `mcr.microsoft.com/dotnet/aspnet:10.0` не содержит библиотеку Kerberos `libgssapi_krb5.so.2`, которая нужна для LDAP/ЕДИН-интеграции. Переменная `LD_LIBRARY_PATH=/usr/lib/aarch64-linux-gnu` в `docker-compose.yml` указывает путь, но самой библиотеки в образе нет.

**Ошибка в логах mnemonios:**
```
Cannot load library libgssapi_krb5.so.2
Error: libgssapi_krb5.so.2: cannot open shared object file: No such file or directory
```

**Исправление:** Добавить установку библиотеки в Dockerfile mnemonios:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
```

**После исправления:** пересобрать образ:
```bash
docker-compose up -d --build mnemonios
```

### 2. Board Portal: старый код при перезапуске тестов

**Симптом:** Тест падает с ошибкой валидации или неожиданным поведением, хотя код исправлен.

**Корневая причина:** `InfrastructureHelper` запускает Board Portal **только если порт 5002 свободен**. Если Board Portal уже запущен от предыдущего прогона, он продолжает работать со **старым кодом**. Новые изменения не подхватываются.

**Решение:** Перед запуском E2E-тестов **принудительно остановить** Board Portal и Admin Console:

```bash
pkill -f 'dotnet.*BoardPortal'
pkill -f 'dotnet.*AdminConsole'
sleep 2
```

После этого `InfrastructureHelper` запустит порталы с актуальным кодом.

**Правило:** При любом изменении кода BoardPortal/AdminConsole **всегда** убивать процессы перед запуском E2E-тестов.

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../E2E_EdinScenarioTests.cs` | E2E-тест (2 метода) |
| `tests/.../Helpers/EdinTestHelper.cs` | ЕДИН: WaitForEdinBinding, GetParticipantMpiMasterId |
| `tests/.../Helpers/AdminConsoleHelper.cs` | Хелперы: CreateLegalEntity, SetOkopf, AddEmployee |
| `tests/.../Helpers/BoardPortalHelper.cs` | Хелпер: AddParticipantWithPersonalData |
| `tools/mpi/seed-mpi.sh` | Утилита получения masterId через ЕДИН API по ПДн |
| `tools/mpi/test-persons.json` | Тестовые данные для seed-mpi.sh |

---

## Как работает получение masterId

### Динамическое получение через ЕДИН API

**masterId НЕ хранится в seed-скриптах** — он получается динамически при привязке участника.

**Поток:**
1. Участник создаётся через Board Portal (`POST /api/participants`)
2. `TriggerEdinBindingAsync` вызывается автоматически (fire-and-forget)
3. `EdinBindingService.ResolveAndBindAsync()` вызывает ЕДИН API (`POST /persons/resolve`)
4. ЕДИН возвращает `masterId` по ПДн (ФИО + ИНН/СНИЛС/ДУЛ)
5. `masterId` сохраняется в `User.MpiMasterId`

### Утилита seed-mpi.sh

Для получения masterId тестовых пользователей без создания участников в Board Portal:

```bash
# Запуск утилиты
./tools/mpi/seed-mpi.sh

# Утилита:
# 1. Читает тестовые данные из tools/mpi/test-persons.json
# 2. Для каждого пользователя вызывает POST /persons/resolve
# 3. Получает masterId из ответа ЕДИН
# 4. Генерирует LDIF и импортирует в LDAP
```

### Важные особенности

- **При перезапуске ЕДИН** все masterId теряются — это нормально
- **При повторном обновлении ПДн** ЕДИН может выдать другой masterId
- **Дедупликация:** одинаковые ПДн → одинаковый masterId (если ЕДИН работает корректно)
- **Системные пользователи** (`system`, `v.vasilyeva`) НЕ проходят через ЕДИН — у них нет masterId
