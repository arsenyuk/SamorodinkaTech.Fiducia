# E2E-тест: ЕДИН-интеграция (проверка UI)

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

Система ЕДИН (Единый Дистрибутив Идентификационных Номеров) интегрирована с Fiducia для привязки пользователей к MPI MasterId. Проверяются UI-элементы: столбец «ЕДИН» в списке пользователей, вкладка «ЕДИН» на странице пользователя, отображение MPI MasterId или статуса «Не привязан».

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 1 | Логин в Admin Console | SYS_ADMIN (`v.vasilyeva`) |
| 2 | Проверка столбца «ЕДИН» в списке пользователей | Автоматическая (assert) |
| 3 | Проверка вкладки «ЕДИН» на странице пользователя | Автоматическая (assert) |
| 4 | Проверка содержимого вкладки «ЕДИН» | Автоматическая (assert) |

---

## Действующие лица

| Роль в системе | Логин | Место в тесте |
|----------------|-------|---------------|
| SYS_ADMIN | `v.vasilyeva` | Все шаги |

---

## Предусловия

1. Инфраструктура запущена
2. Admin Console доступен (порт 5001)
3. Пользователи созданы в LDAP

---

## Тесты

| # | Метод | Проверка |
|---|-------|----------|
| 1 | `UsersList_ShouldHaveEdinColumn` | Таблица `/users` содержит `th:has-text('ЕДИН')` |
| 2 | `UserDetail_ShouldHaveEdinTab` | Страница пользователя содержит `button:text('ЕДИН')` |
| 3 | `EdinTab_ShouldShowMpiMasterIdOrNotLinked` | Вкладка «ЕДИН» содержит «MPI MasterId» или «Не привязан» |

---

## Шаги теста

### Шаг 1: Проверка столбца «ЕДИН»

**Действие:**
```
AuthHelper.LoginAsAdminAsync(adminPage, "v.vasilyeva")
// /users не в sidebar — прямой переход
page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/users"))
var th = await adminPage.WaitForSelectorAsync("th:has-text('ЕДИН')")
```

**Проверка:** `th.Should().NotBeNull()`

### Шаг 2: Проверка вкладки «ЕДИН»

**Действие:**
```
// /users не в sidebar — прямой переход
page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/users"))
await adminPage.ClickAsync("tbody tr:first-child")
var edinTab = await adminPage.WaitForSelectorAsync("button:text('ЕДИН')")
```

**Проверка:** `edinTab.Should().NotBeNull()`

### Шаг 3: Проверка содержимого

**Действие:**
```
await edinTab.ClickAsync()
await adminPage.WaitForFunctionAsync(
    "() => document.body.innerText.includes('MPI MasterId') || document.body.innerText.includes('Не привязан')")
```

**Проверка:** Содержит «MPI MasterId» или «Не привязан»

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../E2E_EdinIntegrationTests.cs` | E2E-тест (3 метода) |
| `tests/.../Helpers/AdminConsoleHelper.cs` | Хелпер: NavigateToAsync |
| `tests/.../Helpers/AuthHelper.cs` | Хелпер: LoginAsAdminAsync |
