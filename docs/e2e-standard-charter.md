# E2E-тест: Типовые уставы ООО (36 вариантов)

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

ООО может работать по одному из 36 типовых уставов, утверждённых Приказом Минэкономразвития № 411. Каждый устав определяет набор параметров: выход участника, переход доли, преимущественное право, наследование, подтверждение протоколов ОСУ и интервал проведения ООСУ.

Администратор (ГД) создаёт ЮЛ, выбирает типовой устав, проверяет его параметры в режиме «только чтение». Для уставов с ExecutiveBody=A (ГД — отдельное лицо) дополнительно добавляются участники общества.

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 0 | Подготовка окружения: DB reset → LDAP → сидирование ЮЛ + роли | Автоматически |
| 1 | Логин в Board Portal | ГД (логин из CharterTestDataFixed) |
| 2 | Заполнение полей ЮЛ + выбор ОКОПФ 12300 | ГД |
| 3 | Выбор типового устава (номер 01–36) | ГД |
| 4 | Проверка 7 параметров устава (только чтение) | Автоматическая (assert) |
| 5 | Проверка интервала ООСУ | Автоматическая (assert) |
| 6 | (для ExecBodyA) Добавление участников общества | ГД |
| 7 | Проверка страниц Board Portal | Автоматическая (assert) |
| 8 | Проверка аудит-лога | Автоматическая (assert) |

---

## Действующие лица

| Роль в системе | Логин | Как создаётся | Место в тесте |
|----------------|-------|---------------|---------------|
| ГД + Администратор ЮЛ | Из `CharterTestDataFixed.PersonsByEntity[entityIndex]` | LDAP + сидирование через Admin Console | Шаг 1–6 |

### Как создаются пользователи

1. **LDAP** (OpenLDAP) — учётные записи (uid, password, MasterId)
2. **Admin Console** — назначение роли LE_ADMIN, связывание EcosystemParticipant с ЮЛ
3. **Board Portal** — ГД регистрирует BoardParticipant (для ExecutiveBodyA)

### Распределение ExecutiveBody по уставам

| Уставы | ExecutiveBody | Добавление участников |
|--------|:---:|:---:|
| 01–06, 19–24 | A (ГД отдельно) | Да |
| 07–12, 25–30, 13–18, 31–36 | B/C (участники — директора) | Нет |

---

## Предусловия

1. Инфраструктура запущена: PostgreSQL, OpenLDAP, Admin Console (порт 5001), Board Portal (порт 5002)
2. БД сброшена: `01_schema.sql` + `02_seed.sql`
3. LDAP-пользователи созданы (`CharterTestGlobalInit.InitializeAsync`)
4. ЮЛ создано и сидировано (`CharterTestSeeder.EnsureSeededAsync`)

---

## Шаги теста

### Шаг 0: Подготовка окружения

```
InfrastructureHelper.EnsureInfrastructureReadyAsync()
CharterTestGlobalInit.InitializeAsync(adminPage, ldapPage)  // DB reset + LDAP
CharterTestSeeder.EnsureSeededAsync(adminPage, charterNumber)  // ЮЛ + роли
```

### Шаг 1: Логин ГД

**Исполнитель:** ГД (логин из тестовых данных)

**Действие:**
```
AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin)
```

**Проверка:** `boardPage.Url.Should().Contain("/main")`

### Шаг 2–3: Заполнение ЮЛ + выбор устава

**Действие:**
```
BoardPortalHelper.CompleteLegalEntitySetupAsync(boardPage, entity, charterNumber)
```

Заполняет наименование, ИНН, ОГРН, выбирает ОКОПФ=12300, типовой устав №XX.

### Шаг 4: Проверка параметров устава

**Действие:** Навигация на вкладку «Устав»

**Проверки:**
- Заголовок: «Типовой устав № XX»
- 7 параметров устава отображаются в режиме «только чтение»

### Шаг 5: Проверка интервала ООСУ

**Проверка:** Вкладка «Интервал ООСУ» содержит selects для дня и месяца

### Шаг 6: Добавление участников (только для ExecutiveBodyA)

**Действие:**
```
BoardPortalHelper.AddParticipantAsync(boardPage, ...)
BoardPortalHelper.AssertParticipantCountAsync(boardPage, expectedCount)
```

### Шаг 7: Проверка страниц

**Действие:**
```
PageVerificationHelper.VerifyBoardPortalPagesAsync(boardPage, testStartTime)
```

### Шаг 8: Проверка аудит-лога

**Проверки:**
- `AuditLogHelper.AssertLoginLoggedAsync(login)`
- `AuditLogHelper.AssertDataUpdateLoggedAsync("legal-entities")`
- (для ExecBodyA) `AuditLogHelper.AssertDataCreateLoggedAsync("participants")`
- `AuditLogHelper.AssertNoNotFoundAsync(from: testStartTime)`

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../E2E_StandardCharterTests.cs` | E2E-тест (36 методов) |
| `tests/.../Helpers/CharterTestDataFixed.cs` | Тестовые данные (entityIndex 1–36) |
| `tests/.../Helpers/CharterTestSeeder.cs` | Сидирование ЮЛ + ролей |
| `tests/.../Helpers/BoardPortalHelper.cs` | Хелперы: CompleteLegalEntitySetup, AddParticipant, AssertParticipantCount |
| `tests/.../Helpers/PageVerificationHelper.cs` | Проверка страниц Board Portal |
| `tests/.../Helpers/AuditLogHelper.cs` | Проверка аудит-лога |
