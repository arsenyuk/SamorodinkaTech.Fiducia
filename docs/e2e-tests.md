# Сквозные (E2E) тесты

## Обзор

Сквозные тесты проверяют пользовательские сценарии от начала до конца: открывают реальный браузер (Playwright + Chromium), переходят на страницы приложения и проверяют корректность UI.

**Стек:** xUnit + Playwright + FluentAssertions

**Расположение:** `tests/SamorodinkaTech.Fiducia.Tests.Functional/Tests/`

**Правило синхронизации (КРИТИЧНО):** `docs/user-stories.md` и E2E-тесты (`Tests.Functional/`) должны однозначно соответствовать друг другу. Номера US в именах тестовых классов (`US001_`, `US002_`, ...) совпадают с номерами US в `docs/user-stories.md`.

---

## Результаты последнего прогона

**Дата:** 2026-09-13
**Среда:** .NET 10 SDK (10.0.300) + Playwright Chromium (headless=false)
**Длительность:** 3 мин 43 сек
**Итого:** 99 тестов → **16 пройдено**, 83 сбой

### Корневая причина массовых сбоев (ИСПРАВЛЕНО 2026-09-13)

Все 83 сбоя вызваны **одной корневой ошибкой**: `CharterTestSeeder.SeedEntityAsync` пытался
назначить роль ГД (CEO) через Admin Console (`AddEmployeeAsync` → dropdown роли).
**ГД не назначается в Admin Console** — роль CEO привязывается при создании
`BoardParticipant` через Board Portal (флаг `is_general_director`).

Dropdown `<select class="form-select">` в модалке добавления сотрудника **не содержит**
роль CEO → таймаут 30 сек → `GlobalFixture.HasFailed` блокирует все последующие тесты.

**Фикс:** удалён блок назначения ГД из `CharterTestSeeder.SeedEntityAsync` (строки 92–117).
Сидер создаёт только Admin User (роль `RoleLeAdmin`). Ожидаемый результат следующего
прогона: все тесты, кроме `E2E_BoardSetupTests` (страница `/board-setup` не загружает wizard),
должны пройти.

### Пройденные тесты (16)

| Класс | Тест | Статус |
|-------|------|--------|
| `E2E_EdinIntegrationTests` | `UsersList_ShouldHaveEdinColumn` | ✅ |
| `E2E_EdinIntegrationTests` | `UserDetail_ShouldHaveEdinTab` | ✅ |
| `E2E_EdinIntegrationTests` | `EdinTab_ShouldShowMpiMasterIdOrNotLinked` | ✅ |
| `E2E_EdinScenarioTests` | `Scenario1_AdminCreatesLeAndBindsEdin` | ✅ |
| `E2E_EdinScenarioTests` | `Scenario2_DeduplicationViaEdin` | ✅ |
| `E2E_UserManagementTests` | `AddEmployee_LdapNotFound_ShowsWarningAndButtonDisabled` | ✅ |
| `E2E_UserManagementTests` | `AddEmployee_LdapFoundButNoRole_ButtonDisabled` | ✅ |
| `E2E_UserManagementTests` | `AddEmployee_LdapFoundAndRoleSelected_ButtonEnabled` | ✅ |
| `E2E_UserManagementTests` | `EmployeeList_ShouldShowFullNameAndLogin` | ✅ |
| `US001_AuthorizationTests` | `BoardPortal_LoginPage_ShowsSelectDropdown` | ✅ |
| `US001_AuthorizationTests` | `AdminConsoleLoginPage_LoadsBlazorShell` | ✅ |
| `US001_AuthorizationTests` | `BoardPortal_LoginPage_ShowsNoSidebar` | ✅ |
| `US001_AuthorizationTests` | `BoardPortal_PublicLanding_Present` | ✅ |
| `US001_AuthorizationTests` | `BoardPortal_OnboardingPage_Rendered` | ✅ |
| `US001_AuthorizationTests` | `BoardPortal_ProposalPage_RenderedForAnonymousUsers` | ✅ |
| `Helpers.LoginTest` | `TestLoginAdminConsole` | ✅ |

### Сбойные тесты (83)

| Класс | Кол-во тестов | Причина |
|-------|---------------|---------|
| `E2E_StandardCharterTests` | 36 | CharterTestSeeder timeout |
| `E2E_NonStandardCharterTests` | 20 | CharterTestSeeder timeout |
| `E2E_BoardSetupTests` | 3 | CharterTestSeeder timeout |
| `E2E_GeneralDirectorTests` | 6 | CharterTestSeeder timeout |
| `E2E_VosuDemandTests` | 1 | CharterTestSeeder timeout |
| `E2E_ParticipantDulChangeTests` | 1 | CharterTestSeeder timeout |
| `US020_ShareRequestTests` | 5 | CharterTestSeeder timeout |
| `US021_DocumentCatalogTests` | 5 | CharterTestSeeder timeout |
| `US023_ParticipantTests` | 5 | CharterTestSeeder timeout |
| `US023_ParticipantTests` (последний US023 — `ShouldLoadWithExpectedContent`) | 1 | CharterTestSeeder timeout |
| **Итого** | **83** | |

### Неисправимые сбои (предыдущие прогоны)

- `E2E_BoardSetupTests` (3 варианта) — страница `/board-setup` не загружает wizard (ADMIN-88).
  **Не зависит** от CharterTestSeeder — требует отдельного.fix.
- `E2E_ParticipantDulChangeTests` — падал из-за CharterTestSeeder. **Ожидается прохождение** после фикса сидера.

---

## Маппинг: Бизнес-процесс → US → E2E-тест

### Авторизация и безопасность

| Бизнес-процесс | US | E2E-тест | Документ | Статус |
|----------------|-----|----------|----------|--------|
| Вход/выход/публичные страницы | US-002 | `US001_AuthorizationTests` | [e2e-authorization.md](e2e-authorization.md) | ✅ Реализован |

### Заседания совета директоров

| Бизнес-процесс | US | E2E-тест | Документ | Статус |
|----------------|-----|----------|----------|--------|
| Типовой устав | — | `E2E_StandardCharterTests` | [e2e-standard-charter.md](e2e-standard-charter.md) | ✅ Реализован |
| Нетиповой устав + модели ЕИО | — | `E2E_NonStandardCharterTests` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Первичный ввод состава СД | — | `E2E_BoardSetupTests` | [e2e-board-setup.md](e2e-board-setup.md) | ❌ Не пройден (страница /board-setup не загружает wizard) |
| Генеральный директор | — | `E2E_GeneralDirectorTests` | [e2e-general-director.md](e2e-general-director.md) | ✅ Реализован |

### Участники (ООО)

| Бизнес-процесс | US | E2E-тест | Документ | Статус |
|----------------|-----|----------|----------|--------|
| Список участников | US-023 | `US023_ParticipantTests` | [e2e-participant-list.md](e2e-participant-list.md) | ✅ Реализован |
| Требования участника | US-020 | `US020_ShareRequestTests` | [e2e-share-request.md](e2e-share-request.md) | ✅ Реализован |
| Каталог предоставленных документов | US-021 | `US021_DocumentCatalogTests` | [e2e-document-catalog.md](e2e-document-catalog.md) | ✅ Реализован |
| Требование о созыве ВОСУ | — | `E2E_VosuDemandTests` | [e2e-vosu-demand.md](e2e-vosu-demand.md) | ✅ Реализован |
| Изменение сведений участника / версионирование ДУЛ | — | `E2E_ParticipantDulChangeTests` | [e2e-participant-dul-change.md](e2e-participant-dul-change.md) | 🔄 Реализован, требует проверки |

### Администрирование

| Бизнес-процесс | US | E2E-тест | Документ | Статус |
|----------------|-----|----------|----------|--------|
| Управление сотрудниками | — | `E2E_UserManagementTests` | [e2e-user-management.md](e2e-user-management.md) | ✅ Реализован |

> **Примечание:** Страница «Общества» (`/legal-entities`) — первый пункт меню Admin Console.
> Создание ЮЛ выполняется через `/legal-entities`, доступ к сотрудникам — через `/access-management?le={id}`.

### ЕДИН-интеграция

| Бизнес-процесс | US | E2E-тест | Документ | Статус |
|----------------|-----|----------|----------|--------|
| ЕДИН-интеграция (UI) | — | `E2E_EdinIntegrationTests` | [e2e-edin-integration.md](e2e-edin-integration.md) | ✅ Реализован |
| ЕДИН-сценарии | — | `E2E_EdinScenarioTests` | [e2e-edin-scenarios.md](e2e-edin-scenarios.md) | ✅ Реализован |

### Версионирование данных

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Версионирование ДУЛ (SCD Type 2): ГД регистрирует участника с ДУЛ → участник подаёт новую запись сведений → автоприменение: старая версия неактивна, новая активна | — | `E2E_ParticipantDulChangeTests` | 🔄 Реализован, требует проверки |

> **Правило:** Проверка страниц справочников выполняется только через
> `PageVerificationHelper.VerifyAdminConsolePagesAsync` — строго один раз
> за прогон. Циклическая проверка в индивидуальных E2E-тестах запрещена.
> См. [AGENTS.md](../AGENTS.md#правило-страницы-справочников--проверка-строго-один-раз-крично).

### ООО — Сценарии

| Бизнес-процесс | US | E2E-тест | Документ | Статус |
|----------------|-----|----------|----------|--------|
| Требования участника (создание, просмотр) | US-020 | `US020_ShareRequestTests` | [e2e-share-request.md](e2e-share-request.md) | ✅ Реализован |
| Каталог предоставленных документов | US-021 | `US021_DocumentCatalogTests` | [e2e-document-catalog.md](e2e-document-catalog.md) | ✅ Реализован |
| ОСУ (ООСУ/ВОСУ) — встречи | US-022 | `US022_OsuMeetingTests` | — | ✅ Реализован |
| Повестка ОСУ | US-022 | `US022_OsuMeetingTests` (agenda-osu) | — | ✅ Реализован |
| Участники ООО | US-023 | `US023_ParticipantTests` | [e2e-participant-list.md](e2e-participant-list.md) | ✅ Реализован |
| Договоры (включая управляющих ИП) | US-024 | `US024_ContractTests` | — | ✅ Реализован |
| Типовой устав (выбор, просмотр, добавление участников, аудит) | — | `E2E_StandardCharterTests` (36 variants) | [e2e-standard-charter.md](e2e-standard-charter.md) | ✅ Реализован |
| Нетиповой устав (параметризованный, добавление участников, аудит) | — | `E2E_NonStandardCharterTests` (14 tests) | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: ГД — наёмный сотрудник | — | `E2E_NonStandardCharterTests::Model1_HiredCeo` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: ГД — участник общества | — | `E2E_NonStandardCharterTests::Model2_CeoParticipant` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: Управляющий — ИП (ст. 42 14-ФЗ) | — | `E2E_NonStandardCharterTests::Model3_ManagerIp` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: Управляющая организация (ст. 42 14-ФЗ) | — | `E2E_NonStandardCharterTests::Model4_ManagingOrg` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: Все участники — директора | — | `E2E_NonStandardCharterTests::Model5_AllParticipantsDirectors` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: Все участники совместно | — | `E2E_NonStandardCharterTests::Model6_AllParticipantsJoint` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: Несколько ЕИО (п. 3 ст. 65.3 ГК РФ) | — | `E2E_NonStandardCharterTests::Model7_MultipleEio` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Первичный ввод состава СД: Вариант 1 (только Председатель) | — | `E2E_BoardSetupTests::BoardSetup_Variant1_ChairOnly` | [e2e-board-setup.md](e2e-board-setup.md) | ❌ Не пройден (страница /board-setup не загружает wizard) |
| Первичный ввод состава СД: Вариант 2 (Председатель + Зам.) | — | `E2E_BoardSetupTests::BoardSetup_Variant2_ChairAndDeputy` | [e2e-board-setup.md](e2e-board-setup.md) | ❌ Не пройден (страница /board-setup не загружает wizard) |
| Первичный ввод состава СД: Вариант 3 (Председатель + Секретарь) | — | `E2E_BoardSetupTests::BoardSetup_Variant3_ChairAndSecretary` | [e2e-board-setup.md](e2e-board-setup.md) | ❌ Не пройден (страница /board-setup не загружает wizard) |
| Коллективное требование (ВОСУ) | — | `E2E_VosuDemandTests` | [e2e-vosu-demand.md](e2e-vosu-demand.md) | ✅ Реализован |
| Изменение сведений участника (ДУЛ): регистрация → информирование → версионирование | — | `E2E_ParticipantDulChangeTests::DulChange_ParticipantUpdatesPassport_ShouldVersionDocument` | [e2e-participant-dul-change.md](e2e-participant-dul-change.md) | 🔄 Реализован, требует проверки |

---

## Правила создания E2E-тестов

### При реализации новой фичи

1. **Ревизия существующих тестов** — проанализировать, не сломаны ли существующие E2E-тесты (без запуска)
2. **Создание новых тестов** — даже если сценарий не может завершиться, но может начаться:
   - Страница загружается (содержит `_framework/blazor.server.js`)
   - UI-элементы присутствуют (кнопки, формы, таблицы)
   - Контент корректен (ожидаемый текст)
3. **Обновление документации** — `docs/user-stories.md` + `docs/e2e-tests.md`

### Паттерн теста для незавершённого сценария

```csharp
/// <summary>
/// US-0XX: [Название фичи] — E2E-тест через Playwright.
/// Сценарий: страница загружается, базовые UI-элементы присутствуют.
/// Полный сценарий требует [описание зависимостей].
/// </summary>
public class US0XX_FeatureTests : BrowserFixture
{
    [Fact]
    public async Task BoardPortal_FeaturePage_ShouldLoadWithExpectedContent()
    {
        var page = await CreateBoardPortalPageAsync("/feature-page");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
        content.Should().Contain("Ожидаемый заголовок");
    }

    [Fact]
    public async Task BoardPortal_FeaturePage_HasCreateButton()
    {
        var page = await CreateBoardPortalPageAsync("/feature-page");
        (await page.QuerySelectorAsync("button.btn-primary"))
            .Should().NotBeNull();
    }
}
```

### Маппинг US → E2E-класс

| US-номер | E2E-класс | Файл |
|----------|-----------|------|
| US-001 | `US001_AuthorizationTests` | `Tests/US001_AuthorizationTests.cs` |
| US-020 | `US020_ShareRequestTests` | `Tests/US020_ShareRequestTests.cs` |
| US-021 | `US021_DocumentCatalogTests` | `Tests/US021_DocumentCatalogTests.cs` |
| US-023 | `US023_ParticipantTests` | `Tests/US023_ParticipantTests.cs` |
| — | `E2E_StandardCharterTests` | `Tests/E2E_StandardCharterTests.cs` |
| — | `E2E_NonStandardCharterTests` | `Tests/E2E_NonStandardCharterTests.cs` |
| — | `E2E_BoardSetupTests` | `Tests/E2E_BoardSetupTests.cs` |
| — | `E2E_GeneralDirectorTests` | `Tests/E2E_GeneralDirectorTests.cs` |
| — | `E2E_EdinIntegrationTests` | `Tests/E2E_EdinIntegrationTests.cs` |
| — | `E2E_EdinScenarioTests` | `Tests/E2E_EdinScenarioTests.cs` |
| — | `E2E_UserManagementTests` | `Tests/E2E_UserManagementTests.cs` |
| — | `E2E_VosuDemandTests` | `Tests/E2E_VosuDemandTests.cs` |
| — | `E2E_ParticipantDulChangeTests` | `Tests/E2E_ParticipantDulChangeTests.cs` |

---

## Запуск тестов

```bash
# Все функциональные тесты (MTP runner — .NET 10 SDK)
dotnet run --project tests/SamorodinkaTech.Fiducia.Tests.Functional

# Список всех тестов
dotnet run --project tests/SamorodinkaTech.Fiducia.Tests.Functional -- --list-tests

# Конкретный тест (через фильтр MTP)
dotnet run --project tests/SamorodinkaTech.Fiducia.Tests.Functional -- --filter "US001_AuthorizationTests"

# С видео-записью (для отладки)
# Добавить в BrowserFixture: Headless = false, RecordVideoDir = "videos/"
```

---

## Чек-лист перед мержем

- [ ] E2E-тесты созданы для новой страницы/фичи
- [ ] `docs/user-stories.md` обновлён (новый US или изменён критерий)
- [ ] `docs/e2e-tests.md` обновлён (маппинг бизнес-процесс → тест)
- [ ] Существующие E2E-тесты не сломаны (ревизия)
