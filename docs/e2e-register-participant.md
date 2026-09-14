# Регистрация участника в E2E-тестах

> **Связанные документы:** [Сквозные тесты](e2e-tests.md), [Сценарии ЕДИН](e2e-edin-scenarios.md)

## Общая инструкция

Для доступа к страницам Board Portal, требующим роль `PARTICIPANT` (DocumentsCatalog, ShareRequests, Participants и др.), участник **обязан** быть зарегистрирован через Board Portal с ПДн (паспорт + ИНН). Роль `PARTICIPANT` назначается автоматически при ЕДИН-привязке.

**Нельзя** назначить PARTICIPANT через Admin Console — роль не отображается в dropdown (`is_assignable = FALSE`).

---

## Ключевая проблема: mpi_master_id

ЕДИН-привязка связывает `EcosystemParticipant` с `User` через `mpi_master_id` в таблице `users`. Если `mpi_master_id = null`, ЕДИН не может найти User → `User=-` → PARTICIPANT роль **не назначается**.

**Цепочка:**
1. `seed-mpi.sh` → пишет `mpiMasterId` в LDAP ✓
2. `AddEmployeeAsync` → создаёт User **без** `mpiMasterId` ✗
3. ЕДИН binding → ищет User по `mpi_master_id` → не находит → `User=-` ✗
4. PARTICIPANT роль → не назначена ✗

**Решение:** при создании User через `AddEmployeeAsync` **обязательно** заполнять `mpi_master_id` из LDAP (через `seed-mpi.sh` или напрямую).

---

## Паттерн (общий для всех тестов)

### Шаг 1: Admin Console — создание ЮЛ + LE_ADMIN + User участника (с mpi_master_id)

```csharp
// Сидер: создание ЮЛ, ОКОПФ, LE_ADMIN (User + EcosystemParticipant)
await CharterTestSeeder.EnsureSeededAsync(adminPage, charterNumber);

// Создание User для участника в Admin Console
await AdminConsoleHelper.AddEmployeeAsync(
    adminPage,
    participant.LastName, participant.FirstName, participant.MiddleName,
    "Участник", participant.Login,
    CharterTestDataFixed.RoleLeAdmin);

// ВАЖНО: заполнить mpi_master_id из LDAP (seed-mpi.sh должен быть запущен заранее)
// Иначе ЕДИН binding не сможет связать User с EcosystemParticipant
await LdapHelper.SetMpiMasterIdAsync(ldapPage, participant.Login, mpiMasterId);
```

Создаёт:
- ЮЛ в `legal_entities`
- LE_ADMIN в `users` + `user_roles` + `ecosystem_participants`
- User участника в `users` **с mpi_master_id**

### Шаг 2: Board Portal — GD регистрирует участника с ПДн

```csharp
// GD (LE_ADMIN) логинится в Board Portal
var gdLogin = persons.Gd?.Login ?? entity.AdminUser.Login;
await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);

// Поиск существующего EcosystemParticipant по ФИО (создан через Admin Console)
var ecoId = await boardPage.EvaluateAsync<Guid?>(
    $@"async () => {{
        const resp = await fetch('/api/participants/eco-search?name={Uri.EscapeDataString(fullName)}', {{
            credentials: 'same-origin'
        }});
        if (!resp.ok) return null;
        const data = await resp.json();
        return data?.length > 0 ? data[0].id : null;
    }}");

// Регистрация участника с ПДн
var participantId = await BoardPortalHelper.AddParticipantWithPersonalDataAsync(
    boardPage,
    fullName: "Иванов Иван Иванович",
    dulTypeCode: "21",
    dulSeries: "4515",
    dulNumber: "111222",
    personInn: "770123456789",
    participantType: "FL",
    sharePercent: 60m,
    shareAmount: 60000m,
    ecosystemParticipantId: ecoId);
```

Создаёт:
- `board_participant` (BoardParticipant)
- Запускает fire-and-forget ЕДИН-binding

### Шаг 3: Ожидание ЕДИН-привязки → роль PARTICIPANT

```csharp
await EdinTestHelper.WaitForEdinBindingAsync(boardPage, participantId, timeoutSeconds: 15);

var mpiMasterId = await EdinTestHelper.GetParticipantMpiMasterIdAsync(boardPage, participantId);
mpiMasterId.Should().NotBeNull("ЕДИН должен привязать MasterId");
```

После ЕДИН-привязки:
- `ecosystem_participants.mpi_master_id` заполнен
- Роль `PARTICIPANT` назначена в `user_roles`

### Шаг 4: Выход GD → вход участника

```csharp
// Выход GD
await boardPage.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/logout"));

// Вход как PARTICIPANT
await AuthHelper.LoginAsBoardUserAsync(boardPage, participantLogin);
```

---

## Шаблон для нового теста

```csharp
private async Task SetupParticipantAsync(IPage boardPage, int charterNumber)
{
    var entity = CharterTestDataFixed.LegalEntities[charterNumber - 1];
    var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];
    var participant = persons.Participants[0];

    // 1. Admin Console: ЮЛ + LE_ADMIN
    var adminPage = await CreateAdminConsolePageAsync();
    try { await CharterTestSeeder.EnsureSeededAsync(adminPage, charterNumber); }
    finally { await adminPage.CloseAsync(); }

    // 2. GD логинится в Board Portal
    var gdLogin = persons.Gd?.Login ?? entity.AdminUser.Login;
    await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);

    // 3. Поиск EcosystemParticipant
    var ecoId = await boardPage.EvaluateAsync<Guid?>(
        $@"async () => {{
            const resp = await fetch('/api/participants/eco-search?name={Uri.EscapeDataString(participant.FullName)}', {{ credentials: 'same-origin' }});
            if (!resp.ok) return null;
            const data = await resp.json();
            return data?.length > 0 ? data[0].id : null;
        }}");

    // 4. Регистрация с ПДн
    var participantId = await BoardPortalHelper.AddParticipantWithPersonalDataAsync(
        boardPage,
        fullName: participant.FullName,
        dulTypeCode: "21",
        dulSeries: (1000 + charterNumber).ToString(),
        dulNumber: (100000 + charterNumber * 111).ToString(),
        personInn: $"770{charterNumber:D5}000",
        sharePercent: participant.SharePercent,
        ecosystemParticipantId: ecoId);

    // 5. ЕДИН binding
    await EdinTestHelper.WaitForEdinBindingAsync(boardPage, participantId, timeoutSeconds: 15);

    // 6. Выход GD
    await boardPage.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/logout"));
}
```

---

## Действующие лица

| Роль в системе | Как создаётся | Где |
|----------------|---------------|-----|
| LE_ADMIN | Admin Console: `AddEmployeeAsync` | `users` + `user_roles` + `ecosystem_participants` |
| PARTICIPANT | Board Portal: `AddParticipantWithPersonalDataAsync` + ЕДИН | `board_participant` + `user_roles` (автоматически) |
| CEO | Board Portal: вкладка «ГД» | `board_participant.is_general_director` |

---

## Частые ошибки

| Ошибка | Причина | Решение |
|--------|---------|---------|
| «Пользователь не найден» | User не создан в `users` таблице | Добавить `AddEmployeeAsync` (LE_ADMIN) перед регистрацией |
| «У вас нет доступа к этой странице» | Роль PARTICIPANT не назначена | Проверить `mpi_master_id` в User + ЕДИН-привязку |
| ЕДИН binding: `User=- (none)` | `mpi_master_id = null` в User | Заполнить `mpi_master_id` из LDAP/SQL |
| `AddEmployeeAsync` timeout | dropdown содержит только `is_assignable=TRUE` роли | Для PARTICIPANT — Board Portal, не Admin Console |

---

## Файлы

| Файл | Назначение |
|------|-----------|
| `tests/.../Helpers/CharterTestSeeder.cs` | Сидирование ЮЛ + LE_ADMIN (Admin Console) |
| `tests/.../Helpers/BoardPortalHelper.cs` | Регистрация участника через Board Portal API |
| `tests/.../Helpers/EdinTestHelper.cs` | Ожидание ЕДИН-привязки |
| `tests/.../Helpers/AuthHelper.cs` | Логин/выход |
