# Валидации полей — Admin Console

Список всех валидаций полей на формах Admin Console (`http://localhost:5001`).

> **Связанные документы:** [E2E-тесты](../e2e-tests.md) | [Бизнес-сценарии](../e2e-scenarios.md)

---

## Страница входа (`/login`)

**Файл:** `SamorodinkaTech.Fiducia.AdminConsole/Pages/Login.razor`

| Поле | Тип | Правило | Сообщение | Тесты |
|------|-----|---------|-----------|-------|
| Пароль | Клиентская | `string.IsNullOrEmpty(_password)` | "Введите пароль" | [`US001_AuthorizationTests`](../e2e-tests.md#авторизация-и-безопасность) |
| Логин (Basic) | Клиентская | `string.IsNullOrEmpty(_username)` | "Введите логин или выберите пользователя" | [`US001_AuthorizationTests`](../e2e-tests.md#авторизация-и-безопасность) |
| Аутентификация | Серверная | `result.Success == false` | Ответ провайдера (`result.ErrorMessage`) | [`US001_AuthorizationTests`](../e2e-tests.md#авторизация-и-безопасность) |
| Роль | RBAC | `role != SYS_ADMIN && role != LE_ADMIN` | "Доступ только для системных администраторов и администраторов ЮЛ" | [`US001_AuthorizationTests`](../e2e-tests.md#авторизация-и-безопасность) |

---

## Юридическое лицо (LegalEntities — wizard)

**Валидатор:** `LegalEntityValidator.Validate()` (`src/Domain/Validation/LegalEntityValidator.cs`)

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| ФИО руководителя | Required (`IsNullOrWhiteSpace`) | "Укажите ФИО руководителя." | [`LegalEntityValidatorTests`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Должность руководителя | Required (`IsNullOrWhiteSpace`) | "Укажите должность руководителя." | [`LegalEntityValidatorTests`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Дата окончания окна ГОСА | >= даты начала | "Дата окончания окна ГОСА не может быть раньше даты начала." | [`LegalEntityValidatorTests::Gosa_EndBeforeStart`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Окно ГОСА (ПАО) | В диапазоне 01.03–30.06 | "Для ПАО окно ГОСА должно находиться в пределах 01.03–30.06." | [`LegalEntityValidatorTests::Gosa_PAO_*`](../e2e-tests.md#маппинг-us--e2e-класс), `E2E_StandardCharterTests`, `E2E_NonStandardCharterTests` |
| Окно ГОСА (НАО) | Фиксировано 01.03–30.06 | "Для НАО интервал ГОСА фиксирован: 01.03–30.06." | [`LegalEntityValidatorTests::Gosa_NAO_*`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Типовой устав | 2 цифры, 01–36 | "Номер типового устава должен быть от 01 до 36." | `E2E_StandardCharterTests` (36 тестов) |
| Типовой устав | Только для ООО | "Типовой устав применим только для ООО." | `E2E_StandardCharterTests` |
| Количество акционеров | Required, >0 (для не-ООО) | "Укажите количество акционеров (участников)." | [`OsaMeetingValidatorTests::Shareholders_*`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Количество акционеров | Не > 50 для НАО/ООО | "Для {тип} количество акционеров (участников) не может превышать 50." | [`OsaMeetingValidatorTests::Shareholders_*`](../e2e-tests.md#маппинг-us--e2e-класс) |

---

## ОСА — Совет директоров (OsuMeetingEdit — wizard)

**Валидатор:** `OsaMeetingValidator.Validate()` (`src/Domain/Validation/OsaMeetingValidator.cs`)

### Шаг 0 — Параметры

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Заочное голосование + ГОСА | Несовместимы | "Заочное голосование несовместимо с интервалом ГОСА." | [`OsaMeetingValidatorTests::Absentee_*`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Количество акционеров | Required, >0 | "Укажите количество акционеров." | [`OsaMeetingValidatorTests::Shareholders_*`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Количество акционеров | Не > 50 для НАО/ООО | "Для {тип} максимальное количество акционеров — 50. Указано: {N}." | [`OsaMeetingValidatorTests::Shareholders_*`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Количество директоров по типам | Сумма <= участников СД | "Общее количество директоров по типам ({total}) не может превышать количество участников СД ({max})." | [`OsaMeetingValidatorTests::DirectorTypes_*`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Количество участников СД | >= минимального | "Количество участников СД ({actual}) не может быть меньше минимального ({min})." | [`OsaMeetingValidatorTests::BoardMembers_*`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Год избрания | Диапазон 1990..текущий+5 | "Год избрания ({year}) вне допустимого диапазона." | [`OsaMeetingValidatorTests::ElectionYear_*`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Год избрания (DB) | Уникальность | "Состав СД за {year} год уже существует. Нельзя создать более одного состава в году." | [`OsaMeetingValidatorTests::UniqueElectionYear_*`](../e2e-tests.md#маппинг-us--e2e-класс) |

### Шаг 1 — Протокол

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Протокол подписан | `editProtocolSigned && HasValue` | "Протокол не подписан" | — (нет E2E для wizard) |
| Дата подписания | Не в будущем | "Дата подписания протокола не может быть в будущем" | — (нет E2E для wizard) |
| Дата подписания | >= referenceDate (завершение ОСА) | "Дата подписания не может быть раньше {date}" | — (нет E2E для wizard) |
| Дата подписания | <= referenceDate + 3 дня | "Дата подписания не может быть позднее {date} (3 дня после завершения ОСА)" | — (нет E2E для wizard) |

### Шаг 2 — Состав СД

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| ФИО членов СД | Required для каждого (кроме секретаря/врем. председ.) | (переход блокируется) | — (нет E2E для wizard) |
| Тип участника | Required если типы директоров настроены | `MemberTypeId` обязателен | — (нет E2E для wizard) |
| Дублирование ФИО | Нет дублей среди членов СД | (переход блокируется) | — (нет E2E для wizard) |
| Секретарь = STAFF | Секретарь обязан иметь тип STAFF; остальные — не могут | (переход блокируется) | — (нет E2E для wizard) |
| Врем. председательствующий | Не > 1; если GMS_DECISION — ФИО + тип обязательны | (переход блокируется) | — (нет E2E для wizard) |
| Количество строк | = editBoardMemberNumber и >= editBoardMinNumber | (переход блокируется) | — (нет E2E для wizard) |

### Шаг 3 — Файлы ГОСА

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Файлы | Если `editIsGosa && editHasBoard` — хотя бы один файл обязателен | (переход блокируется) | — (нет E2E для wizard) |

---

## Управление доступом (AccessManagement)

**Файл:** `SamorodinkaTech.Fiducia.AdminConsole/Pages/AccessManagement.razor`

### LDAP-поиск

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Логин | Required | "Введите логин." | [`E2E_UserManagementTests::AddEmployee_LdapNotFound`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Результат LDAP | Пользователь найден | "Пользователь «{login}» не найден в LDAP." | [`E2E_UserManagementTests::AddEmployee_LdapNotFound`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Дубль логина | DB: `EcosystemParticipants` | "Логин «{login}» уже используется в этом ЮЛ." | [`E2E_UserManagementTests`](../e2e-tests.md#маппинг-us--e2e-класс) |

### Добавление сотрудника

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Роль текущего пользователя | SYS_ADMIN или LE_ADMIN | "Недостаточно прав." | [`E2E_UserManagementTests`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Роль назначаемого | != SYS_ADMIN | "Нельзя «Системный администратор»." | — (нет теста) |
| Юридическое лицо | Required | "Выберите ЮЛ." | [`E2E_UserManagementTests`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Дубль логина (save) | DB: уникальность | "Логин «{login}» уже используется." | [`E2E_UserManagementTests`](../e2e-tests.md#маппинг-us--e2e-класс) |

### Создание ЮЛ

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Наименование | Required | "Введите наименование." | `E2E_StandardCharterTests`, `E2E_NonStandardCharterTests` |
| ИНН | Required | "Введите ИНН." | `E2E_StandardCharterTests`, `E2E_NonStandardCharterTests` |
| ИНН | 10 или 12 цифр | "ИНН: 10 или 12 цифр." | — (нет теста) |
| ИНН | DB: уникальность | "ИНН {inn} уже существует." | — (нет теста) |

### Внешнее лицо

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Юридическое лицо | Required | "Выберите ЮЛ." | — (нет теста) |
| Тип ЮЛ | Определён | "Тип ЮЛ не определён." | — (нет теста) |
| Дубль логина | DB: уникальность в ЮЛ | "Логин «{login}» уже используется в этом ЮЛ." | — (нет теста) |

---

## Списание члена СД (BoardMemberResign)

**Файл:** `SamorodinkaTech.Fiducia.AdminConsole/Pages/BoardMemberResign.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Файл РДЛ | Required если основание = "DISQUALIFICATION" | "Для основания «Дисквалификация» необходимо приложить выписку из РДЛ." | — (нет теста) |
| Правовое основание | Required если основание = "LEGAL" | "Для основания «Иные основания по ФЗ» необходимо указать статью и закон." | — (нет теста) |

---

## Настройки (Settings)

**Файл:** `SamorodinkaTech.Fiducia.AdminConsole/Pages/Settings.razor`

### Список запрещённых расширений (`ValidateExtensionList`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Только `[a-zA-Z0-9]` | "Недопустимое расширение «{ext}». Допустимы только латинские буквы и цифры без точки." | `US002_MeetingTests` (settings) |
| Без дублей | "Дубликат расширения «{ext}»." | `US002_MeetingTests` (settings) |
| Max 20 символов | "Расширение «{ext}» слишком длинное (максимум 20 символов)." | `US002_MeetingTests` (settings) |

### Числовые настройки (`ValidateNonNegativeInt`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| >= 0 | "Значение не может быть отрицательным." | `US002_MeetingTests` (settings) |

### Шаблон имени заседания (`ValidateTemplate`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Max 200 символов | "Шаблон слишком длинный (максимум 200 символов)." | `US002_MeetingTests` (settings) |
| `{PLACEHOLDER}` в верхнем регистре | "Плейсхолдер «{p}» должен быть в верхнем регистре: «{P}»." | `US002_MeetingTests` (settings) |
| Только известные: `{YYYY}`, `{YY}`, `{MM}`, `{DD}` | "Неизвестный плейсхолдер «{p}». Допустимы: {YYYY}, {YY}, {MM}, {DD}" | `US002_MeetingTests` (settings) |

---

## Шаблоны уведомлений (NotificationTemplateEdit)

**Файл:** `SamorodinkaTech.Fiducia.AdminConsole/Pages/NotificationTemplateEdit.razor`

Валидация полей отсутствует. Сохранение без проверки обязательных полей. Ошибки перехватываются через `ExceptionFlattener.Unwrap(ex)`. — (нет теста)

---

## Шаблоны (OrgTemplates)

**Файл:** `SamorodinkaTech.Fiducia.AdminConsole/Pages/OrgTemplates.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Наименование | Required (`IsNullOrWhiteSpace`) | (save блокируется молча) | `US004_CommitteeTests` (org-templates) |

---

## Этапы (OrgStages)

**Файл:** `SamorodinkaTech.Fiducia.AdminConsole/Pages/OrgStages.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Наименование этапа | Required (`IsNullOrWhiteSpace`) | (save блокируется молча) | `US004_CommitteeTests` (org-templates) |
| Наименование вехи | Required (`IsNullOrWhiteSpace`) | (save блокируется молча) | `US004_CommitteeTests` (org-templates) |

---

## Задачи (OrgOffers)

**Файл:** `SamorodinkaTech.Fiducia.AdminConsole/Pages/OrgOffers.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Наименование | Required (`IsNullOrWhiteSpace`) | (save блокируется молча) | `US004_CommitteeTests` (org-templates) |

---

## Серверные валидации — Program.cs (API endpoints)

**Файл:** `SamorodinkaTech.Fiducia.AdminConsole/Program.cs`

| Endpoint | Поле | Правило | Сообщение | Тесты |
|----------|------|---------|-----------|-------|
| `POST /api/session/login` | Token | Required | "Token is required" | [`US001_AuthorizationTests`](../e2e-tests.md#авторизация-и-безопасность) |
| `POST /api/legal-entities/{id}/gosa-window` | Даты ГОСА | Валидность для ОКОПФ | "Недопустимый интервал ГОСА для данной ОПФ" | `E2E_StandardCharterTests`, `E2E_NonStandardCharterTests` |
| `POST /api/board/members` | HasBoardOfDirectors | true | "Для данного юрлица Совет директоров отключён" | `E2E_NonStandardCharterTests::NonStandardCharter_HasBoardOfDirectors` |
| `POST /api/files/upload` | Request | Not null | "Invalid request" | — (нет теста) |
| `POST /api/files/upload/chunk` | Параметры | uploadId, chunkIndex, file | "Missing parameters" / "Invalid chunkIndex" | — (нет теста) |
| `POST /api/files/upload/complete` | Request | Not null | "Invalid request" | — (нет теста) |
| `PUT /api/legal-entities/{id}/okopf` | okopfCode | Найден в ref_okopf | "ОКОПФ с кодом {code} не найден" | — (нет теста) |

---

## Общие компоненты (SharedComponents)

### FileUpload (`src/SharedComponents/Components/FileUpload.razor`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Размер файла > MaxSizeBytes | "Файл слишком большой ({size}). Максимум: {max}." | — (нет теста) |
| Запрещённое расширение | "Загрузка файлов с расширением .{ext} запрещена." | — (нет теста) |
| HTTP-ошибка | "Ошибка загрузки: {message}" | — (нет теста) |

### QrScanButton (`src/SharedComponents/Components/QrScanButton.razor`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Размер > 50 МБ | "Файл слишком большой. Максимум: 50 МБ." | — (нет теста) |
| Недопустимое расширение | "Недопустимое расширение файла: {ext}. Допустимые: {list}" | — (нет теста) |
| HTTP-ошибка | "Ошибка считывания QR-кода ({status})." | — (нет теста) |

---

## Domain-валидаторы (общие)

Используются обоими порталами.

### LegalEntityValidator (`src/Domain/Validation/LegalEntityValidator.cs`)

| Константа | Значение | Назначение |
|-----------|----------|------------|
| `MaxShareholdersForNonPao` | 50 | Макс. акционеров для НАО/ООО |
| `MinElectionYear` | 1990 | Мин. год избрания |
| `MaxElectionYearOffset` | 5 | Макс. смещение года вперёд |
| `MaxStandardCharterNumber` | 36 | Макс. номер типового устава |

Unit-тесты: [`LegalEntityValidatorTests`](../e2e-tests.md#маппинг-us--e2e-класс) (15 тестов)

### OsaMeetingValidator (`src/Domain/Validation/OsaMeetingValidator.cs`)

Валидации ОСА — см. раздел «ОСА — Совет директоров» выше.

Unit-тесты: [`OsaMeetingValidatorTests`](../e2e-tests.md#маппинг-us--e2e-класс) (37 тестов)

### InnIpValidator (`src/Domain/Validation/InnIpValidator.cs`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Required | "ИНН ИП обязателен" | `US024_ContractTests` |
| 12 цифр | "ИНН ИП должен содержать ровно 12 цифр" | `US024_ContractTests` |
| Контрольная сумма (10-я цифра) | "Неверная контрольная сумма ИНН (10-я цифра)" | `US024_ContractTests` |
| Контрольная сумма (12-я цифра) | "Неверная контрольная сумма ИНН (12-я цифра)" | `US024_ContractTests` |

### OgrnipValidator (`src/Domain/Validation/OgrnipValidator.cs`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Optional (null = валидно) | — | `US024_ContractTests` |
| 15 цифр | "ОГРНИП должен содержать ровно 15 цифр" | `US024_ContractTests` |
| Контрольная сумма | "Неверная контрольная сумма ОГРНИП" | `US024_ContractTests` |

---

## Источники файлов

- `SamorodinkaTech.Fiducia.AdminConsole/Pages/Login.razor`
- `SamorodinkaTech.Fiducia.AdminConsole/Pages/AccessManagement.razor`
- `SamorodinkaTech.Fiducia.AdminConsole/Pages/BoardMemberResign.razor`
- `SamorodinkaTech.Fiducia.AdminConsole/Pages/Settings.razor`
- `SamorodinkaTech.Fiducia.AdminConsole/Pages/NotificationTemplateEdit.razor`
- `SamorodinkaTech.Fiducia.AdminConsole/Pages/OrgTemplates.razor`
- `SamorodinkaTech.Fiducia.AdminConsole/Pages/OrgStages.razor`
- `SamorodinkaTech.Fiducia.AdminConsole/Pages/OrgOffers.razor`
- `SamorodinkaTech.Fiducia.AdminConsole/Program.cs`
- `src/Domain/Validation/LegalEntityValidator.cs`
- `src/Domain/Validation/OsaMeetingValidator.cs`
- `src/Domain/Validation/InnIpValidator.cs`
- `src/Domain/Validation/OgrnipValidator.cs`
- `src/Domain/Validation/OkopfTypeMapper.cs`
- `src/SharedComponents/Components/FileUpload.razor`
- `src/SharedComponents/Components/QrScanButton.razor`
