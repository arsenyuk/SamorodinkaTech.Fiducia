# E2E-тест: Управление сотрудниками

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

Администратор добавляет сотрудника через `/access-management` в Admin Console.
ЮЛ создаётся на отдельной странице «Общества» (`/legal-entities`), затем администратор переходит к сотрудникам через клик по ЮЛ.
Проверяется поведение при разных состояниях: несуществующий LDAP-логин, LDAP найден без роли, LDAP найден с ролью.

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 1 | Логин в Admin Console | SYS_ADMIN (`v.vasilyeva`) |
| 2 | Создание тестового ЮЛ на странице «Общества» | SYS_ADMIN |
| 3 | Переход к сотрудникам (клик по ЮЛ) | SYS_ADMIN |
| 4 | Открытие модала добавления сотрудника | SYS_ADMIN |
| 5 | Ввод логина + поиск в LDAP | SYS_ADMIN |
| 6 | Проверка состояния кнопки «Добавить» | Автоматическая (assert) |

---

## Действующие лица

| Роль в системе | Логин | Место в тесте |
|----------------|-------|---------------|
| SYS_ADMIN | `v.vasilyeva` | Все шаги |

---

## Тесты

| # | Метод | LDAP-логин | Результат LDAP | Роль | Кнопка |
|---|-------|-----------|:---:|-------|:---:|
| 1 | `AddEmployee_LdapNotFound_ShowsWarningAndButtonDisabled` | `nonexistent_user_xyz_999` | Не найден | — | disabled |
| 2 | `AddEmployee_LdapFoundButNoRole_ButtonDisabled` | `nechaev.va` | Найден | Не выбрана | disabled |
| 3 | `AddEmployee_LdapFoundAndRoleSelected_ButtonEnabled` | `nechaev.va` | Найден | LE_ADMIN | enabled |

---

## Шаги теста

### Шаг 1–3: Подготовка

```
AuthHelper.LoginAsAdminAsync(adminPage, "v.vasilyeva")
AdminConsoleHelper.CreateLegalEntityAsync(adminPage, "Тестовое ЮЛ", inn)
// CreateLegalEntityAsync создаёт ЮЛ на /legal-entities и переходит к /access-management?le={id}
```

### Шаг 4: Открытие модала

**Действие:** На странице `/access-management?le={id}` клик «Добавить сотрудника»

### Шаг 5: Ввод логина + поиск

**Действие:** Ввод логина в поле → клик кнопки поиска

**Для теста 1 (LDAP не найден):**
```
Ожидание: .text-warning с текстом «не найден в LDAP»
Кнопка «Добавить»: disabled
```

**Для теста 2 (LDAP найден, без роли):**
```
Ожидание: readonly input заполнен (ФИО из LDAP)
Предупреждений нет
Кнопка «Добавить»: disabled (роль не выбрана)
```

**Для теста 3 (LDAP найден + роль):**
```
Ожидание: readonly input заполнен
Выбор роли: LE_ADMIN из dropdown
Кнопка «Добавить»: enabled
```

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../E2E_UserManagementTests.cs` | E2E-тест (3 метода) |
| `tests/.../Helpers/AdminConsoleHelper.cs` | Хелпер: CreateLegalEntity, NavigateTo |
| `tests/.../Helpers/AuthHelper.cs` | Хелпер: LoginAsAdmin |
