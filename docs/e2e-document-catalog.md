# E2E-тест: Каталог предоставленных документов

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

Участник (PARTICIPANT) заходит на страницу каталога предоставленных документов. Проверяется загрузка страницы, наличие accordion-элементов или пустого состояния, отсутствие ошибок и работоспособность API.

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 0 | Сидирование ЮЛ (entity 1) | Автоматически |
| 1 | Логин в Board Portal | Участник (`zhirov.at1`) |
| 2 | Навигация на /documents/catalog | Участник |
| 3 | Проверка UI и API | Автоматическая (assert) |

---

## Действующие лица

| Роль в системе | Логин | Место в тесте |
|----------------|-------|---------------|
| Участник ООО | `zhirov.at1` | Шаг 1–2 |

---

## Тесты

| # | Метод | Проверка |
|---|-------|----------|
| 1 | `ShouldLoadWithExpectedContent` | Blazor shell + «Предоставленные документы» |
| 2 | `ShouldHaveAccordionOrEmptyState` | Accordion элемент ИЛИ «Нет предоставленных документов» |
| 3 | `ShouldNotShowNotFound` | Нет Blazor Router NotFound |
| 4 | `ApiShouldReturn200` | `GET /api/documents/catalog` → HTTP 200 |
| 5 | `ApiShouldReturnGroups` | Ответ содержит `data.groups` как массив |

---

## Шаги теста

### Подготовка (общая)

```
InfrastructureHelper.EnsureInfrastructureReadyAsync()
CharterTestGlobalInit.InitializeAsync(adminPage, ldapPage)
CharterTestSeeder.EnsureSeededAsync(adminPage, 1)
AuthHelper.LoginAsBoardUserAsync(boardPage, "zhirov.at1")
```

### Тест 1: Содержимое страницы

```
var page = await CreateBoardPortalPageAsync("/documents/catalog")
var content = await page.ContentAsync()
content.Should().Contain("_framework/blazor.server.js")
content.Should().Contain("Предоставленные документы")
```

### Тест 2: Accordion или пустое состояние

```
var accordion = await page.QuerySelectorAsync(".accordion, [class*=accordion]")
var emptyState = await page.QuerySelectorAsync("*:has-text('Нет предоставленных документов')")
(accordion is not null || emptyState is not null).Should().BeTrue()
```

### Тест 3: Нет NotFound

```
var content = await page.ContentAsync()
content.Should().NotContain("Sorry, there's nothing at this address.")
```

### Тест 4–5: Проверка API

```
var status = await page.EvaluateAsync<string>(
    "async () => { const r = await fetch('/api/documents/catalog'); return r.status; }")
status.Should().Be("200")

var hasGroups = await page.EvaluateAsync<bool>(
    "async () => { const d = await (await fetch('/api/documents/catalog')).json(); return Array.isArray(d.groups); }")
hasGroups.Should().BeTrue()
```

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../US021_DocumentCatalogTests.cs` | E2E-тест (5 методов) |
| `tests/.../Helpers/CharterTestSeeder.cs` | Сидирование ЮЛ |
| `tests/.../Helpers/AuthHelper.cs` | Хелпер: LoginAsBoardUser |
