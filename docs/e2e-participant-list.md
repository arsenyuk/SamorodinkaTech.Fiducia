# E2E-тест: Участники ООО

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

Участник (PARTICIPANT) заходит на страницу участников общества в Board Portal. Проверяется загрузка страницы, наличие вкладок для ООО, отсутствие ошибок и работоспособность API. Страница доступна только для ООО (ОКОПФ 12300).

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 0 | Сидирование ЮЛ (entity 1, ОКОПФ 12300) | Автоматически |
| 1 | Логин в Board Portal | Участник (`zhirov.at1`) |
| 2 | Навигация на /participants | Участник |
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
| 1 | `ShouldLoadWithExpectedContent` | Blazor shell + «Участники» |
| 2 | `ShouldHaveTabsForLLC` | Вкладка «Участники общества» |
| 3 | `ShouldNotShowNotFound` | Нет Blazor Router NotFound |
| 4 | `ApiShouldReturn200` | `GET /api/participants` → HTTP 200 |
| 5 | `ApiShouldReturnArray` | Ответ — JSON-массив |

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
var page = await CreateBoardPortalPageAsync("/participants")
var content = await page.ContentAsync()
content.Should().Contain("_framework/blazor.server.js")
content.Should().Contain("Участники")
```

### Тест 2: Вкладка для ООО

```
var content = await page.ContentAsync()
content.Should().Contain("Участники общества")
```

### Тест 3: Нет NotFound

```
var content = await page.ContentAsync()
content.Should().NotContain("Sorry, there's nothing at this address.")
```

### Тест 4–5: Проверка API

```
var status = await page.EvaluateAsync<string>(
    "async () => { const r = await fetch('/api/participants'); return r.status; }")
status.Should().Be("200")

var isArray = await page.EvaluateAsync<bool>(
    "async () => { const d = await (await fetch('/api/participants')).json(); return Array.isArray(d); }")
isArray.Should().BeTrue()
```

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../US023_ParticipantTests.cs` | E2E-тест (5 методов) |
| `tests/.../Helpers/CharterTestSeeder.cs` | Сидирование ЮЛ |
| `tests/.../Helpers/AuthHelper.cs` | Хелпер: LoginAsBoardUser |
