# E2E-тест: Требования участника

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

Участник (PARTICIPANT) заходит на страницу требований в Board Portal. Проверяется загрузка страницы, наличие кнопки создания, отсутствие ошибок и работоспособность API.

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 0 | Сидирование ЮЛ (entity 1) | Автоматически |
| 1 | Логин в Board Portal | Участник (`zhirov.at1`) |
| 2 | Навигация на /share-requests | Участник |
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
| 1 | `ShouldLoadWithExpectedContent` | Blazor shell + «Мои запросы» |
| 2 | `ShouldHaveCreateButton` | Кнопка «Подать требование» |
| 3 | `ShouldNotShowNotFound` | Нет «Sorry, there's nothing at this address.» |
| 4 | `ApiShouldReturn200` | `GET /api/share-requests` → HTTP 200 |
| 5 | `ShareRequestTypes_ApiShouldReturn200` | `GET /api/share-requests/types` → ok=true |

---

## Шаги теста

### Подготовка (общая для всех тестов)

```
InfrastructureHelper.EnsureInfrastructureReadyAsync()
CharterTestGlobalInit.InitializeAsync(adminPage, ldapPage)
CharterTestSeeder.EnsureSeededAsync(adminPage, 1)
AuthHelper.LoginAsBoardUserAsync(boardPage, "zhirov.at1")
```

### Тест 1–3: Проверка UI

```
var page = await CreateBoardPortalPageAsync("/share-requests")
var content = await page.ContentAsync()
content.Should().Contain("_framework/blazor.server.js")
content.Should().Contain("Мои запросы")  // тест 1
content.Should().Contain("Подать требование")  // тест 2
content.Should().NotContain("Sorry, there's nothing at this address.")  // тест 3
```

### Тест 4–5: Проверка API

```
var response = await page.EvaluateAsync<string>(
    "async () => { const r = await fetch('/api/share-requests'); return r.status; }")
response.Should().Be("200")

var typesResponse = await page.EvaluateAsync<bool>(
    "async () => { const r = await fetch('/api/share-requests/types'); return (await r.json()).ok; }")
typesResponse.Should().BeTrue()
```

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../US020_ShareRequestTests.cs` | E2E-тест (5 методов) |
| `tests/.../Helpers/CharterTestSeeder.cs` | Сидирование ЮЛ |
| `tests/.../Helpers/AuthHelper.cs` | Хелпер: LoginAsBoardUser |
