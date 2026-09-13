# E2E-тест: Первичный ввод состава Совета директоров

> **Связанные документы:** [Сквозные тесты](e2e-tests.md)

## Сценарий

### Бизнес-условие

Для ООО с нетиповым уставом, в котором предусмотрены Совет директоров, необходимо выполнить первичный ввод состава СД через wizard назначения ролей. Три варианта: (1) только Председатель СД, (2) Председатель + Зам. председателя, (3) Председатель + Секретарь СД.

### Кто что делает

| Шаг | Действие | Исполнитель |
|-----|----------|-------------|
| 0 | Подготовка окружения | Автоматически |
| 1 | Логин в Board Portal | ГД |
| 2 | Настройка ЮЛ (нетиповый устав + has-board=true) | ГД |
| 3 | Добавление участников общества | ГД |
| 4 | Переход на /board-setup | ГД |
| 5 | Добавление членов СД через wizard | ГД |
| 6 | Сохранение состава СД | ГД |
| 7 | Проверка в Admin Console (/board-participant-roles) | Автоматическая (assert) |

---

## Действующие лица

| Роль в системе | Логин | Место в тесте |
|----------------|-------|---------------|
| ГД + Администратор ЮЛ | Из `CharterTestDataFixed` | Шаг 1–6 |

---

## Предусловия

1. Инфраструктура запущена
2. БД сброшена
3. ЮЛ сидировано (entityIndex 64–66)
4. ЮЛ настроено: нетиповый устав + `has-board-of-directors=true`

---

## Шаги теста

### Шаг 1: Подготовка ЮЛ

```
SetupFullCycleAsync(entityIndex)
BoardPortalHelper.SelectNonStandardCharterAsync(boardPage)
BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", "true")
BoardPortalHelper.AddParticipantAsync(boardPage, ...)
```

### Шаг 2: Переход на wizard состава СД

```
boardPage.GotoAsync($"{baseUrl}/board-setup")
boardPage.WaitForSelectorAsync("h3")
```

**Проверка:** На странице отображаются роли: «Председатель СД», «Зам. председателя», «Секретарь СД»

### Шаг 3: Добавление членов СД

**Вариант 1 (entityIndex 64):**
```
AddBoardMemberAsync(boardPage, "CHAIR", "Иванов Иван Иванович")
```

**Вариант 2 (entityIndex 65):**
```
AddBoardMemberAsync(boardPage, "CHAIR", "Петров Пётр Петрович")
AddBoardMemberAsync(boardPage, "DEPUTY_CHAIR", "Сидоров Сидор Сидорович")
```

**Вариант 3 (entityIndex 66):**
```
AddBoardMemberAsync(boardPage, "CHAIR", "Козлов Козлом Козлович")
AddBoardMemberAsync(boardPage, "SECRETARY", "Федорова Федора Федоровна")
```

### Шаг 4: Сохранение

**Действие:** Клик «Сохранить состав СД»

**Проверка:** Появляется сообщение об успешном сохранении

### Шаг 5: Проверка в Admin Console

**Действие:** Навигация на `/board-participant-roles`

**Проверка:** Заголовок h3 содержит «Назначения ролей в СД»

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../E2E_BoardSetupTests.cs` | E2E-тест (3 метода) |
| `tests/.../Helpers/CharterTestDataFixed.cs` | Тестовые данные (entityIndex 64–66) |
| `tests/.../Helpers/BoardPortalHelper.cs` | Хелперы: AddBoardMemberAsync, NavigateToBoardSetupAsync |
| `tests/.../Helpers/AdminConsoleHelper.cs` | Проверка Admin Console |

---

## Известные проблемы

### 1. Страница /board-setup не загружает wizard

**Симптом:** Тест падает с ошибкой `Timeout 30000ms exceeded. waiting for Locator("text=Председатель СД") to be visible`

**Корневая причина:** Страница `/board-setup?leId={id}` требует параметр `leId` в URL. Без него `BoardSetupWizard.razor` показывает ошибку «Не указано юридическое лицо (параметр leId)» и не загружает wizard.

**Статус:** Тесты `BoardSetup_Variant1/2/3` не пройдены. Требуется исправление навигации на страницу wizard — передавать `legalEntityId` из БД после сидирования ЮЛ.
