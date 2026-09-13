# E2E-тест: Изменение сведений участника (версионирование ДУЛ)

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

Генеральный директор (ГД) ООО является одновременно:
- **Администратором ЮЛ** (роль `LE_ADMIN`) — управляет настройками, добавляет участников
- **Генеральным директором** (роль `CEO`) — подписывает документы, принимает решения
- **Участником общества** (BoardParticipant с долей) — имеет право голоса

ГД регистрирует нового участника общества через Board Portal с данными документа, удостоверяющего личность (ДУЛ).
Участник заходит сам в Board Portal и подаёт новую запись сведений (новый паспорт).
Новая запись автоматически применяется: старая версия ДУЛ деактивируется, создаётся новая активная версия.

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 0 | Подготовка окружения: LDAP (с MasterId) → Admin Console (роли) | Автоматически |
| 1 | Логин в Board Portal | ГД (`kazakov.nv`) |
| 2 | Поиск EcosystemParticipant участника | ГД (`kazakov.nv`) |
| 3 | Регистрация участника общества с ДУЛ через Board Portal (`POST /api/participants`) | ГД (`kazakov.nv`) |
| 4 | Проверка: ДУЛ создан и активен | Автоматическая (assert) |
| 5 | Логин участника в Board Portal | Участник (`frolov.sa68`) |
| 6 | Подача информирования об изменении сведений (новый паспорт) | Участник (`frolov.sa68`) |
| 7 | Проверка: старый ДУЛ деактивирован, новый активен | Автоматическая (assert) |

---

## Действующие лица

| Роль в системе | Логин | Как создаётся | Место в тесте |
|----------------|-------|---------------|---------------|
| ГД + Администратор ЮЛ + Участник общества | `kazakov.nv` | LDAP (uid, password, MasterId). Регистрация как BoardParticipant — через Board Portal | Шаг 1–4 |
| Участник общества | `frolov.sa68` | LDAP (uid, password, MasterId). Регистрация как BoardParticipant — через Board Portal (ГД) | Шаг 5–7 |

### Как создаются пользователи

1. **LDAP** (OpenLDAP) — создаёт учётные записи пользователей (uid, password, MasterId)
2. **Admin Console** — назначает роль LE_ADMIN (`kazakov.nv`), связывает EcosystemParticipant с ЮЛ
3. **Board Portal** — ГД регистрирует BoardParticipant (доля, ДУЛ, Person) с привязкой к EcosystemParticipant. Роль CEO назначается при привязке BoardParticipant

### Назначение ролей (ЮЛ №68, ОКОПФ 12300)

- `kazakov.nv` → `LE_ADMIN` (назначается через Admin Console) + `CEO` (назначается при привязке BoardParticipant через Board Portal)
- `frolov.sa68` — роль назначается при регистрации BoardParticipant через Board Portal

---

## Предусловия

1. Инфраструктура запущена: PostgreSQL, OpenLDAP, Admin Console (порт 5001), Board Portal (порт 5002)
2. БД сброшена: `01_schema.sql` + `02_seed.sql` (справочники включая `ref_dul_type` с кодом `21`)
3. LDAP-пользователи удалены и пересозданы (`CharterTestGlobalInit.InitializeAsync`)
4. ЮЛ №68 создано: наименование, ИНН, ОКОПФ=12300 (ООО)
5. Роли назначены: `kazakov.nv` → `LE_ADMIN`, `frolov.sa68` → `PARTICIPANT`

---

## Шаги теста

### Шаг 0: Подготовка окружения

**LDAP** создаёт учётные записи `kazakov.nv` и `frolov.sa68`.
**ЕДИН** привязывает MasterId.
**Admin Console** назначает роли: `kazakov.nv` → `LE_ADMIN`, `frolov.sa68` → `PARTICIPANT`.

### Шаг 1: Логин ГД

**Исполнитель:** `kazakov.nv`

**Действие:**
```
AuthHelper.LoginAsBoardUserAsync(boardPage, "kazakov.nv")
```

**Проверка:** `boardPage.Url.Should().Contain("/main")`

### Шаг 2: Поиск EcosystemParticipant участника

**Исполнитель:** `kazakov.nv`

**Действие:** GD ищет EcosystemParticipant `frolov.sa68` по ФИО:
```
GET /api/participants/eco-search?name=Фролов Станислав Андреевич
```

**Ожидаемый результат:** массив с объектом `{ id: "...", login: "frolov.sa68", fullName: "Фролов Станислав Андреевич" }`

### Шаг 3: Регистрация участника через Board Portal

**Исполнитель:** `kazakov.nv`

**Действие:**
```
BoardPortalHelper.AddParticipantWithDulAsync(boardPage,
    fullName: "Фролов Станислав Андреевич",
    dulTypeCode: "21",
    dulSeries: "4600",
    dulNumber: "111222",
    personInn: "781234567890",
    sharePercent: 60,
    ecosystemParticipantId: <id из шага 2>)
```

Вызывает `POST /api/participants` через `fetch()` в браузере.

**Тело запроса (JSON):**
```json
{
  "participantType": "FL",
  "lastName": "Фролов",
  "firstName": "Станислав",
  "middleName": "Андреевич",
  "dulTypeCode": "21",
  "dulSeries": "4600",
  "dulNumber": "111222",
  "personInn": "781234567890",
  "sharePercent": 60,
  "ecosystemParticipantId": "..."
}
```

**Что происходит на сервере:**
1. `ValidateAccessAsync` — проверка: пользователь привязан к ЮЛ (ООО)
2. Создание `Person` (Фролов Станислав Андреевич)
3. Создание `IdentityDocument` (dul_type_id=21, series=4600, number=111222, is_active=true)
4. Привязка к `EcosystemParticipant` (ecosystemParticipantId из запроса)
5. Создание `BoardParticipant` (share_percent=60)
6. `SaveChangesAsync()` — все записи сохраняются в БД
7. Возврат `participantId`

**Ожидаемый результат:**
- `participantId` — не пустой Guid
- В БД: `board_participant` (participant_type=FL, person_id заполнен, ecosystem_participant_id = frolov.sa68)
- В БД: `person` (Фролов Станислав Андреевич)
- В БД: `identity_documents` (dul_type_id=21, series=4600, number=111222, is_active=true)

### Шаг 4: Проверка начального состояния

**Действия:**
- `GET /api/participants/{id}` — проверить DulSeries=4600, DulNumber=111222
- `GET /api/participants/{id}/identity-documents` — проверить 1 запись, isActive=true

**Ожидаемый JSON:**
```json
[{"id":"...","dulTypeCode":"21","series":"4600","number":"111222","isActive":true}]
```

### Шаг 5: Логин участника

**Исполнитель:** `frolov.sa68`

**Действие:**
```
AuthHelper.LoginAsBoardUserAsync(boardPage, "frolov.sa68")
```

**Проверка:** `boardPage.Url.Should().Contain("/main")`

### Шаг 6: Участник подаёт новую запись сведений

**Исполнитель:** `frolov.sa68`

**Действие:**
```
BoardPortalHelper.SubmitParticipantChangeAsync(boardPage,
    participantId: participantId,
    dulTypeCode: "21",
    passportSeries: "4610",
    passportNumber: "333444",
    comment: "Замена паспорта")
```

Вызывает `POST /api/participant-changes` через `fetch()` в браузере.

**Что происходит на сервере:**
1. Создание `board_participant_change` (status=pending)
2. Источник="electronic" → `ApplyParticipantChangeAsync`:
   - Нахождение текущего активного `identity_documents` (is_active=true)
   - **Сравнение:** серия 4600→4610, номер 111222→333444
   - Старый `identity_documents` → `is_active=false`
   - Создание нового `identity_documents` (series=4610, number=333444, is_active=true)
   - `board_participant_change.status` → "approved"
3. `SaveChangesAsync()`

**Ожидаемый результат:**
- `changeId` — не пустой Guid
- В БД: 2 записи `identity_documents` для данного `person_id`

### Шаг 7: Проверка версионирования

**Действие:** `GET /api/participants/{id}/identity-documents`

**Ожидаемый JSON (2 записи):**
```json
[
  {"series":"4610","number":"333444","isActive":true},
  {"series":"4600","number":"111222","isActive":false}
]
```

**Проверки:**
- Ровно 2 записи
- Старая версия: `is_active=false`, `series=4600`, `number=111222`
- Новая версия: `is_active=true`, `series=4610`, `number=333444`
- `GET /api/participants/{id}` показывает `DulSeries=4610`, `DulNumber=333444`

---

## Известные проблемы

### 1. Десериализация JSON (ИСПРАВЛЕНО)

API возвращает camelCase (`id`, `series`, `isActive`), C# record ожидает PascalCase. Решение: `PropertyNameCaseInsensitive = true`.

### 2. Конфликт логинов LDAP

При повторном прогоне логин `frolov.sa68` уже существует в LDAP от предыдущего прогона. LDAP-очистка между прогонами работает некорректно.

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../E2E_ParticipantDulChangeTests.cs` | E2E-тест |
| `tests/.../Helpers/CharterTestDataFixed.cs` | Тестовые данные (ЮЛ №68) |
| `tests/.../Helpers/CharterTestSeeder.cs` | Сидирование: создание ЮЛ + назначение ролей |
| `tests/.../Helpers/BoardPortalHelper.cs` | Хелперы API-вызовов |
| `src/BoardPortal/Endpoints/ParticipantEndpoints.cs` | POST `/api/participants`, GET `/{id}/identity-documents`, POST `/api/participant-changes` |
