# Сквозные (E2E) тесты

## Обзор

Сквозные тесты проверяют пользовательские сценарии от начала до конца: открывают реальный браузер (Playwright + Chromium), переходят на страницы приложения и проверяют корректность UI.

**Стек:** xUnit + Playwright + FluentAssertions

**Расположение:** `tests/SamorodinkaTech.Fiducia.Tests.Functional/Tests/`

**Правило синхронизации (КРИТИЧНО):** `docs/user-stories.md` и E2E-тесты (`Tests.Functional/`) должны однозначно соответствовать друг другу. Номера US в именах тестовых классов (`US001_`, `US002_`, ...) совпадают с номерами US в `docs/user-stories.md`.

---

## Результаты последнего прогона

**Дата:** 2026-09-14 (четвёртый прогон — после реализации email)
**Среда:** .NET 10 SDK (10.0.300) + Playwright Chromium (headless=false)
**Итого:** 100 тестов → **50 пройдено**, 50 сбой (каскадные + known issue)

### Изменения (с предыдущего прогона)

| Изменение | Описание |
|-----------|----------|
| Email-сервис | Реализован `IEmailService` через MailKit (SMTP) |
| NotificationService | Интегрирована отправка email после сохранения уведомления в БД |
| Board Portal | Добавлен раздел «Настройки» (`/settings`, `/settings/email`) |
| Admin Console | Удалена страница `/email-settings` (перенесена в Board Portal) |
| БД | Добавлена колонка `email_enabled` в `legal_entity_email_settings` |
| RBAC | Обновлена документация: роли LE_ADMIN, CEO для настроек email |

### Пройденные тесты (50)

| Класс | Кол-во | Статус |
|-------|--------|--------|
| `US001_AuthorizationTests` | 6 | ✅ Все пройдены |
| `US020_ShareRequestTests` | 5 | ✅ Все пройдены |
| `US021_DocumentCatalogTests` | 5 | ✅ Все пройдены |
| `US023_ParticipantTests` | 5 | ✅ Все пройдены |
| `E2E_EdinIntegrationTests` | 3 | ✅ Все пройдены |
| `E2E_EdinScenarioTests` | 2 | ✅ Все пройдены |
| `E2E_UserManagementTests` | 4 | ✅ Все пройдены |
| `E2E_GeneralDirectorTests` | 6 | ✅ Все пройдены |
| `E2E_StandardCharter_ExecBodyA_SignTests` | 6 | ✅ Все пройдены |
| `Helpers.LoginTest` | 1 | ✅ Пройден |
| **Итого** | **50** | |

### Неработоспособные тесты (50)

| Класс | Кол-во | Тип | Причина |
|-------|--------|-----|---------|
| `E2E_StandardCharter_ExecBodyA_NotarialTests` | 6 | Каскадный | `GlobalFixture.HasFailed` |
| `E2E_StandardCharter_ExecBodyB_NotarialTests` | 6 | Каскадный | `GlobalFixture.HasFailed` |
| `E2E_StandardCharter_ExecBodyB_SignTests` | 6 | Каскадный | `GlobalFixture.HasFailed` |
| `E2E_StandardCharter_ExecBodyC_NotarialTests` | 6 | Каскадный | `GlobalFixture.HasFailed` |
| `E2E_StandardCharter_ExecBodyC_SignTests` | 6 | Каскадный | `GlobalFixture.HasFailed` |
| `E2E_CustomCharterTests` | 20 | Каскадный | `GlobalFixture.HasFailed` |
| `E2E_BoardSetupTests` | 3 | Known issue | Страница `/board-setup` не загружает wizard (ADMIN-88) |
| `E2E_VosuDemandTests` | 1 | Каскадный | `GlobalFixture.HasFailed` |
| `E2E_ParticipantDulChangeTests` | 1 | Каскадный | `GlobalFixture.HasFailed` |
| **Итого** | **50** | | |

---

## Маппинг: Бизнес-процесс → US → E2E-тест

### Авторизация и безопасность

| Бизнес-процесс | US | E2E-тест | Документ | Статус |
|----------------|-----|----------|----------|--------|
| Вход/выход/публичные страницы | US-002 | `US001_AuthorizationTests` | [e2e-authorization.md](e2e-authorization.md) | ✅ Реализован |

### Заседания совета директоров

| Бизнес-процесс | US | E2E-тест | Документ | Статус |
|----------------|-----|----------|----------|--------|
| Типовой устав | — | `E2E_StandardCharter_ExecBody{A,B,C}_{Notarial,Sign}Tests` (6 файлов) | [e2e-standard-charter.md](e2e-standard-charter.md) | ✅ Реализован |
| Индивидуальный устав + модели ЕИО | — | `E2E_CustomCharterTests` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Первичный ввод состава СД | — | `E2E_BoardSetupTests` | [e2e-board-setup.md](e2e-board-setup.md) | ✅ Реализован |
| Генеральный директор | — | `E2E_GeneralDirectorTests` | [e2e-general-director.md](e2e-general-director.md) | ✅ Реализован |

### Участники (ООО)

| Бизнес-процесс | US | E2E-тест | Документ | Статус |
|----------------|-----|----------|----------|--------|
| Список участников | US-023 | `US023_ParticipantTests` | [e2e-participant-list.md](e2e-participant-list.md) | ✅ Реализован |
| Карточка участника (детальная страница) | US-023 | `US023_ParticipantTests::BoardPortal_ParticipantDetail_ShouldLoadWithAllFields` | [e2e-participant-list.md](e2e-participant-list.md) | ✅ Реализован |
| Участник ЮЛ: создание и карточка | US-023 | `US023_ParticipantTests::BoardPortal_ParticipantUl_ShouldCreateAndShowDetail` | [e2e-participant-list.md](e2e-participant-list.md) | ✅ Реализован |
| ФЛ без фамилии — отклонение (400) | US-023 | `US023_ParticipantTests::BoardPortal_ParticipantFl_EmptyLastName_ShouldReject` | [e2e-participant-list.md](e2e-participant-list.md) | ✅ Реализован |
| ФЛ без имени — отклонение (400) | US-023 | `US023_ParticipantTests::BoardPortal_ParticipantFl_EmptyFirstName_ShouldReject` | [e2e-participant-list.md](e2e-participant-list.md) | ✅ Реализован |
| Доля < 100% без оплаты — отклонение (400) | US-023 | `US023_ParticipantTests::BoardPortal_ParticipantFl_EmptyPayment_ShouldReject` | [e2e-participant-list.md](e2e-participant-list.md) | ✅ Реализован |
| Доля 100% без оплаты — допустимо (201) | US-023 | `US023_ParticipantTests::BoardPortal_ParticipantFl_FullyPaidNoPayment_ShouldAccept` | [e2e-participant-list.md](e2e-participant-list.md) | ✅ Реализован |
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
| Типовой устав (выбор, просмотр, добавление участников, аудит) | — | `E2E_StandardCharter_ExecBody{A,B,C}_{Notarial,Sign}Tests` (36 variants, 6 файлов) | [e2e-standard-charter.md](e2e-standard-charter.md) | ✅ Реализован |
| Индивидуальный устав (параметризованный, добавление участников, аудит) | — | `E2E_CustomCharterTests` (14 tests) | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: ГД — наёмный сотрудник | — | `E2E_CustomCharterTests::Model1_HiredCeo` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: ГД — участник общества | — | `E2E_CustomCharterTests::Model2_CeoParticipant` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: Управляющий — ИП (ст. 42 14-ФЗ) | — | `E2E_CustomCharterTests::Model3_ManagerIp` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: Управляющая организация (ст. 42 14-ФЗ) | — | `E2E_CustomCharterTests::Model4_ManagingOrg` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: Все участники — директора | — | `E2E_CustomCharterTests::Model5_AllParticipantsDirectors` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: Все участники совместно | — | `E2E_CustomCharterTests::Model6_AllParticipantsJoint` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Модель ЕИО: Несколько ЕИО (п. 3 ст. 65.3 ГК РФ) | — | `E2E_CustomCharterTests::Model7_MultipleEio` | [e2e-nonstandard-charter.md](e2e-nonstandard-charter.md) | ✅ Реализован |
| Первичный ввод состава СД: Вариант 1 (только Председатель) | — | `E2E_BoardSetupTests::BoardSetup_Variant1_ChairOnly` | [e2e-board-setup.md](e2e-board-setup.md) | ✅ Реализован |
| Первичный ввод состава СД: Вариант 2 (Председатель + Зам.) | — | `E2E_BoardSetupTests::BoardSetup_Variant2_ChairAndDeputy` | [e2e-board-setup.md](e2e-board-setup.md) | ✅ Реализован |
| Первичный ввод состава СД: Вариант 3 (Председатель + Секретарь) | — | `E2E_BoardSetupTests::BoardSetup_Variant3_ChairAndSecretary` | [e2e-board-setup.md](e2e-board-setup.md) | ✅ Реализован |
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
| — | `E2E_StandardCharter_ExecBody{A,B,C}_{Notarial,Sign}Tests` | `Tests/E2E_StandardCharter_ExecBody*.cs` (6 файлов) |
| — | `E2E_CustomCharterTests` | `Tests/E2E_CustomCharterTests.cs` |
| — | `E2E_BoardSetupTests` | `Tests/E2E_BoardSetupTests.cs` |
| — | `E2E_GeneralDirectorTests` | `Tests/E2E_GeneralDirectorTests.cs` |
| — | `E2E_EdinIntegrationTests` | `Tests/E2E_EdinIntegrationTests.cs` |
| — | `E2E_EdinScenarioTests` | `Tests/E2E_EdinScenarioTests.cs` |
| — | `E2E_UserManagementTests` | `Tests/E2E_UserManagementTests.cs` |
| — | `E2E_VosuDemandTests` | `Tests/E2E_VosuDemandTests.cs` |
| — | `E2E_ParticipantDulChangeTests` | `Tests/E2E_ParticipantDulChangeTests.cs` |

---

## Запуск тестов

### Подготовка окружения (обязательно перед первым запуском)

```bash
# Полная подготовка: Docker, БД, seed, сборка
./prepare-e2e.sh

# С запуском порталов после подготовки
./prepare-e2e.sh --with-portals

# Без сброса БД mnemonios (если не требуется)
./prepare-e2e.sh --skip-mnemonios

# Без seed MPI (если ЕДИН не нужен)
./prepare-e2e.sh --skip-mpi
```

Скрипт выполняет:
1. Остановка порталов
2. Загрузка `.env`
3. Сборка решения (`dotnet build`)
4. Запуск Docker-контейнеров (PostgreSQL, LDAP, ЕДИН)
5. Ожидание готовности сервисов
6. Сброс БД Fiducia (schema + seed)
7. Сброс БД mnemonios (schema + seed)
8. Seed MPI (ЕДИН API → LDAP)

### Запуск тестов

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
