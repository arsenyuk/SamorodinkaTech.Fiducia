# E2E-тест: Индивидуальные уставы ООО и модели ЕИО

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

ООО может иметь индивидуальный устав с настраиваемыми параметрами. Администратор настраивает каждый параметр отдельно (13 параметров) и проверяет его сохранение. Также проверяются 7 моделей организации единоличного исполнительного органа (ЕИО).

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 0 | Подготовка окружения | Автоматически |
| 1 | Логин в Board Portal | ГД |
| 2 | Заполнение полей ЮЛ + выбор индивидуального устава | ГД |
| 3 | Настройка параметра устава / выбор модели ЕИО | ГД |
| 4 | Добавление участников общества | ГД |
| 5 | Сохранение | ГД |
| 6 | Проверка страниц и аудит-лога | Автоматическая (assert) |

---

## Действующие лица

| Роль в системе | Логин | Место в тесте |
|----------------|-------|---------------|
| ГД + Администратор ЮЛ | Из `CharterTestDataFixed` | Шаг 1–5 |

---

## Предусловия

1. Инфраструктура запущена
2. БД сброшена
3. LDAP-пользователи созданы
4. ЮЛ сидировано (entityIndex 37–57)

---

## Тесты параметров устава (entityIndex 37–50)

| # | Метод | Параметр | Значение |
|---|-------|----------|----------|
| 1 | `CustomCharter_ExitAllowed` | exit-allowed | `true` |
| 2 | `CustomCharter_ExitMinSharePercent` | exit-min-share | `5` |
| 3 | `CustomCharter_ExitMaxSharePercent` | exit-max-share | `40` |
| 4 | `CustomCharter_ExitConditionDescription` | exit-condition | `по истечении 2 лет с момента вступления` |
| 5 | `CustomCharter_ExitRequiresUnanimousOsu` | exit-unanimous | `true` |
| 6 | `CustomCharter_TransferToParticipants` | transfer-participants | `true` |
| 7 | `CustomCharter_TransferToThirdParties` | transfer-third-parties | `CONSENT` |
| 8 | `CustomCharter_PreemptiveRight` | preemptive-right | `true` |
| 9 | `CustomCharter_InheritanceWithoutConsent` | inheritance | `true` |
| 10 | `CustomCharter_ExecutiveBody` | executive-body | `A` |
| 11 | `CustomCharter_HasBoardOfDirectors` | has-board | `true` (+ проверка вкладки СД) |
| 12 | `CustomCharter_BoardDecidesConveningOsu` | board-convenes-osu | `true` |
| 13 | `CustomCharter_VosuThresholdPercent` | vosu-threshold | `5` |
| 14 | `CustomCharter_AllParameters` | все 13 параметров | комбинация |

---

## Тесты моделей ЕИО (entityIndex 51–57)

| # | Метод | Тип ЕИО | Описание | Юридическое основание |
|---|-------|:---:|----------|----------------------|
| 1 | `Model1_HiredCeo` | A | ГД — наёмный сотрудник, не участник | ст. 40 14-ФЗ |
| 2 | `Model2_CeoParticipant` | A | ГД — участник общества | ст. 40 14-ФЗ |
| 3 | `Model3_ManagerIp` | D | Управляющий — ИП | ст. 42 14-ФЗ |
| 4 | `Model4_ManagingOrg` | E | Управляющая организация — ЮЛ | ст. 42 14-ФЗ |
| 5 | `Model5_AllParticipantsDirectors` | B | Все участники — директора самостоятельно | п. 3 ст. 65.3 ГК РФ |
| 6 | `Model6_AllParticipantsJoint` | C | Все участники совместно | п. 3 ст. 65.3 ГК РФ |
| 7 | `Model7_MultipleEio` | F | Несколько ЕИО | п. 3 ст. 65.3 ГК РФ |

---

## Шаги теста (общий паттерн)

### Шаг 0: Подготовка

```
InfrastructureHelper.EnsureInfrastructureReadyAsync()
CharterTestGlobalInit.InitializeAsync(adminPage, ldapPage)
CharterTestSeeder.EnsureSeededAsync(adminPage, entityIndex)
AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin)
BoardPortalHelper.FillLegalEntityFieldsAsync(boardPage, entity)
```

### Шаг 1: Выбор индивидуального устава

```
BoardPortalHelper.SelectCustomCharterAsync(boardPage)
```

### Шаг 2: Настройка параметра

```
BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, parameterCode, value)
// или для моделей ЕИО:
BoardPortalHelper.SetExecutiveBodyAsync(boardPage, executiveBodyType)
```

### Шаг 3: Добавление участников

```
BoardPortalHelper.AddParticipantAsync(boardPage, ...)
BoardPortalHelper.AssertParticipantCountAsync(boardPage, expectedCount)
```

### Шаг 4: Сохранение и проверка

```
BoardPortalHelper.SaveAndVerifyAsync(boardPage)
PageVerificationHelper.VerifyBoardPortalPagesAsync(boardPage, testStartTime)
```

### Шаг 5: Проверка аудит-лога

```
AuditLogHelper.AssertLoginLoggedAsync(login)
AuditLogHelper.AssertDataUpdateLoggedAsync("legal-entities")
AuditLogHelper.AssertDataCreateLoggedAsync("participants")
AuditLogHelper.AssertNoNotFoundAsync(from: testStartTime)
```

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../E2E_CustomCharterTests.cs` | E2E-тест (21 метод) |
| `tests/.../Helpers/CharterTestDataFixed.cs` | Тестовые данные (entityIndex 37–57) |
| `tests/.../Helpers/CustomCharterTestData.cs` | Константы параметров устава |
| `tests/.../Helpers/BoardPortalHelper.cs` | Хелперы: SelectCustomCharter, ConfigureCharterParameter, SetExecutiveBody |
