# Интеграции с внешними системами

> Документирует API-прокси, Docker-демо-стенды и известные ограничения для внешних интеграций проекта.

---

## Известные ограничения: Docker-образы x86_64 на Apple Silicon (M1–M4)

Ряд внешних систем поставляются только в виде linux/amd64 Docker-образов и **не работают** на Mac с ARM-процессорами через эмуляцию (Rosetta 2 / QEMU):

| Система | Образ | Статус на Apple Silicon | Причина |
|---------|-------|------------------------|---------|
| **TrueConf Server** | `trueconf/trueconf-server:stable` | ❌ SIGBUS на `tc_server` | Невыровненный доступ к памяти в ядре видеосервера (72 MB) — не эмулируется Rosetta |
| **OpenLDAP** (`osixia`) | `osixia/openldap:latest` | ❌ init-скрипты падают | `/container/run/startup/slapd` не находит `replication-disable.ldif` и `root-password-change.ldif` — образ несовместим с эмуляцией |

**Решение для разработки**: использовать API-прокси с mock-объектами (все операции покрыты unit-тестами без поднятых серверов).

**Решение для production**: развёртывание на нативном x86_64 Linux (VPS, bare-metal, или CI/CD-раннер).

---

# Интеграция с TrueConf Server

> Внешняя система: **TrueConf Server** (API v4, для v5.5+) — российская платформа видеоконференцсвязи.

---

## Известные ограничения

### Запуск TrueConf Server в Docker на Apple Silicon (M1–M4)

Попытка запуска TrueConf Server через официальный Docker-образ (`trueconf/trueconf-server:stable`, ~1.1 GB, linux/amd64) на macOS с Apple Silicon **неудачна**:

| Режим эмуляции | Результат | Причина |
|----------------|-----------|---------|
| **Rosetta 2** | `tc_server` падает с SIGBUS (Bus error), supervisor переводит процесс в FATAL | Невыровненный доступ к памяти в бинарнике ядра видеосервера (72 MB), не эмулируется Rosetta |
| **QEMU** | Контейнер уходит в restart loop, инициализация не завершается за 90+ сек | Общая нестабильность эмуляции x86_64 на ARM |

**Вывод**: TrueConf Server **не работает в Docker** на Apple Silicon через любую эмуляцию x86_64. Требуется нативный x86_64 Linux (VPS, bare-metal, или Windows/Linux-сервер).

**Альтернативы для локальной разработки**:
- Аренда VPS с Ubuntu x86_64 (300–500 ₽/мес)
- Физический сервер / рабочая станция с x86_64 Linux
- Использование только API-прокси (реализован) — бизнес-логика не требует поднятого сервера для тестов

---

## Обзор

Для обеспечения дистанционного участия членов совета директоров в заседаниях платформа интегрируется с TrueConf Server через REST API v4. Интеграция построена по паттерну **Proxy (API-клиент)** с соблюдением принципов SOLID: интерфейс определён в Domain, реализация — в Infrastructure, что позволяет тестировать бизнес-логику без поднятого сервера TrueConf.

---

## Место в архитектуре

```
┌──────────────┐      ┌─────────────────────┐      ┌─────────────────┐
│  Application │─────▶│  ITrueConfApiClient  │─────▶│  TrueConf Server │
│  (Use Cases) │      │  (Domain Interface)  │      │  (REST API v4)   │
└──────────────┘      └─────────────────────┘      └─────────────────┘
                              ▲
                              │ implements
                      ┌───────┴──────────┐
                      │ TrueConfApiClient │  ← Infrastructure
                      │ (HttpClient)      │
                      └──────────────────┘
```

Интеграция расположена на уровне **Инфраструктуры** в соответствии с чистой архитектурой: внешние зависимости направлены внутрь, бизнес-логика зависит только от абстракции `ITrueConfApiClient`.

---

## Аутентификация

TrueConf Server API v4 использует **OAuth2 (client_credentials)**:

```
POST /oauth2/v1/token
{
    "grant_type": "client_credentials",
    "client_id":     "<oauth_app_id>",
    "client_secret": "<oauth_app_secret>"
}
→ { "access_token": "..." }
```

Токен передаётся query-параметром во все последующие запросы:
```
GET /api/v3.11/conferences?access_token=<token>
```

### Настройка на стороне TrueConf

1. В панели администратора TrueConf (`/admin`):
   - Включить HTTPS.
   - Перейти в **API → OAuth2**.
   - Создать OAuth2-приложение с правами: `conferences`, `groups`, `groups.users`, `users`.
2. Сохранить `client_id` и `client_secret` в конфигурацию Fiducia.

---

## Операции API, используемые в проекте

| Метод | Endpoint | Применение |
|-------|----------|------------|
| `POST` | `/oauth2/v1/token` | Получение токена доступа |
| `POST` | `/api/v3.11/conferences` | Создание ВКС-комнаты для заседания СД |
| `GET` | `/api/v3.11/conferences/{id}` | Проверка статуса конференции, получение ссылки для входа |
| `DELETE` | `/api/v3.11/conferences/{id}` | Удаление устаревших конференций |
| `GET` | `/api/v3.11/conferences?state=stopped&tag=...` | Очистка завершённых заседаний |
| `GET` | `/api/v3.11/users` | Синхронизация списка участников СД с пользователями TrueConf |

---

## Жизненный цикл конференции в Fiducia

```
Создание заседания СД
  │
  ├─ 1. MeetingService.CreateAsync()
  │     └─ Создаёт запись заседания в БД
  │
  ├─ 2. TrueConfApiClient.CreateConferenceAsync()
  │     ├─ display_name: "Заседание СД №42"
  │     ├─ start_time:    дата и время начала
  │     ├─ duration:      плановая длительность
  │     └─ tag:           "board-meeting"
  │     → Возвращает conferenceId и joinLink
  │
  ├─ 3. Сохранить joinLink в заседание
  │
  ├─ 4. NotificationService отправляет директорам
  │     ссылку на ВКС вместе с повесткой
  │
Проведение заседания
  │
  ├─ 5. Директора подключаются по joinLink
  │
  └─ 6. MeetingService завершает заседание

Очистка (фоновая задача)
  │
  └─ 7. TrueConfApiClient.DeleteConferenceAsync()
        или GetStoppedConferencesAsync() → массовое удаление
        через N дней после завершения
```

---

## Структура кода

```
src/Domain/
  Models/TrueConf/
    TrueConfConference.cs            — модель конференции
    TrueConfSchedule.cs              — расписание
    TrueConfUser.cs                  — пользователь
    TrueConfTokenResponse.cs         — ответ OAuth2
    CreateTrueConfConferenceRequest.cs — запрос на создание
  Interfaces/
    ITrueConfApiClient.cs            — интерфейс клиента (абстракция)

src/Infrastructure/
  Services/
    TrueConfApiClient.cs             — реализация через HttpClient

tests/SamorodinkaTech.Fiducia.Tests.Unit/
  Mocks/
    MockTrueConfApiClient.cs         — mock для тестирования
  Services/
    TrueConfApiClientTests.cs        — 11 unit-тестов
```

---

## Конфигурация

В `appsettings.json` (секция `TrueConf`):

```json
{
  "TrueConf": {
    "ServerUrl": "https://video.company.ru",
    "ClientId": "your-oauth-app-id",
    "ClientSecret": "your-oauth-app-secret",
    "DefaultTag": "board-meeting",
    "ConferenceDurationMinutes": 120,
    "RetentionDays": 90
  }
}
```

| Параметр | Описание |
|----------|----------|
| `ServerUrl` | URL TrueConf Server (FQDN или IP) |
| `ClientId` | Идентификатор OAuth2-приложения |
| `ClientSecret` | Секретный ключ OAuth2-приложения |
| `DefaultTag` | Тег для фильтрации конференций СД |
| `ConferenceDurationMinutes` | Длительность ВКС по умолчанию |
| `RetentionDays` | Срок хранения завершённых конференций до удаления |

---

## Регистрация в DI

```csharp
// Program.cs
builder.Services.AddHttpClient<ITrueConfApiClient, TrueConfApiClient>(
    (sp, client) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILogger<TrueConfApiClient>>();
        var serverUrl = config["TrueConf:ServerUrl"]!;
        return new TrueConfApiClient(client, logger, serverUrl);
    });
```

---

## Тестирование

Интеграция тестируется на двух уровнях:

1. **Unit-тесты** (11 тестов) — используют `MockTrueConfApiClient`, проверяют корректность контракта без реального сервера:
   - Создание конференций с проверкой всех полей
   - Уникальность идентификаторов
   - CRUD-операции (включая граничные случаи: не найден, повторное удаление)
   - Фильтрация завершённых заседаний по тегу
   - Имитация отказа сервера (`SimulateFailure = true`)

2. **Интеграционные тесты** (планируются) — подключаются к реальному TrueConf Server в тестовом контуре.

---

## Безопасность

- **HTTPS** обязателен для всех запросов к TrueConf Server.
- OAuth2-токен передаётся только в query-параметрах (спецификация TrueConf API v4).
- `client_secret` хранится в защищённом хранилище (Azure Key Vault / HashiCorp Vault / переменные окружения), не в открытой конфигурации.
- Аудит-лог Fiducia фиксирует факт создания и удаления ВКС-комнат.
- Доступ к API TrueConf ограничен сетью (файрвол/сегментация): только сервер приложений Fiducia.

---

## Зависимости

- **TrueConf Server** v5.5+ (для API v4). Более старые версии используют API v3.
- **Сетевой доступ** от сервера Fiducia к TrueConf Server по HTTPS (порт 443).
- Конфигурация OAuth2-приложения в панели администратора TrueConf (права `conferences`, `groups`, `users`).

---

## Ссылки

- [Документация TrueConf Server API v4](https://developers.trueconf.ru/server-api/ru.html)
- [Примеры API TrueConf (GitHub)](https://github.com/TrueConf/TrueConf-Server-API-examples)
- [OAuth2 в TrueConf Server](https://docs.trueconf.com/server/admin/web-config#oauth2)

---

# Интеграция с МТС Линк

> Внешняя система: **МТС Линк** (API v3, `userapi.mts-link.ru`) — российская платформа для вебинаров, встреч и онлайн-обучения.

---

## Обзор

МТС Линк используется как альтернативный провайдер видеоконференцсвязи для заседаний совета директоров. Интеграция построена по тому же паттерну **Proxy (API-клиент)**, что и TrueConf: интерфейс в Domain, реализация в Infrastructure.

**Ключевое отличие от TrueConf**: МТС Линк использует двухшаговое создание мероприятия — сначала шаблон (Event), затем сама встреча (EventSession).

---

## Место в архитектуре

```
┌──────────────┐      ┌────────────────────┐      ┌──────────────────┐
│  Application │─────▶│  IMtsLinkApiClient  │─────▶│  МТС Линк API v3 │
│  (Use Cases) │      │  (Domain Interface) │      │  userapi.mts-link │
└──────────────┘      └────────────────────┘      └──────────────────┘
                              ▲
                              │ implements
                      ┌───────┴─────────┐
                      │ MtsLinkApiClient │  ← Infrastructure
                      │ (HttpClient)     │
                      └─────────────────┘
```

---

## Аутентификация

МТС Линк API v3 использует авторизацию через **API-ключ** в HTTP-заголовке:

```
x-auth-token: <api_key>
```

Ключ создаётся в личном кабинете: [my.mts-link.ru/business/api](https://my.mts-link.ru/business/api) (доступен на тарифах PRO, Enterprise, Total).

**Важно**: ключ действует от имени создателя организации. Все запросы к API выполняются с его правами. Также поддерживается OAuth2 для ограничения доступа данными конкретного пользователя.

---

## Двухшаговое создание встречи

МТС Линк разделяет сущности **Event** (шаблон) и **EventSession** (конкретная встреча):

```
Шаг 1: POST /v3/events
  ├─ name, accessSettings, type=meeting
  └─ → { eventId: 2356695, link: "..." }

Шаг 2: POST /v3/events/{eventID}/sessions
  ├─ name, startsAtTimestamp
  └─ → { eventSessionId: 2405055, link: "..." }
```

Клиент `MtsLinkApiClient.CreateMeetingAsync()` выполняет оба шага автоматически.

---

## Операции API, используемые в проекте

| Метод | Endpoint | Применение |
|-------|----------|------------|
| `POST` | `/v3/events` | Создание шаблона мероприятия (шаг 1) |
| `POST` | `/v3/events/{id}/sessions` | Создание встречи (шаг 2) |
| `GET` | `/v3/eventsessions/{id}` | Проверка статуса, получение данных |
| `DELETE` | `/v3/eventsessions/{id}` | Удаление встречи |
| `PUT` | `/v3/eventsessions/{id}/start` | Запуск трансляции |
| `PUT` | `/v3/eventsessions/{id}/stop` | Остановка трансляции |
| `POST` | `/v3/eventsessions/{id}/register` | Регистрация участника (директора) |

---

## Жизненный цикл встречи в Fiducia

```
Создание заседания СД
  │
  ├─ 1. MtsLinkApiClient.CreateMeetingAsync()
  │     ├─ POST /v3/events (шаблон)
  │     ├─ POST /v3/events/{id}/sessions (встреча)
  │     └─ → eventSessionId + link
  │
  ├─ 2. Для каждого директора:
  │     └─ RegisterParticipantAsync(eventSessionId, ...)
  │        → персональная ссылка для входа
  │
  ├─ 3. NotificationService отправляет
  │     персональные ссылки директорам
  │
Проведение заседания
  │
  ├─ 4. StartEventSessionAsync() — запуск
  ├─ 5. Директора подключаются по ссылкам
  └─ 6. StopEventSessionAsync() — завершение

Очистка
  │
  └─ 7. DeleteEventSessionAsync(eventSessionId, sendEmail: false)
```

---

## Структура кода

```
src/Domain/
  Models/MtsLink/
    MtsLinkEvent.cs                       — шаблон мероприятия (Event)
    MtsLinkEventSession.cs                — встреча (EventSession)
    MtsLinkParticipation.cs               — участие с персональной ссылкой
    CreateMtsLinkMeetingRequest.cs        — запрос на создание встречи
    RegisterMtsLinkParticipantRequest.cs  — запрос на регистрацию участника
  Interfaces/
    IMtsLinkApiClient.cs                  — интерфейс клиента

src/Infrastructure/
  Services/
    MtsLinkApiClient.cs                   — реализация через HttpClient

tests/SamorodinkaTech.Fiducia.Tests.Unit/
  Mocks/
    MockMtsLinkApiClient.cs               — mock для тестирования
  Services/
    MtsLinkApiClientTests.cs              — 11 unit-тестов
```

---

## Конфигурация

В `appsettings.json` (секция `MtsLink`):

```json
{
  "MtsLink": {
    "BaseUrl": "https://userapi.mts-link.ru",
    "ApiToken": "your-api-token",
    "DefaultType": "meeting",
    "DefaultDuration": "PT1H30M0S",
    "DefaultLang": "RU"
  }
}
```

| Параметр | Описание |
|----------|----------|
| `BaseUrl` | Базовый URL API МТС Линк |
| `ApiToken` | API-ключ из личного кабинета |
| `DefaultType` | Тип мероприятия по умолчанию: `meeting` / `webinar` / `training` |
| `DefaultDuration` | Длительность в формате ISO 8601 (`PT1H30M0S` = 1.5 часа) |
| `DefaultLang` | Язык интерфейса встречи (`RU` / `EN`) |

---

## Регистрация в DI

```csharp
// Program.cs
builder.Services.AddHttpClient<IMtsLinkApiClient, MtsLinkApiClient>(
    (sp, client) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILogger<MtsLinkApiClient>>();
        var baseUrl = config["MtsLink:BaseUrl"]!;
        var apiToken = config["MtsLink:ApiToken"]!;
        return new MtsLinkApiClient(client, logger, baseUrl, apiToken);
    });
```

---

## Тестирование

1. **Unit-тесты** (11 тестов) — используют `MockMtsLinkApiClient`:
   - Создание встречи с проверкой двухшагового процесса
   - Уникальность идентификаторов
   - CRUD встреч (получить/удалить/start/stop)
   - Регистрация одного и нескольких участников с уникальными ссылками
   - Имитация отказа API (`SimulateFailure = true`) — все 6 методов выбрасывают `HttpRequestException`

2. **Интеграционные тесты** (планируются) — с реальным API МТС Линк в тестовом контуре.

---

## Безопасность

- Все запросы выполняются по HTTPS.
- `ApiToken` хранится в защищённом хранилище (переменные окружения / vault).
- Ключ действует от имени создателя организации — доступ включает все мероприятия и файлы всех сотрудников. Для ограниченного доступа используйте OAuth2.
- Аудит-лог Fiducia фиксирует создание, запуск и удаление встреч.
- Сетевой доступ ограничен: только сервер Fiducia → `userapi.mts-link.ru:443`.

---

## Зависимости

- **МТС Линк**: тариф PRO, Enterprise или Total (API-доступ).
- **API-ключ**: создаётся в личном кабинете [my.mts-link.ru/business/api](https://my.mts-link.ru/business/api).
- **Сетевой доступ**: HTTPS к `userapi.mts-link.ru`.

---

## Ссылки

- [МТС Линк API — список методов](https://help.mts-link.ru/article/19615)
- [Интеграция API. С чего начать](https://help.mts-link.ru/article/19686)
- [OAuth интеграции в МТС Линк](https://help.mts-link.ru/article/21115)
- [Создание мероприятия (EventSession)](https://help.mts-link.ru/article/19682)
- [Регистрация участника](https://help.mts-link.ru/article/19671)

---

# Интеграция с LDAP/AD-каталогом

> Внешняя система: **OpenLDAP / Active Directory** — корпоративный каталог пользователей для синхронизации состава Совета директоров и SSO-аутентификации.

---

## Обзор

LDAP-каталог используется как единый источник данных о директорах. При подготовке ОСА администратор выбирает членов СД из каталога, а не вводит данные вручную. Аутентификация через LDAP (SSO) позволяет директорам входить под своими корпоративными учётными записями.

**Ограничение**: Docker-образ `osixia/openldap` не работает на Apple Silicon (см. [Известные ограничения](#известные-ограничения-docker-образы-x86_64-на-apple-silicon-m1m4)). Разработка и тестирование ведутся через mock-объекты.

---

## Место в архитектуре

```
┌──────────────┐      ┌──────────────┐      ┌───────────────────┐
│  Login.razor │─────▶│ IAuthProvider │      │                   │
│  (Admin)     │      │ LdapAuthProv. │─────▶│  LDAP-каталог     │
└──────────────┘      └──────┬───────┘      │  (OpenLDAP / AD)  │
                             │              └───────────────────┘
                    ┌────────▼──────────┐
                    │   ILdapService    │  ← Domain (абстракция)
                    └────────┬──────────┘
                             │ implements
                    ┌────────▼──────────┐
                    │   LdapService     │  ← Infrastructure
                    │ (S.DS.Protocols)  │
                    └───────────────────┘

┌──────────────────┐      ┌──────────────────────────┐
│ Страница СД      │─────▶│ IBoardMemberLdapService   │
│ (Admin Console)  │      │ • GetCandidatesAsync()    │
└──────────────────┘      │ • IsDuplicate()           │
                          │ • FindDuplicates()        │
                          └──────────────────────────┘
```

---

## Аутентификация (SSO)

Настроена через `LdapAuthProvider` — реализует `IAuthProvider`:

1. **Bind**: проверка логина/пароля через `ILDapService.AuthenticateAsync()`
2. **Поиск**: получение `LdapUser` с атрибутами и членством в группах
3. **Роли**: маппинг LDAP-групп на роли Fiducia:
   - `cn=SysAdmins` → `SYS_ADMIN` (доступ в Admin Console)
   - `cn=BoardOfDirectors` → `MEMBER_BOARD` (доступ в Board Portal)
4. **Приоритет**: если пользователь в обеих группах — получает `SYS_ADMIN`

### Включение SSO

```json
// appsettings.json
"Auth": { "Method": "LDAP" },
"Ldap": {
    "Enabled": true,
    "SysAdminGroupDn": "cn=SysAdmins,ou=Groups,dc=bryansk-arsenal,dc=local",
    "BoardGroupDn": "cn=BoardOfDirectors,ou=Groups,dc=bryansk-arsenal,dc=local"
}
```

---

## Синхронизация состава СД

При редактировании Совета директоров (страница «ЮЛ») администратор нажимает «Загрузить из LDAP» — вызывается `IBoardMemberLdapService.GetCandidatesAsync()`, который:

1. Читает членов группы `cn=BoardOfDirectors` из каталога
2. Маппит LDAP-должности на типы `ref_board_member_types`:
   - «Председатель Совета директоров» → `EXECUTIVE`
   - «Независимый директор» → `INDEPENDENT`
   - «Неисполнительный директор» → `NON_EXECUTIVE`
   - «Член Совета директоров» → `STAFF`
3. Возвращает `BoardMemberCandidate` с заполненными полями: логин, ФИО, email, телефон, предложенный тип

### Контроль уникальности

- `IsDuplicate(existingLogins, newLogin)` — проверяет, не назначен ли уже этот LDAP-пользователь
- `FindDuplicates(logins)` — находит все дубликаты в списке перед сохранением

---

## Docker-демо

`docker-compose.ldap.yml` — OpenLDAP с seed-данными (6 директоров ПАО «Брянский арсенал» + администратор). **Не работает на Apple Silicon** — требуется нативный x86_64 Linux.

Структура каталога:
```
dc=bryansk-arsenal,dc=local
├── ou=Users
│   ├── i.ivanov (Председатель СД)
│   ├── p.petrov (Независимый директор)
│   ├── s.sidorov (Неисполнительный директор)
│   ├── k.kuznetsov (Исполнительный директор)
│   ├── a.smirnova (Член СД)
│   ├── f.fedorov (Член СД, миноритарии)
│   └── v.vasilieva (Корп. секретарь → SYS_ADMIN)
└── ou=Groups
    ├── cn=BoardOfDirectors (6 членов)
    └── cn=SysAdmins (v.vasilieva)
```

---

## Структура кода

```
src/Domain/
  Models/Ldap/
    LdapUser.cs                         — пользователь каталога
    BoardMemberCandidate.cs             — кандидат в СД с предложенным типом
  Interfaces/
    ILdapService.cs                     — низкоуровневые LDAP-операции
    IBoardMemberLdapService.cs          — бизнес-уровень (маппинг, уникальность)

src/Infrastructure/
  Services/
    LdapService.cs                      — реализация через S.DS.Protocols
    BoardMemberLdapService.cs           — композиция над ILdapService
  Authentication/
    LdapAuthProvider.cs                 — SSO-провайдер (IAuthProvider)

docker-compose.ldap.yml                 — демо-стенд (только x86_64)
tools/ldap/seed.ldif                    — seed-данные каталога

tests/.../Mocks/
    MockLdapService.cs                  — mock LDAP (in-memory)
    MockBoardMemberLdapService.cs       — mock бизнес-уровня
tests/.../Services/
    LdapServiceTests.cs                 — 11 тестов
    BoardMemberLdapServiceTests.cs      — 12 тестов
```

---

## Конфигурация

```json
{
  "Ldap": {
    "Enabled": false,
    "Server": "localhost",
    "Port": 389,
    "BaseDn": "dc=bryansk-arsenal,dc=local",
    "BindUser": "cn=admin,dc=bryansk-arsenal,dc=local",
    "BindPassword": "admin",
    "BoardGroupDn": "cn=BoardOfDirectors,ou=Groups,dc=bryansk-arsenal,dc=local",
    "SysAdminGroupDn": "cn=SysAdmins,ou=Groups,dc=bryansk-arsenal,dc=local"
  }
}
```

---

## Архитектурное решение

См. [ADR-011: Интеграция с LDAP/AD-каталогом](decision_records/dr_architecture.md#adr-011-интеграция-с-ldapad-каталогом-для-синхронизации-состава-совета-директоров).
