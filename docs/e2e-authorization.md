# E2E-тест: Авторизация и публичные страницы

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

Проверяются страницы авторизации и публичные страницы Board Portal и Admin Console: страница входа, лендинг, onboarding, proposal. Тесты не требуют авторизации и могут выполняться параллельно (без Collection).

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 1 | Открытие страницы входа / лендинга | Анонимный пользователь |
| 2 | Проверка UI-элементов | Автоматическая (assert) |

---

## Тесты

| # | Метод | Страница | Проверка |
|---|-------|----------|----------|
| 1 | `BoardPortal_LoginPage_ShowsSelectDropdown` | `/login` (Board Portal) | `select.form-select` видим |
| 2 | `AdminConsoleLoginPage_LoadsBlazorShell` | `/login` (Admin Console) | Содержит `_framework/blazor.server.js` |
| 3 | `BoardPortal_LoginPage_ShowsNoSidebar` | `/login` (Board Portal) | `.sidebar` отсутствует (AuthLayout) |
| 4 | `BoardPortal_PublicLanding_Present` | `/` (Board Portal) | Содержит «Fiducia» |
| 5 | `BoardPortal_OnboardingPage_Rendered` | `/onboarding` | Blazor shell загружен |
| 6 | `BoardPortal_ProposalPage_RenderedForAnonymousUsers` | `/proposal` | Blazor shell загружен |

---

## Шаги теста

### Тест 1: Dropdown на странице входа

```
var page = await CreateBoardPortalPageAsync("/login")
var dropdown = await page.WaitForSelectorAsync("select.form-select")
dropdown.Should().NotBeNull()
```

### Тест 2: Blazor shell Admin Console

```
var page = await CreateAdminConsolePageAsync("/login")
var content = await page.ContentAsync()
content.Should().Contain("_framework/blazor.server.js")
```

### Тест 3: Нет sidebar на login Board Portal

```
var page = await CreateBoardPortalPageAsync("/login")
var sidebar = await page.QuerySelectorAsync(".sidebar")
sidebar.Should().BeNull()
```

### Тест 4: Публичный лендинг

```
var page = await CreateBoardPortalPageAsync("/")
var content = await page.ContentAsync()
content.Should().Contain("Fiducia")
```

### Тест 5–6: Onboarding и Proposal

```
var page = await CreateBoardPortalPageAsync("/onboarding")  // или "/proposal"
var content = await page.ContentAsync()
content.Should().Contain("_framework/blazor.server.js")
```

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../US001_AuthorizationTests.cs` | E2E-тест (6 методов) |
| `tests/.../BrowserFixture.cs` | Базовый класс: CreateBoardPortalPageAsync, CreateAdminConsolePageAsync |
