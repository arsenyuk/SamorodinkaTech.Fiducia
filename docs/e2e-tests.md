# Сквозные (E2E) тесты

## Обзор

Сквозные тесты проверяют пользовательские сценарии от начала до конца: открывают реальный браузер (Playwright + Chromium), переходят на страницы приложения и проверяют корректность UI.

**Стек:** xUnit + Playwright + FluentAssertions

**Расположение:** `tests/SamorodinkaTech.Fiducia.Tests.Functional/Tests/`

**Правило синхронизации (КРИТИЧНО):** `docs/user-stories.md` и E2E-тесты (`Tests.Functional/`) должны однозначно соответствовать друг другу. Номера US в именах тестовых классов (`US001_`, `US002_`, ...) совпадают с номерами US в `docs/user-stories.md`.

---

## Маппинг: Бизнес-процесс → US → E2E-тест

### Авторизация и безопасность

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Вход/выход/публичные страницы | US-002 | `US001_AuthorizationTests` | ✅ Реализован |

### Заседания совета директоров

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Типовой устав | — | `E2E_StandardCharterTests` | ✅ Реализован |
| Нетиповой устав | — | `E2E_NonStandardCharterTests` | ✅ Реализован |
| Первичный ввод состава СД | — | `E2E_BoardSetupTests` | ✅ Реализован |
| Генеральный директор | — | `E2E_GeneralDirectorTests` | ✅ Реализован |

### Участники (ООО)

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Список участников | US-023 | `US023_ParticipantTests` | ✅ Реализован |
| Требования участника | US-020 | `US020_ShareRequestTests` | ✅ Реализован |
| Каталог предоставленных документов | US-021 | `US021_DocumentCatalogTests` | ✅ Реализован |
| Требование о созыве ВОСУ | — | `E2E_VosuDemandTests` | ✅ Реализован |

### Администрирование

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Управление пользователями | — | `E2E_UserManagementTests` | ✅ Реализован |

### ЕДИН-интеграция

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| ЕДИН-интеграция (привязка) | — | `E2E_EdinIntegrationTests` | ✅ Реализован |
| ЕДИН-сценарии | — | `E2E_EdinScenarioTests` | ✅ Реализован |

> **Правило:** Проверка страниц справочников выполняется только через
> `PageVerificationHelper.VerifyAdminConsolePagesAsync` — строго один раз
> за прогон. Циклическая проверка в индивидуальных E2E-тестах запрещена.
> См. [AGENTS.md](../AGENTS.md#правило-страницы-справочников--проверка-строго-один-раз-крично).

### ООО — Сценарии

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Требования участника (создание, просмотр) | US-020 | `US020_ShareRequestTests` | ✅ Реализован |
| Каталог предоставленных документов | US-021 | `US021_DocumentCatalogTests` | ✅ Реализован |
| ОСУ (ООСУ/ВОСУ) — встречи | US-022 | `US022_OsuMeetingTests` | ✅ Реализован |
| Повестка ОСУ | US-022 | `US022_OsuMeetingTests` (agenda-osu) | ✅ Реализован |
| Участники ООО | US-023 | `US023_ParticipantTests` | ✅ Реализован |
| Договоры (включая управляющих ИП) | US-024 | `US024_ContractTests` | ✅ Реализован |
| Типовой устав (выбор, просмотр, добавление участников, аудит) | — | `E2E_StandardCharterTests` (36 variants) | ✅ Реализован |
| Нетиповой устав (параметризованный, добавление участников, аудит) | — | `E2E_NonStandardCharterTests` (14 tests) | ✅ Реализован |
| Модель ЕИО: ГД — наёмный сотрудник | — | `E2E_NonStandardCharterTests::Model1_HiredCeo` | ✅ Реализован |
| Модель ЕИО: ГД — участник общества | — | `E2E_NonStandardCharterTests::Model2_CeoParticipant` | ✅ Реализован |
| Модель ЕИО: Управляющий — ИП (ст. 42 14-ФЗ) | — | `E2E_NonStandardCharterTests::Model3_ManagerIp` | ✅ Реализован |
| Модель ЕИО: Управляющая организация (ст. 42 14-ФЗ) | — | `E2E_NonStandardCharterTests::Model4_ManagingOrg` | ✅ Реализован |
| Модель ЕИО: Все участники — директора | — | `E2E_NonStandardCharterTests::Model5_AllParticipantsDirectors` | ✅ Реализован |
| Модель ЕИО: Все участники совместно | — | `E2E_NonStandardCharterTests::Model6_AllParticipantsJoint` | ✅ Реализован |
| Модель ЕИО: Несколько ЕИО (п. 3 ст. 65.3 ГК РФ) | — | `E2E_NonStandardCharterTests::Model7_MultipleEio` | ✅ Реализован |
| Первичный ввод состава СД: Вариант 1 (только Председатель) | — | `E2E_BoardSetupTests::BoardSetup_Variant1_ChairOnly` | ✅ Реализован |
| Первичный ввод состава СД: Вариант 2 (Председатель + Зам.) | — | `E2E_BoardSetupTests::BoardSetup_Variant2_ChairAndDeputy` | ✅ Реализован |
| Первичный ввод состава СД: Вариант 3 (Председатель + Секретарь) | — | `E2E_BoardSetupTests::BoardSetup_Variant3_ChairAndSecretary` | ✅ Реализован |
| Коллективное требование (ВОСУ) | — | — | ❌ Нет теста |

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

---

## Запуск тестов

```bash
# Все функциональные тесты
dotnet test tests/SamorodinkaTech.Fiducia.Tests.Functional

# Конкретный тест
dotnet test --filter "FullyQualifiedName~US001_AuthorizationTests"

# С видео-записью (для отладки)
# Добавить в BrowserFixture: Headless = false, RecordVideoDir = "videos/"
```

---

## Чек-лист перед мержем

- [ ] E2E-тесты созданы для новой страницы/фичи
- [ ] `docs/user-stories.md` обновлён (новый US или изменён критерий)
- [ ] `docs/e2e-tests.md` обновлён (маппинг бизнес-процесс → тест)
- [ ] Существующие E2E-тесты не сломаны (ревизия)
