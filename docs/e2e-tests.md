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
| Вход в систему | US-002 | `US001_AuthorizationTests` | ✅ Реализован |
| Выход из системы | US-003 | `US001_AuthorizationTests` (public pages) | ✅ Реализован |
| Публичные страницы | US-002 | `US001_AuthorizationTests` | ✅ Реализован |
| Принудительное закрытие сессии | US-011 | — | ❌ Нет теста |
| Аудит-лог входов/выходов | US-012 | — | ❌ Нет теста |

### Заседания совета директоров

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Создание заседания | US-004 | `US002_MeetingTests` | ✅ Реализован |
| Голосование | US-005 | `US005_VotingTests` | ✅ Реализован |
| Просмотр документов заседания | US-008 | `US004_CommitteeTests` (documents page) | ✅ Реализован |
| Печатные формы по ГОСТу | US-009 | `US004_CommitteeTests` (print-forms page) | ✅ Реализован |

### Комитеты

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Управление комитетами | US-006 | `US004_CommitteeTests` | ✅ Реализован |

### Участники (ООО)

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Список участников | US-023 | `US023_ParticipantTests` | ✅ Реализован |
| Требования участника | US-020 | `US020_ShareRequestTests` | ✅ Реализован |
| Каталог предоставленных документов | US-021 | `US021_DocumentCatalogTests` | ✅ Реализован |

### Оповещения

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Оповещения (UI) | US-010 | `US010_NotificationTests` | ✅ Реализован |
| Отправленные уведомления (Admin) | US-010.1 | `US004_CommitteeTests` (sent-notifications) | ✅ Реализован |

### Администрирование

| Бизнес-процесс | US | E2E-тест | Статус |
|----------------|-----|----------|--------|
| Настройка ЮЛ | US-001 | `US001_AuthorizationTests` (admin console) | ✅ Реализован |
| Справочники | US-012 (admin) | `US004_CommitteeTests` (dictionaries) | ✅ Реализован |
| Пользователи | — | `US004_CommitteeTests` (users, roles) | ✅ Реализован |
| Шаблоны орг-планов | — | `US004_CommitteeTests` (org-templates) | ✅ Реализован |
| Настройки | — | `US002_MeetingTests` (settings) | ✅ Реализован |

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
| US-002 | `US002_MeetingTests` | `Tests/US002_MeetingTests.cs` |
| US-004 | `US004_CommitteeTests` | `Tests/US004_CommitteeTests.cs` |
| US-005 | `US005_VotingTests` | `Tests/US005_VotingTests.cs` |
| US-010 | `US010_NotificationTests` | `Tests/US010_NotificationTests.cs` |
| US-020 | `US020_ShareRequestTests` | `Tests/US020_ShareRequestTests.cs` |
| US-021 | `US021_DocumentCatalogTests` | `Tests/US021_DocumentCatalogTests.cs` |
| US-022 | `US022_OsuMeetingTests` | `Tests/US022_OsuMeetingTests.cs` |
| US-023 | `US023_ParticipantTests` | `Tests/US023_ParticipantTests.cs` |
| US-024 | `US024_ContractTests` | `Tests/US024_ContractTests.cs` |
| — | `E2E_StandardCharterTests` | `Tests/E2E_StandardCharterTests.cs` |
| — | `E2E_NonStandardCharterTests` | `Tests/E2E_NonStandardCharterTests.cs` |

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
