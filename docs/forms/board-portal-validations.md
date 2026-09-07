# Валидации полей — Board Portal

Список всех валидаций полей на формах Board Portal (`http://localhost:5002`).

> **Связанные документы:** [E2E-тесты](../e2e-tests.md) | [Бизнес-сценарии](../e2e-scenarios.md)

---

## Страница входа (`/login`)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/Login.razor`

| Поле | Тип | Правило | Сообщение | Тесты |
|------|-----|---------|-----------|-------|
| Пароль | Клиентская | `string.IsNullOrEmpty(_password)` | "Введите пароль" | [`US001_AuthorizationTests`](../e2e-tests.md#авторизация-и-безопасность) |
| Логин (Basic) | Клиентская | `string.IsNullOrEmpty(_username)` | "Введите логин или выберите пользователя" | [`US001_AuthorizationTests`](../e2e-tests.md#авторизация-и-безопасность) |
| Аутентификация | Серверная | `result.Success == false` | Ответ провайдера (`result.ErrorMessage`) | [`US001_AuthorizationTests`](../e2e-tests.md#авторизация-и-безопасность) |
| Онбординг | Post-login | `is_external == True && !pep_signed` | Редирект на `/onboarding` | — (нет теста) |

---

## Онбординг (`/onboarding/{Token}`)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/Onboarding.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Токен URL | Пользователь найден и приглашение не истекло | "Приглашение не найдено" / "Ссылка недействительна..." | — (нет теста) |
| Чекбокс «Нет судимости» | Required | (кнопка "Далее" заблокирована) | — (нет теста) |
| Чекбокс «Нет банкротства» | Required | (кнопка "Далее" заблокирована) | — (нет теста) |
| Чекбокс «Согласие на ОД» | Required | (кнопка "Далее" заблокирована) | — (нет теста) |
| OTP-код | 6 символов | (кнопка "Подтвердить" заблокирована) | — (нет теста) |
| OTP-код | Верификация | "Неверный код. Попробуйте ещё раз." | — (нет теста) |

---

## Предложение к повестке (`/proposal`)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/Proposal.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| ФИО | Required | "Укажите ФИО." | — (нет теста) |
| Текст вопроса | Required | "Опишите суть вопроса." | — (нет теста) |
| Email | Optional | (не валидируется) | — |

---

## Заседания СД (Meetings — создание)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/Meetings.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Номер заседания | Required | "Введите номер заседания" | [`US002_MeetingTests`](../e2e-tests.md#заседания-совета-директоров) |
| Дедлайн | > даты начала | "Дедлайн должен быть позже даты начала" | [`US002_MeetingTests`](../e2e-tests.md#заседания-совета-директоров) |
| Срок голосования | >= 3 календарных дней | "Минимальный срок голосования — 3 календарных дня (ст. 68 208-ФЗ)" | [`US002_MeetingTests`](../e2e-tests.md#заседания-совета-директоров) |
| Участники | Min 1 | "Выберите хотя бы одного участника" | [`US002_MeetingTests`](../e2e-tests.md#заседания-совета-директоров) |

---

## Комитеты (Committees — создание)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/Committees.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Код-аббревиатура | Required | "Введите код-аббревиатуру" | [`US004_CommitteeTests`](../e2e-tests.md#комитеты) |
| Наименование | Required | "Введите наименование комитета" | [`US004_CommitteeTests`](../e2e-tests.md#комитеты) |
| Председатель != секретарь | Не одно лицо | "Председатель и секретарь не могут быть одним лицом" | [`US004_CommitteeTests`](../e2e-tests.md#комитеты) |
| Код | DB: уникальность | "Комитет с таким кодом уже существует" | [`US004_CommitteeTests`](../e2e-tests.md#комитеты) |

---

## Договоры — добавление (ContractAdd)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/ContractAdd.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| ИНН ИП (MANAGEMENT_IP) | 12 цифр | "ИНН ИП: 12 цифр, ОГРНИП: 15 цифр" | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| ИНН ИП | Контрольная сумма (InnIpValidator) | "Неверная контрольная сумма ИНН (10-я цифра)" / "(12-я цифра)" | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| ОГРНИП (MANAGEMENT_IP) | 15 цифр | "ИНН ИП: 12 цифр, ОГРНИП: 15 цифр" | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| ОГРНИП | Контрольная сумма (OgrnipValidator) | "Неверная контрольная сумма ОГРНИП" | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| ИНН организации (MANAGEMENT_UL) | 10 цифр | "ИНН: 10 цифр, ОГРН: 13 цифр" | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| ОГРН (MANAGEMENT_UL) | 13 цифр | "ИНН: 10 цифр, ОГРН: 13 цифр" | [`US024_ContractTests`](../e2e-tests.md#договоры) |

---

## Договоры — редактирование (ContractEdit)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/ContractEdit.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Файл | max 52 MB | (catch -> `_formError`) | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| Сохранение | Server | Тело ошибки API (`"Ошибка сохранения: {body}"`) | [`US024_ContractTests`](../e2e-tests.md#договоры) |

---

## Юридическое лицо (LegalEntities — wizard)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/LegalEntities.razor`
**Валидатор:** `LegalEntityValidator.Validate()` + дополнительные проверки в `Save()`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| ФИО руководителя | Required | "Укажите ФИО руководителя." | [`LegalEntityValidatorTests::Director_*`](../e2e-tests.md#маппинг-us--e2e-класс), `E2E_StandardCharterTests`, `E2E_NonStandardCharterTests` |
| Должность руководителя | Required | "Укажите должность руководителя." | [`LegalEntityValidatorTests::Director_*`](../e2e-tests.md#маппинг-us--e2e-класс), `E2E_StandardCharterTests`, `E2E_NonStandardCharterTests` |
| Окно ГОСА | End >= Start | "Дата окончания окна ГОСА не может быть раньше даты начала." | [`LegalEntityValidatorTests::Gosa_*`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Окно ГОСА (ПАО) | 01.03-30.06 | "Для ПАО окно ГОСА должно находиться в пределах 01.03-30.06." | [`LegalEntityValidatorTests::Gosa_PAO_*`](../e2e-tests.md#маппинг-us--e2e-класс), `E2E_StandardCharterTests`, `E2E_NonStandardCharterTests` |
| Окно ГОСА (НАО) | Фиксировано 01.03-30.06 | "Для НАО интервал ГОСА фиксирован: 01.03-30.06." | [`LegalEntityValidatorTests::Gosa_NAO_*`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Типовой устав | 01-36, 2 цифры | "Номер типового устава должен быть от 01 до 36." | `E2E_StandardCharterTests` (36 тестов) |
| Типовой устав | Только для ООО | "Типовой устав применим только для ООО." | `E2E_StandardCharterTests` |
| СНИЛС руководителя | 11 цифр | "СНИЛС должен содержать 11 цифр (формат XXX-XXX-XXX XX)" | [`E2E_GeneralDirectorTests::GeneralDirector_SaveWithSnils`](../e2e-tests.md#маппинг-us--e2e-класс) |
| Макс. комитетов на члена | > 0 (если задано) | "Укажите максимальное число комитетов для одного члена СД" | — (нет теста) |
| Макс. возглавляемых комитетов | > 0 (если задано) | "Укажите максимальное число возглавляемых комитетов для одного члена СД" | — (нет теста) |
| Мин. членов комитета | >= 2 (если задано) | "Минимальное количество членов в комитете должно быть не менее 2" | — (нет теста) |
| Кворум комитета | 1-100% (если задано) | "Кворум комитета должен быть от 1 до 100%" | — (нет теста) |
| Кворум совместного заседания | 1-100% (если задано) | "Кворум совместного заседания должен быть от 1 до 100%" | — (нет теста) |
| Кворум СД | >= 50% | "Кворум не может быть менее 50% (п. 2 ст. 68 208-ФЗ)" | — (нет теста) |
| Интервал ГОСА (ОКОПФ) | Валидность для типа ЮЛ | "Недопустимый интервал ГОСА для данной ОПФ" | `E2E_StandardCharterTests`, `E2E_NonStandardCharterTests` |

---

## Список ОСА (OsaMeetings — создание)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/OsaMeetings.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Форма ОСА | Выбрана | (кнопка заблокирована) | — (нет теста) |
| Начало ГОСА | Не позже завершения | "Дата начала не может быть позже даты завершения" | — (нет теста) |
| Начало ГОСА | В пределах разрешённого окна | Предупреждение: "Начало выходит за разрешённый интервал..." | — (нет теста) |
| Завершение ГОСА | В пределах разрешённого окна | Предупреждение: "Завершение выходит за разрешённый интервал..." | — (нет теста) |
| Дата ОСА | >= 20 дней (ст. 52 208-ФЗ) | Предупреждение: "Дата менее чем через 20 дней..." | — (нет теста) |
| Дата ОСА | >= 40 дней (рекомендация) | Предупреждение: "Дата менее чем через 40 дней..." | — (нет теста) |

---

## Список ОСУ (OsuMeetings — создание)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/OsuMeetings.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Форма ОСУ | Выбрана | (кнопка заблокирована) | [`US022_OsuMeetingTests`](../e2e-tests.md#ооо--сценарии) |
| Начало | Не позже завершения | "Дата начала не может быть позже даты завершения" | [`US022_OsuMeetingTests`](../e2e-tests.md#ооо--сценарии) |
| Начало | В пределах разрешённого окна | Предупреждение: "Дата выходит за разрешённый интервал..." | — (нет теста) |

---

## ОСА — редактирование (OsaMeetingEdit — wizard)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/OsaMeetingEdit.razor`
**Валидатор:** `OsaMeetingValidator.Validate()` + серверная валидация в `SaveEdit()`

### Шаг 0 — Параметры

Те же валидации, что в разделе ОСА выше (акционеры, заочное+ГОСА, год избрания, типы директоров, состав СД).

Unit-тесты: [`OsaMeetingValidatorTests`](../e2e-tests.md#маппинг-us--e2e-класс) (37 тестов)

### Шаг 1 — Протокол

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Протокол подписан | `editProtocolSigned && HasValue` | "Протокол не подписан" | — (нет E2E для wizard) |
| Дата подписания | Не в будущем | "Дата подписания протокола не может быть в будущем" | — (нет E2E для wizard) |
| Дата подписания | >= referenceDate (завершение ОСА) | "Дата подписания не может быть раньше {date}" | — (нет E2E для wizard) |
| Дата подписания | <= referenceDate + 3 дня | "Дата подписания не может быть позднее {date} (3 дня после завершения ОСА)" | — (нет E2E для wizard) |

### Шаг 2 — Состав СД

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| ФИО членов СД | Required для каждого (кроме секретаря/врем. председ.) | (переход блокируется) | — (нет E2E для wizard) |
| Тип участника | Required если типы директоров настроены | `MemberTypeId` обязателен | — (нет E2E для wizard) |
| Дублирование ФИО | Нет дублей среди членов СД | (переход блокируется) | — (нет E2E для wizard) |
| Секретарь = STAFF | Секретарь обязан иметь тип STAFF; остальные - не могут | (переход блокируется) | — (нет E2E для wizard) |
| Врем. председательствующий | Не > 1; если GMS_DECISION - ФИО + тип обязательны | (переход блокируется) | — (нет E2E для wizard) |
| Количество строк | = editBoardMemberNumber и >= editBoardMinNumber | (переход блокируется) | — (нет E2E для wizard) |
| ldapDuplicateWarning | Нет дублей LDAP | (переход блокируется) | — (нет E2E для wizard) |

### Шаг 3 — Файлы ГОСА

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Файлы | Если `editIsGosa && editHasBoard` - хотя бы один файл обязателен | (переход блокируется) | — (нет E2E для wizard) |

### Серверная валидация SaveEdit()

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Количество членов СД = заявленному | "Количество членов СД в списке ({n}) не совпадает с заявленным ({m})" | — (нет E2E для wizard) |
| Количество >= минимального | "Количество членов СД в списке ({n}) меньше законодательного минимума ({m})" | — (нет E2E для wizard) |
| Поля членов СД заполнены | "Для членов СД должны быть заполнены все поля (ФИО, тип участника)" | — (нет E2E для wizard) |
| Дублирование ФИО | "Дублирование ФИО среди членов СД: ..." | — (нет E2E для wizard) |
| Секретарь = STAFF | "Секретарь СД должен иметь тип «Штатный сотрудник»" | — (нет E2E для wizard) |
| Не-секретарь != STAFF | "Тип «Штатный сотрудник» разрешён только для секретаря СД" | — (нет E2E для wizard) |
| Врем. председ. <= 1 | "Временный председательствующий может быть только один" | — (нет E2E для wizard) |
| Врем. председ. заполнен | "Временный председательствующий не заполнен (ФИО, тип участника)" | — (нет E2E для wizard) |
| Файлы ГОСА | "Для ГОСА необходимо прикрепить хотя бы один юридически значимый документ" | — (нет E2E для wizard) |

---

## ОСУ — редактирование (OsuMeetingEdit — wizard)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/OsuMeetingEdit.razor`

Идентичная структура валидаций, что и в OsaMeetingEdit (раздел выше).

---

## Запросы участника (ShareRequestCreate)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Pages/ShareRequestCreate.razor`

| Поле | Правило | Сообщение | Тесты |
|------|---------|-----------|-------|
| Текст требования (коллективный) | Required | "Введите текст требования" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Номер типового устава | Required (для `CHANGE_STANDARD_CHARTER_NUMBER`) | "Выберите номер типового устава" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Описание (OTHER) | Required | "Введите описание требования" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Пункты повестки | Заголовок + описание обязательны | "Введите заголовок пункта {N}" / "Введите описание пункта {N}" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |

---

## Серверные валидации — ShareRequestEndpoints (бизнес-правила)

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Endpoints/ShareRequestEndpoints.cs`

### Общие проверки

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| ЮЛ выбрано | "Юридическое лицо не выбрано" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Тип запроса существует | "Неизвестный тип запроса: {id}" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Тип доступен для ОПФ | "Тип запроса «{name}» не доступен для данного типа организации" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Пользователь привязан | "Пользователь не привязан к участнику экосистемы" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Участник найден | "Не найден участник для текущего пользователя" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Дубль активного | "Уже есть активное требование типа «{name}»" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |

### Отзыв запроса

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Только нотариальные | "Отзыв доступен только для нотариальных оферт" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Не отозван | "Запрос уже отозван" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| <= 24 часа | "Прошло более 24 часов с момента создания" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |

### Поддержка / Отзыв поддержки

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Статус "Сбор поддержек" | "Поддержка доступна только для требований в статусе «Сбор поддержек»" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Дубль поддержки | "Вы уже поддержали это требование" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Отзыв: статус "Сбор поддержек" | "Отзыв доступен только для требований в статусе «Сбор поддержек»" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Отзыв: была поддержка | "Вы не поддерживали это требование" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |

### Решение CEO

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| ACCEPTED или REJECTED | "Решение должно быть ACCEPTED или REJECTED" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| Место для OFFICE | "Для способа 'Ознакомление в офисе' необходимо указать место ознакомления" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |

### Бизнес-правила по типам

| Тип запроса | Правило | Сообщение | Тесты |
|-------------|---------|-----------|-------|
| NOTARY_LIST_MAINTENANCE | Не утверждено | "Ведение списка участников через нотариат уже утверждено" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| PREEMPTIVE_LIST | Преим. право активно | "Преимущественное право не действует" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| EXIT_APPLICATION | Выход в уставе | "Выход из ООО не предусмотрен уставом" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| EXIT_APPLICATION | Доля >= мин. | "Ваша доля ({n}%) ниже минимальной для выхода ({m}%)" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| EXIT_APPLICATION | Доля <= макс. | "Ваша доля ({n}%) выше максимальной для выхода ({m}%)" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| CHANGE_STANDARD_CHARTER_NUMBER | Номер типового сейчас | "Текущий устав не является типовым" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| CHANGE_STANDARD_CHARTER_NUMBER | Номер отличается | "Новый номер типового устава должен отличаться от текущего" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| CONVERT_TO_CUSTOM | Нетиповой сейчас | "Текущий устав уже является нетиповым" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| CONVERT_TO_CUSTOM | Файл устава | "Необходимо приложить файл проекта устава" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| CONVERT_TO_NJSC | Сейчас ООО | "Преобразование в НАО доступно только для ООО" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| CONVERT_TO_PJSC | Сейчас НАО | "Преобразование в ПАО доступно только для НАО" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| CHANGE_CHARTER_PROVISION | Нетиповой устав | "Устав является типовым; используйте требование «Изменить номер...»" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |
| DEMAND_VOSU | Доля >= порога устава | "Доля участника ({n}%) ниже порога устава ({m}%)" | [`US020_ShareRequestTests`](../e2e-tests.md#участники-ооо) |

---

## Серверные валидации — ParticipantEndpoints

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Endpoints/ParticipantEndpoints.cs`

| Endpoint | Правило | Сообщение | Тесты |
|----------|---------|-----------|-------|
| Все endpoints | Пользователь найден по JWT | "Не удалось определить пользователя" | [`US023_ParticipantTests`](../e2e-tests.md#участники-ооо) |
| Все endpoints | Привязка к ЮЛ | "Пользователь не привязан к юридическому лицу" | [`US023_ParticipantTests`](../e2e-tests.md#участники-ооо) |
| Все endpoints | ЮЛ существует | "Юридическое лицо не найдено" | [`US023_ParticipantTests`](../e2e-tests.md#участники-ооо) |
| Все endpoints | Тип ЮЛ = ООО | `Results.Forbid()` + аудит | [`US023_ParticipantTests`](../e2e-tests.md#участники-ооо) |
| Все endpoints | ЮЛ выбрано | "Юридическое лицо не выбрано" | [`US023_ParticipantTests`](../e2e-tests.md#участники-ооо) |
| Загрузка XML | Расширение `.xml` | "XML-файл должен иметь расширение .xml" | [`US023_ParticipantTests`](../e2e-tests.md#участники-ооо) |
| Загрузка подписи | Расширение `.sig`/`.p7s` | "Файл подписи должен иметь расширение .sig или .p7s" | [`US023_ParticipantTests`](../e2e-tests.md#участники-ооо) |

---

## Серверные валидации — ContractEndpoints

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Endpoints/ContractEndpoints.cs`

| Endpoint | Правило | Сообщение | Тесты |
|----------|---------|-----------|-------|
| Поиск | Query не пуст | "Введите ИНН или ОГРН" | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| Поиск (MANAGEMENT_IP) | ИНН ИП: 12 цифр + контрольная сумма | InnIpValidator | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| Поиск (MANAGEMENT_IP) | ОГРНИП: 15 цифр + контрольная сумма | OgrnipValidator | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| Создание | contractType, counterpartyInn, counterpartyName обязательны | "contractType, counterpartyInn, counterpartyName are required" | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| Создание | Роль: LAWYER или CEO+MANAGEMENT | `Results.Forbid()` | [`US024_ContractTests`](../e2e-tests.md#договоры) |
| Создание | ЮЛ выбрано | "Юридическое лицо не выбрано" | [`US024_ContractTests`](../e2e-tests.md#договоры) |

---

## Серверные валидации — AgendaItemEndpoints

**Файл:** `SamorodinkaTech.Fiducia.BoardPortal/Endpoints/AgendaItemEndpoints.cs`

| Endpoint | Правило | Сообщение | Тесты |
|----------|---------|-----------|-------|
| Все | ЮЛ выбрано | "Юридическое лицо не выбрано" | [`US022_OsuMeetingTests`](../e2e-tests.md#ооо--сценарии) |
| Создание | DocumentTypeCode обязателен | "Код типа документа не может быть пустым" | [`US022_OsuMeetingTests`](../e2e-tests.md#ооо--сценарии) |
| Создание | FK DocumentType | "Неизвестный тип документа: {code}" | [`US022_OsuMeetingTests`](../e2e-tests.md#ооо--сценарии) |
| Создание | Дубль NOTARY_LIST | "Ведение списка участников через нотариат уже утверждено" | [`US022_OsuMeetingTests`](../e2e-tests.md#ооо--сценарии) |
| Создание | Совет директоров существует | "Не найден состав Совета директоров" | [`US022_OsuMeetingTests`](../e2e-tests.md#ооо--сценарии) |
| Accept | Статус PENDING | "Невозможно принять: статус «{status}»" | [`US022_OsuMeetingTests`](../e2e-tests.md#ооо--сценарии) |
| Reject | Статус PENDING | "Невозможно отклонить: статус «{status}»" | [`US022_OsuMeetingTests`](../e2e-tests.md#ооо--сценарии) |

---

## Общие компоненты (SharedComponents)

### FileUpload (`src/SharedComponents/Components/FileUpload.razor`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Размер файла > MaxSizeBytes | "Файл слишком большой ({size}). Максимум: {max}." | — (нет теста) |
| Запрещённое расширение | "Загрузка файлов с расширением .{ext} запрещена." | — (нет теста) |
| HTTP-ошибка | "Ошибка загрузки: {message}" | — (нет теста) |

### QrScanButton (`src/SharedComponents/Components/QrScanButton.razor`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Размер > 50 МБ | "Файл слишком большой. Максимум: 50 МБ." | — (нет теста) |
| Недопустимое расширение | "Недопустимое расширение файла: {ext}. Допустимые: {list}" | — (нет теста) |
| HTTP-ошибка | "Ошибка считывания QR-кода ({status})." | — (нет теста) |

---

## Domain-валидаторы (общие)

Используются обоими порталами.

### LegalEntityValidator (`src/Domain/Validation/LegalEntityValidator.cs`)

| Константа | Значение | Назначение |
|-----------|----------|------------|
| `MaxShareholdersForNonPao` | 50 | Макс. акционеров для НАО/ООО |
| `MinElectionYear` | 1990 | Мин. год избрания |
| `MaxElectionYearOffset` | 5 | Макс. смещение года вперёд |
| `MaxStandardCharterNumber` | 36 | Макс. номер типового устава |

Unit-тесты: [`LegalEntityValidatorTests`](../e2e-tests.md#маппинг-us--e2e-класс) (15 тестов)

### OsaMeetingValidator (`src/Domain/Validation/OsaMeetingValidator.cs`)

Валидации ОСА - см. раздел «ОСА — редактирование» выше.

Unit-тесты: [`OsaMeetingValidatorTests`](../e2e-tests.md#маппинг-us--e2e-класс) (37 тестов)

### InnIpValidator (`src/Domain/Validation/InnIpValidator.cs`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Required | "ИНН ИП обязателен" | `US024_ContractTests` |
| 12 цифр | "ИНН ИП должен содержать ровно 12 цифр" | `US024_ContractTests` |
| Контрольная сумма (10-я цифра) | "Неверная контрольная сумма ИНН (10-я цифра)" | `US024_ContractTests` |
| Контрольная сумма (12-я цифра) | "Неверная контрольная сумма ИНН (12-я цифра)" | `US024_ContractTests` |

### OgrnipValidator (`src/Domain/Validation/OgrnipValidator.cs`)

| Правило | Сообщение | Тесты |
|---------|-----------|-------|
| Optional (null = валидно) | — | `US024_ContractTests` |
| 15 цифр | "ОГРНИП должен содержать ровно 15 цифр" | `US024_ContractTests` |
| Контрольная сумма | "Неверная контрольная сумма ОГРНИП" | `US024_ContractTests` |

---

## Источники файлов

- `SamorodinkaTech.Fiducia.BoardPortal/Pages/Login.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/Onboarding.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/Proposal.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/Meetings.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/Committees.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/ContractAdd.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/ContractEdit.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/LegalEntities.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/OsaMeetings.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/OsaMeetingEdit.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/OsuMeetings.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/OsuMeetingEdit.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Pages/ShareRequestCreate.razor`
- `SamorodinkaTech.Fiducia.BoardPortal/Endpoints/ShareRequestEndpoints.cs`
- `SamorodinkaTech.Fiducia.BoardPortal/Endpoints/ParticipantEndpoints.cs`
- `SamorodinkaTech.Fiducia.BoardPortal/Endpoints/ContractEndpoints.cs`
- `SamorodinkaTech.Fiducia.BoardPortal/Endpoints/AgendaItemEndpoints.cs`
- `src/Domain/Validation/LegalEntityValidator.cs`
- `src/Domain/Validation/OsaMeetingValidator.cs`
- `src/Domain/Validation/InnIpValidator.cs`
- `src/Domain/Validation/OgrnipValidator.cs`
- `src/Domain/Validation/OkopfTypeMapper.cs`
- `src/SharedComponents/Components/FileUpload.razor`
- `src/SharedComponents/Components/QrScanButton.razor`
