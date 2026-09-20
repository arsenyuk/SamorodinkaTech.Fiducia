# E2E-тест: Вкладка «Генеральный директор»

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

На странице ЮЛ в Board Portal находится вкладка «ГД» (генеральный директор). Вкладка видна только при ExecutiveBody=A (ГД — отдельное лицо) для индивидуального или типового устава. Администратор назначает участника генеральным директором, вводит СНИЛС, проверяет сохранение данных. При ExecutiveBody=B/C вкладка не отображается.

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 0 | Подготовка окружения | Автоматически |
| 1 | Логин в Board Portal | ГД |
| 2 | Настройка ЮЛ (устав + ExecutiveBody) | ГД |
| 3 | Добавление участников общества | ГД |
| 4 | Проверка видимости/невидимости вкладки «ГД» | Автоматическая (assert) |
| 5 | Назначение участника ГД + ввод СНИЛС | ГД |
| 6 | Сохранение и проверка персистентности | Автоматическая (assert) |

---

## Действующие лица

| Роль в системе | Логин | Место в тесте |
|----------------|-------|---------------|
| ГД + Администратор ЮЛ | Из `CharterTestDataFixed` | Шаг 1–5 |

---

## Предусловия

1. Инфраструктура запущена
2. БД сброшена
3. ЮЛ сидировано (entityIndex 58–63)

---

## Тесты

| # | Метод | entityIndex | ExecutiveBody | Устав | Вкладка «ГД» | Проверка |
|---|-------|:-----------:|:---:|-------|:---:|----------|
| 1 | `TabVisible_ShouldAssignParticipantAndSave` | 58 | A | Индивидуальный | Видна | Назначение 1-го участника ГД + СНИЛС |
| 2 | `TwoParticipants_ShouldSelectSecond` | 59 | A | Индивидуальный | Видна | Выбор 2-го участника |
| 3 | `SaveWithSnils_ShouldPersistData` | 60 | A | Индивидуальный | Видна | Сохранение + проверка после навигации |
| 4 | `ExecBodyB_TabNotVisible` | 61 | B | Индивидуальный | НЕ видна | — |
| 5 | `ExecBodyC_TabNotVisible` | 62 | C | Индивидуальный | НЕ видна | — |
| 6 | `StandardCharter_ExecBodyA_TabVisible` | 63 | A | Типовой №1 | Видна | Назначение ГД при типовом уставе |

---

## Шаги теста

### Шаг 1–3: Подготовка

```
SetupFullCycleAsync(entityIndex)
BoardPortalHelper.SelectCustomCharterAsync(boardPage)  // или SelectStandardCharterAsync
BoardPortalHelper.SetExecutiveBodyAsync(boardPage, executiveBodyType)
BoardPortalHelper.AddParticipantAsync(boardPage, ...)  // с ДУЛ
```

### Шаг 4: Проверка видимости вкладки

**Для ExecBody=A (тесты 1–3, 6):**
```
var gdTab = await boardPage.QuerySelectorAsync("button:has-text('ГД')");
gdTab.Should().NotBeNull();
```

**Для ExecBody=B/C (тесты 4–5):**
```
var gdTab = await boardPage.QuerySelectorAsync("button:has-text('ГД')");
gdTab.Should().BeNull();
```

### Шаг 5: Назначение ГД + СНИЛС

**Действие:**
1. Клик по вкладке «ГД»
2. Выбор участника из dropdown
3. Проверка readonly-полей (ФИО, ИНН, тип участника)
4. Ввод СНИЛС (например, `123-456-789 00`)
5. Клик «Сохранить»

### Шаг 6: Проверка персистентности (тест 3)

**Действие:** Повторный переход на вкладку «ГД»

**Проверка:** Данные (участник + СНИЛС) сохранились после навигации

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../E2E_GeneralDirectorTests.cs` | E2E-тест (6 методов) |
| `tests/.../Helpers/CharterTestDataFixed.cs` | Тестовые данные (entityIndex 58–63) |
| `tests/.../Helpers/BoardPortalHelper.cs` | Хелперы: SetExecutiveBody, GD-related helpers |
