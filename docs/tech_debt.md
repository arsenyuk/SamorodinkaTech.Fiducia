# Технический долг

## MPI: контроль уникальности УЗ для одного ФЛ

**Статус:** открыто

**Описание:**
Текущая архитектура допускает несколько `users` с одним `mpi_master_id` (например, несколько LDAP-доменов). UNIQUE constraint не применяется.

**Сценарий:**
1. LDAP-синхронизация создаёт УЗ с `mpi_master_id = X` в домене A
2. LDAP-синхронизация создаёт ещё одну УЗ с тем же `mpi_master_id = X` в домене B
3. ЕДИН resolve находит `mpi_master_id = X` в `users` → привязывает к первой найденной УЗ

**Решение:**
- UI для слияния УЗ: выбор основной + перенос ролей/назначений
- Автоматическое обнаружение дубликатов при LDAP-синхронизации
- Приоритет: средний

## MPI: ошибка в доказательствах идентичности

**Статус:** открыто

**Описание:**
Если введены неверные данные ДУЛ/ИНН/СНИЛС → ЕДИН привязывает `mpi_master_id` к другому физическому лицу. Ручное редактирование MasterId запрещено.

**Решение:**
- ЕДИН Steward review: при конфликте в MPI (один MasterId → разные данные) Steward разрешает конфликт
- Fiducia не предоставляет UI для коррекции — проблема решается на стороне MPI
- Приоритет: средний

## Участник теряет доступ при продаже доли

**Статус:** открыто

**Описание:**
Участник получает доступ к Board Portal при определении доли (привязка через ЕДИН → `PARTICIPANT`). При продаже доли участник формально теряет статус участника ООО, но его роль `PARTICIPANT` в системе **не отзывается автоматически**.

**Негативный сценарий:**
1. Участник привязан через ЕДИН → роль `PARTICIPANT` → доступ к «Информирование», «Подача требований», «Просмотр документации»
2. Участник продаёт долю другому лицу
3. Факт продажи отражается в ЕГРЮЛ (запись в Единый государственный реестр юридических лиц)
4. **Проблема:** роль `PARTICIPANT` остаётся у старого владельца → доступ сохраняется

**Решение:**
- Интеграция с ЕГРЮЛ для отслеживания изменений состава участников
- При получении события из ЕГРЮЛ об изменении доли/выбытии участника:
  1. Отозвать роль `PARTICIPANT` у выбывшего участника
  2. Отвязать `BoardParticipant` от `EcosystemParticipant`
  3. Уведомить выбывшего участника о прекращении доступа
- Альтернатива ( interim ): ручная процедура через Admin Console (LE_ADMIN отзывает роль)
- Приоритет: средний

## DaData API: прокси для сведений об организациях и баланса

**Статус:** открыто (реализован stub-прокси)

**Описание:**
Прокси для DaData API с двумя методами:
- `FindCompanyAsync(query)` — поиск организации по ИНН/ОГРН/наименованию
- `GetBalanceAsync(inn)` — запрос финансового баланса (требует тариф «Профессионал»)

**Важно:** данные ЕГРЮЛ в DaData обновляются **до 3 дней**.

**Текущая реализация:**
- `IDaDataApiClient` (Domain) — интерфейс
- `DaDataCompanyInfo`, `DaDataBalance` (Domain) — DTO
- `DaDataOptions` (Infrastructure) — настройки (BaseUrl, ApiKey, SecretKey, Enabled)
- `DaDataApiClient` (Infrastructure) — HTTP-клиент

**Требуется для полной реализации:**
- Audit-декоратор (`AuditDaDataDecorator`)
- DI-регистрация в обоих `Program.cs`
- Конфигурация в `appsettings.json` + `.env`
- Интеграция с UI (Admin Console / Board Portal)
- Кеширование результатов (данные актуальны до 3 дней — нет смысла запрашивать часто)
- Обработка ошибок тарифа (403 при отсутствии доступа к балансам)
- Unit-тесты с mock

**Лицензионные ограничения:**
- `FindCompanyAsync` — доступен на всех тарифах DaData
- `GetBalanceAsync` — требует тариф «Профессионал» или выше
- **Список ГД (руководителей)** — доступен **только на тарифе «Максимальный»**

**Приоритет:** низкий

## SMS: прокси через Платформу рассылок Stream Telecom

**Статус:** открыто (реализован stub-прокси)

**Описание:**
Прокси для отправки SMS через REST API Платформы рассылок (stream-telecom.ru).
Только POST-метод. Base URL: `https://gateway.api.sc/rest/`

**Текущая реализация:**
- `ISmsApiClient` (Domain) — интерфейс
- `SmsSendRequest`, `SmsSendResponse` (Domain) — DTO
- `StreamTelecomOptions` (Infrastructure) — настройки (BaseUrl, Login, Password, DefaultSender, Enabled)
- `StreamTelecomSmsClient` (Infrastructure) — HTTP-клиент: `POST /Send/SendSms/`

**Требуется для полной реализации:**

### Файлы для создания

| # | Файл | Слой | Описание |
|---|------|------|----------|
| 1 | `src/Infrastructure/Auditing/AuditStreamTelecomDecorator.cs` | Infrastructure | Audit-декоратор: логирует каждую SMS-отправку в `ISecurityAuditService` с IP клиента |
| 2 | `src/Infrastructure/Services/StreamTelecomCallbackHandler.cs` | Infrastructure | Обработчик callback_url: принимает POST от Stream Telecom, обновляет статус доставки в БД |
| 3 | `src/Infrastructure/Services/StreamTelecomStopListService.cs` | Infrastructure | Сервис стоп-листа: добавление/удаление/получение номеров через `/sms_black_list/` |
| 4 | `SamorodinkaTech.Fiducia.AdminConsole/Program.cs` | Api | DI-регистрация: `Configure<StreamTelecomOptions>` + `AddScoped<ISmsApiClient>` с обёрткой `AuditStreamTelecomDecorator` |
| 5 | `SamorodinkaTech.Fiducia.BoardPortal/Program.cs` | Api | DI-регистрация (зеркальная к AdminConsole) |
| 6 | `SamorodinkaTech.Fiducia.AdminConsole/appsettings.json` | Config | Секция `"StreamTelecom": { "BaseUrl": "...", "Login": "", "Password": "", "DefaultSender": "", "Enabled": false }` |
| 7 | `SamorodinkaTech.Fiducia.BoardPortal/appsettings.json` | Config | Аналогичная секция |
| 8 | `.env.example` | Config | `STREAMTELECOM__LOGIN=`, `STREAMTELECOM__PASSWORD=`, `STREAMTELECOM__DEFAULTSENDER=`, `STREAMTELECOM__ENABLED=true` |
| 9 | `docs/integration.md` | Docs | Описание интеграции: лицензия, тариф, ограничения |
| 10 | `tests/.../Mocks/MockSmsApiClient.cs` | Tests | Mock для unit-тестов |

### Audit-декоратор

```csharp
// AuditStreamTelecomDecorator: ISmsApiClient → логирует в ISecurityAuditService
// Код действия: "EXTERNAL:StreamTelecom:SmsSend"
// Лог: "SMS → {phone}, sender={sender}, result={success/error}, messageId={id}"
```

### DI-регистрация (в обоих Program.cs)

```csharp
builder.Services.Configure<StreamTelecomOptions>(
    builder.Configuration.GetSection("StreamTelecom"));

if (builder.Configuration.GetValue<bool>("StreamTelecom:Enabled"))
{
    builder.Services.AddScoped<ISmsApiClient>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<StreamTelecomOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<StreamTelecomSmsClient>>();
        var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
        var inner = new StreamTelecomSmsClient(httpClient, logger,
            options.Login, options.Password, options.DefaultSender);
        var auditService = sp.GetRequiredService<ISecurityAuditService>();
        var ipProvider = sp.GetRequiredService<IClientIpProvider>();
        var auditLogger = sp.GetRequiredService<ILogger<AuditStreamTelecomDecorator>>();
        return new AuditStreamTelecomDecorator(inner, auditService, ipProvider, auditLogger);
    });
}
```

### Обработка статусов доставки

Два механизма:
1. **Callback URL** — Stream Telecom отправляет POST на указанный URL со статусом доставки
2. **Polling** — периодический запрос `POST /State/state.php` с `messageId` для получения статуса

Статусы: `-1` (отправлено), `0` (доставлено), `42` (не доставлено), `46` (просрочено), `255` (недоступно)

### Стоп-лист

- `POST /sms_black_list/?add` — добавление номера (phone + block-reason)
- `POST /sms_black_list/?list` — получение списка (постранично, 1000 номеров/страница)
- `POST /sms_black_list/?remove` — удаление номера

### Retry-политика

- Ошибки 5xx / код 8 (GatewayError) / код 9 (InternalServerError) — retry до 3 раз с экспоненциальной задержкой (1с → 2с → 4с)
- Код 10 (Flood SMS) — retry через 60с
- Код 5 (NotEnoughCredits) — без retry,立即 log + уведомление
- Код 4 (UnauthorizedAccess) — без retry,立即 log

**API-методы Stream Telecom REST:**
- `POST /Send/SendSms/` — отправка единичного SMS (реализован)
- `POST /Send/SendBulk/` — массовая отправка (один текст)
- `POST /Send/SendBulkPacket/` — пакетная отправка (разные тексты)
- `POST /State/state.php` — статус сообщения
- `GET /Statistic/` — статистика
- `POST /Statistic/all_stat.php` — детальная статистика
- `POST /Balance/balance.php` — баланс
- `POST /Balance/price_list.php` — список тарифов

**Приоритет:** низкий

## SMTP: настройка режима безопасности подключения

**Статус:** открыто

**Описание:**
`SmtpEmailService.cs:61-63` — хардкод: `UseSsl=false` → `SecureSocketOptions.None`. Нет промежуточного режима `StartTls`.

**Текущая реализация:**
```csharp
var secureSocketOptions = _options.UseSsl
    ? SecureSocketOptions.SslOnConnect
    : SecureSocketOptions.None;
```

**Решение:**
- Добавить enum `SmtpSecurityMode { None, StartTls, SslOnConnect }` в `SmtpOptions`
- Маппинг: `None` → `SecureSocketOptions.None`, `StartTls` → `SecureSocketOptions.StartTls`, `SslOnConnect` → `SecureSocketOptions.SslOnConnect`
- Поле `SecurityMode` в `SmtpOptions` (по умолчанию `SslOnConnect`)
- Миграция: `UseSsl: true` → `SecurityMode: SslOnConnect`, `UseSsl: false` → `SecurityMode: None`
- Убрать deprecated-поле `UseSsl` после миграции

**Файл:** `src/Infrastructure/Services/SmtpEmailService.cs`, `src/Infrastructure/Services/SmtpOptions.cs`

**Приоритет:** низкий
