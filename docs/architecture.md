# Архитектура проекта «Цифровой Совет Директоров»

---

## Обзор

Платформа построена по принципам модульной монолитной архитектуры с выделенным ядром бизнес-логики. Система спроектирована для обеспечения юридической значимости решений, полного соответствия корпоративному законодательству РФ и работы в офлайн-режиме.

---

## Архитектурные принципы

| Принцип | Описание |
|---------|----------|
| **Чистая архитектура** | Зависимости направлены внутрь — от инфраструктуры к бизнес-логике |
| **Domain-Driven Design** | Моделирование через домены, ограниченные контексты |
| **Event-Driven** | Асинхронная коммуникация через события |
| **CQRS** | Разделение чтения и записи для оптимизации |
| **SOLID** | Соблюдение всех пяти принципов |
| **Legal-First** | Юридические требования на первом месте |
| **Database-First** | Схема БД ведётся одним каноническим SQL (ADR‑012) |
| **Справочники с префиксом** | Все справочники имеют префикс `ref_` и единый стиль идентификаторов |

---

## Общая архитектура

```mermaid
graph TB
    subgraph "Клиенты"
        WEB[Веб-портал<br/>Board Portal]
        ADMIN[Админ-панель<br/>Admin Console]
    end

    subgraph "API Gateway"
        GW[API Gateway<br/>Rate Limiting<br/>Authentication<br/>JWT + 2FA]
    end

    subgraph "Сервисы"
        AUTH[Auth Service<br/>OAuth2 + ПЭП]
        MEETING[Meeting Service<br/>Заседания]
        VOTING[Voting Service<br/>Голосование]
        COMMITTEE[Committee Service<br/>Комитеты]
        DOC[Document Service<br/>Документооборот]
        NOTIFY[Notification Service<br/>Оповещения]
        AUDIT[Audit Service<br/>Аудит-лог]
    end

    subgraph "Ядро"
        DOMAIN[Доменная модель]
        EVENTS[Event Bus]
        CQRS[CQRS Pipeline]
    end

    subgraph "Инфраструктура"
        DB[(PostgreSQL)]
        CACHE[(Redis)]
        MQ[RabbitMQ]
        STORAGE[File Storage<br/>Документы]
        TSP[TSP Server<br/>Точное время]
        CRYPTO[КриптоПро<br/>УКЭП]
    end

    WEB --> GW
    ADMIN --> GW

    GW --> AUTH
    GW --> MEETING
    GW --> VOTING
    GW --> COMMITTEE
    GW --> DOC

    AUTH --> DOMAIN
    MEETING --> DOMAIN
    VOTING --> DOMAIN
    COMMITTEE --> DOMAIN
    DOC --> DOMAIN

    DOMAIN --> EVENTS
    EVENTS --> NOTIFY
    EVENTS --> AUDIT

    MEETING --> DB
    VOTING --> DB
    COMMITTEE --> DB
    DOC --> DB
    AUDIT --> DB

    VOTING --> CACHE
    MEETING --> CACHE

    EVENTS --> MQ
    NOTIFY --> MQ

    DOC --> STORAGE
    VOTING --> TSP
    VOTING --> CRYPTO
```

---

## Системная архитектура

```mermaid
graph LR
    subgraph "Frontend"
        REACT[React SPA<br/>Board Portal]
        ADMIN_REACT[React SPA<br/>Admin Console]
    end

    subgraph "Backend Services"
        API[API Layer<br/>REST/GraphQL]
        BIZ[Business Layer<br/>Use Cases]
        INFRA[Infrastructure<br/>Repositories]
    end

    subgraph "External"
        TSP_S[TSP Server<br/>RFC 3161]
        CRYPTO_S[КриптоПро<br/>CSP]
        SMTP_S[SMTP Server<br/>Email]
    end

    REACT --> API
    ADMIN_REACT --> API

    API --> BIZ
    BIZ --> INFRA

    INFRA --> TSP_S
    INFRA --> CRYPTO_S
    INFRA --> SMTP_S
```

---

## Структура каталогов

```
SamorodinkaTech.Fiducia/
├── src/
│   ├── Api/                          # REST API (ASP.NET Core)
│   │   ├── Controllers/
│   │   ├── Filters/
│   │   ├── Middleware/
│   │   └── Program.cs
│   ├── Application/                  # Application Layer (Use Cases)
│   │   ├── Contracts/                # Interfaces
│   │   ├── Services/                 # Application Services
│   │   ├── Commands/                 # CQRS Commands
│   │   ├── Queries/                  # CQRS Queries
│   │   └── Validators/              # FluentValidation
│   ├── Domain/                       # Domain Layer (Core)
│   │   ├── Entities/                 # Domain Entities
│   │   ├── ValueObjects/            # Value Objects
│   │   ├── Aggregates/              # Aggregates
│   │   ├── Events/                  # Domain Events
│   │   ├── Exceptions/             # Domain Exceptions
│   │   └── Interfaces/             # Domain Interfaces
│   ├── Infrastructure/              # Infrastructure Layer
│   │   ├── Persistence/             # EF Core, Repositories
│   │   ├── Messaging/               # RabbitMQ, MediatR
│   │   ├── External/                # External Services (TSP, КриптоПро)
│   │   ├── Cache/                   # Redis
│   │   └── Storage/                 # File Storage
│   └── Common/                      # Shared Kernel
│       ├── Primitives/              # Base Classes
│       ├── Extensions/              # Extension Methods
│       └── Constants/              # Constants
├── tests/
│   ├── Unit/                        # Unit Tests
│   ├── Integration/                # Integration Tests
│   └── Functional/                 # Functional Tests
├── docs/                            # Documentation
├── tools/                           # Dev Tools, Scripts
├── .mimocode/                       # AI Assistant Config
├── *.sln                            # Solution Files
└── *.md                             # Documentation
```

---

## Компоненты системы

### 1. API Layer (Presentation)

**Ответственность**: Обработка HTTP-запросов, валидация, авторизация.

**Технологии**: ASP.NET Core 8, MediatR, FluentValidation.

**Ключевые элементы**:
- `Controllers/` — REST API endpoints
- `Filters/` — Action/Exception фильтры
- `Middleware/` — Pipeline middleware

### 2. Application Layer

**Ответственность**: Оркестрация бизнес-операций, транзакции, кросс-модульная коммуникация.

**Паттерны**: CQRS, MediatR, Use Cases.

**Ключевые элементы**:
- `Commands/` — Запись данных (Create, Update, Delete)
- `Queries/` — Чтение данных (Get, Search, Filter)
- `Services/` — Application Services
- `Validators/` — Валидация входных данных

### 3. Domain Layer

**Ответственность**: Бизнес-логика, правила, инварианты.

**Паттерны**: Domain Events, Value Objects, Aggregates.

**Ключевые элементы**:
- `Entities/` — Сущности с уникальным Identity
- `ValueObjects/` — Immutable value types
- `Aggregates/` — Корни агрегации
- `Events/` — Domain Events
- `Exceptions/` — Business Exceptions

### 4. Infrastructure Layer

**Ответственность**: Техническая реализация, доступ к данным, внешние интеграции.

**Технологии**: EF Core, RabbitMQ, Redis, MinIO/S3.

**Ключевые элементы**:
- `Persistence/` — Repositories, EF Core DbContext
- `Messaging/` — Event Bus, Message Handlers
- `External/` — API Clients (TSP, КриптоПро, SMTP)
- `Cache/` — Redis Cache
- `Storage/` — File Storage (MinIO/S3)

---

## Модули системы

### Module: Auth (Авторизация)

**Описание**: Управление пользователями, аутентификация, авторизация.

**Компоненты**:
- `UserService` — Управление пользователями
- `AuthService` — OAuth2 + 2FA
- `PepAgreementService` — Подписание Соглашения о ПЭП
- `RoleService` — RBAC матрица

**Доменная модель**:
```mermaid
classDiagram
    class User {
        +int Id
        +string LastName
        +string FirstName
        +string MiddleName
        +string Email
        +string Phone
        +bool IsExternal
        +bool PepAgreementSigned
        +DateTime? PepSignedAt
    }

    class Role {
        +int Id
        +string RoleCode
        +string RoleName
    }

    class UserRole {
        +int UserId
        +int RoleId
    }

    User "1" --> "*" UserRole
    Role "1" --> "*" UserRole
```

### Module: Meeting (Заседания)

**Описание**: Организация и управление заседаниями совета директоров.

**Компоненты**:
- `MeetingService` — Управление заседаниями
- `AgendaService` — Повестка дня
- `QuorumService` — Контроль кворума
- `NotificationService` — Уведомления о созыве

**Доменная модель**:
```mermaid
classDiagram
    class Meeting {
        +int Id
        +string MeetingNumber
        +MeetingForm MeetingForm
        +MeetingStatus Status
        +DateTime? VotingStartAt
        +DateTime? VotingEndAt
        +int CreatedBy
    }

    class AgendaQuestion {
        +int Id
        +int MeetingId
        +int SequenceNumber
        +string QuestionText
        +string ProposedResolution
        +QuestionStatus Status
    }

    Meeting "1" --> "*" AgendaQuestion
```

### Module: Voting (Голосование)

**Описание**: Электронное голосование с ПЭП/УКЭП.

**Компоненты**:
- `BulletinService` — Бюллетени
- `VoteService` — Подсчёт голосов
- `SignatureService` — Электронные подписи
- `TspService` — Метки времени

**Доменная модель**:
```mermaid
classDiagram
    class Bulletin {
        +int Id
        +int AgendaQuestionId
        +int UserId
        +VoteValue VoteValue
        +string? SpecialOpinion
        +SignatureType SignatureType
        +string SignatureValue
        +DateTime SignedAt
        +bool IsCancelled
        +string? CancellationReason
    }

    class VoteValue {
        <<enumeration>>
        ZA
        PROTIV
        VOZDERZHALSYA
    }

    class SignatureType {
        <<enumeration>>
        PEP
        UKEP
    }

    Bulletin --> VoteValue
    Bulletin --> SignatureType
```

### Module: Committee (Комитеты)

**Описание**: Динамическое управление комитетами совета директоров.

**Компоненты**:
- `CommitteeService` — Управление комитетами
- `CommitteeMemberService` — Члены комитетов
- `CommitteeTaskService` — Поручения комитетам
- `CommitteeMeetingService` — Заседания комитетов

**Доменная модель**:
```mermaid
classDiagram
    class Committee {
        +int Id
        +string Code
        +string Name
        +BehaviorType BehaviorType
        +bool IsActive
        +int? ChairId
        +int? SecretaryId
    }

    class CommitteeMember {
        +int CommitteeId
        +int UserId
    }

    class CommitteeTask {
        +int Id
        +int CommitteeId
        +int? AgendaQuestionId
        +string TaskDescription
        +DateTime DeadlineAt
        +TaskStatus Status
        +int CreatedBy
    }

    class BehaviorType {
        <<enumeration>>
        CONTROL
        STRATEGIC
    }

    Committee --> BehaviorType
    Committee "1" --> "*" CommitteeMember
    Committee "1" --> "*" CommitteeTask
```

### Module: Document (Документооборот)

**Описание**: Генерация и управление документами по ГОСТ Р 7.0.97-2025.

**Компоненты**:
- `ProtocolService` — Протоколы заседаний
- `GostTemplateService` — ГОСТ-шаблоны
- `WatermarkService` — Водяные знаки
- `FileStorageService` — Хранилище файлов

### Module: Audit (Аудит)

**Описание**: Некорректируемый журнал аудита ИБ.

**Компоненты**:
- `AuditLogService` — Журнал аудита
- `SecurityEventService` — События безопасности

---

## Взаимодействие модулей

```mermaid
sequenceDiagram
    participant S as Секретарь
    participant API as API Gateway
    participant MEETING as Meeting Service
    participant VOTING as Voting Service
    participant NOTIFY as Notification Service
    participant DB as Database
    participant AUDIT as Audit Service

    S->>API: POST /api/meetings
    API->>MEETING: CreateMeeting

    MEETING->>DB: Save Meeting
    MEETING->>AUDIT: Log Action

    MEETING->>NOTIFY: SendNotifications
    NOTIFY->>NOTIFY: SendEmail to Directors

    Note over VOTING: Голосование
    VOTING->>API: POST /api/bulletins
    API->>VOTING: SubmitVote

    VOTING->>VOTING: Validate Quorum
    VOTING->>VOTING: Sign with PEP/UKEP
    VOTING->>VOTING: Get TSP Timestamp

    VOTING->>DB: Save Bulletin
    VOTING->>AUDIT: Log Vote

    VOTING->>API: Return Result
    API->>S: 200 OK
```

---

## Поток данных

### Поток организации заседания

```mermaid
flowchart TD
    A[Секретарь создаёт уведомление] --> B[Формирование повестки]
    B --> C[Выбор участников]
    C --> D[Отправка уведомлений]
    D --> E[Ожидание голосования]
    E --> F{Кворум?}
    F -->|Да| G[Подсчёт голосов]
    F -->|Нет| H[Блокировка]
    G --> I[Генерация протокола]
    I --> J[Подписание УКЭП]
    J --> K[Архивирование]
```

### Поток офлайн-голосования

```mermaid
flowchart TD
    A[Директор заходит на ПК] --> B[Скачивание материалов]
    B --> C[Шифрование в IndexedDB]
    C --> D[Голосование оффлайн]
    D --> E[Подпись ПЭП локально]
    E --> F{Есть сеть?}
    F -->|Да| G[Синхронизация с сервером]
    F -->|Нет| H[Ожидание сети]
    H --> F
    G --> I[Проверка UTC-меток]
    I --> J[Сохранение в БД]
```

---

## Требования к производительности

| Метрика | Целевое значение |
|---------|------------------|
| Latency API (p95) | < 200ms |
| Latency API (p99) | < 500ms |
| Throughput | 500 RPS |
| Concurrent Users | 1000 |
| Data Availability | 99.9% |
| Recovery Time Objective | < 15 мин |
| Recovery Point Objective | < 5 мин |

---

## Схема развёртывания

> Ниже приведены два описания: **фактическое** текущее развёртывание (as-is)
> и **целевое** развёртывание для Фазы 3 (to-be), закреплённое решениями
> ADR-001..ADR-007. Их не следует путать — as-is значительно проще и не
> содержит части компонентов, упомянутых в to-be диаграмме.

### Текущая реализация (as-is)

Фактически развёрнуты два независимых Blazor Server приложения и одна БД,
без внешних инфраструктурных сервисов:

```mermaid
graph TB
    subgraph "Локальная разработка / текущий деплой"
        subgraph "AdminConsole (.NET 9, Blazor Server, Kestrel)"
            ADMIN[AdminConsole<br/>http://localhost:5169]
        end

        subgraph "BoardPortal (.NET 9, Blazor Server, Kestrel)"
            BOARD[BoardPortal<br/>http://localhost:5112]
        end

        subgraph "Docker Compose"
            PG[(PostgreSQL 16<br/>fiducia-postgres<br/>port 5434)]
        end
    end

    ADMIN --> PG
    BOARD --> PG
```

**Ключевые отличия от целевой архитектуры**:

| Компонент | Целевая архитектура (to-be) | Фактическое состояние (as-is) |
|-----------|------------------------------|-------------------------------|
| Приложения | API Instance 1/2 за Load Balancer | Два отдельных Blazor Server приложения (AdminConsole, BoardPortal), без балансировщика |
| БД | PostgreSQL Primary + Replica | Один инстанс PostgreSQL 16 (`fiducia-postgres`, порт 5434), без реплики |
| Кэш | Redis Cluster (Primary + Replica) | Отсутствует |
| Message Broker | RabbitMQ (2 узла) | Отсутствует — коммуникация in-process |
| Файловое хранилище | MinIO/S3 | Не реализовано |
| TSP Server | Внешний сервис меток времени | Не реализовано |
| КриптоПро (УКЭП) | Внешняя интеграция | Не реализовано (см. "Известные проблемы" в `AGENTS.md`) |
| Сессии | — | JWT cookie через `ISessionService`, без Redis |

### Целевая архитектура (to-be, Фаза 3)

Ориентир для масштабирования, зафиксированный в ADR-001..ADR-007. Не
описывает текущее развёртывание.

```mermaid
graph TB
    subgraph "Production (Фаза 3, целевое состояние)"
        LB[Load Balancer]
        
        subgraph "App Cluster"
            API1[API Instance 1]
            API2[API Instance 2]
        end

        subgraph "Data Cluster"
            PG[(PostgreSQL Primary)]
            PG1[(PostgreSQL Replica)]
        end

        subgraph "Cache Cluster"
            REDIS1[(Redis Primary)]
            REDIS2[(Redis Replica)]
        end

        subgraph "Message Broker"
            MQ1[RabbitMQ Node 1]
            MQ2[RabbitMQ Node 2]
        end

        subgraph "Storage"
            S3[MinIO/S3]
        end

        subgraph "External"
            TSP_S[TSP Server]
            CRYPTO_S[КриптоПро]
        end
    end

    LB --> API1
    LB --> API2

    API1 --> PG
    API2 --> PG

    PG --> PG1

    API1 --> REDIS1
    API2 --> REDIS1

    REDIS1 --> REDIS2

    API1 --> MQ1
    API2 --> MQ2

    API1 --> S3
    API2 --> S3

    API1 --> TSP_S
    API2 --> TSP_S

    API1 --> CRYPTO_S
    API2 --> CRYPTO_S
```

---

## Бизнес-уровень: трассируемость требований законодательства

Отображение бизнес/юридического уровня ведётся как отдельный слой
трассируемости между нормами федеральных законов и техническими
компонентами системы, зафиксированный в **ADR-008** и подробно раскрытый в
отдельном документе [`docs/legal-traceability.md`](legal-traceability.md).
Здесь приведена сводная таблица соответствия:

| Закон | Требование | Модуль/Компонент | Статус реализации |
|-------|-----------|-------------------|--------------------|
| 208-ФЗ (АО) | Автоматический контроль кворума (ст. 68) | Meeting → `QuorumService` | Реализовано |
| 208-ФЗ (АО) | Фиксация участников в протоколе (ст. 181.2 ГК РФ) | Document → `ProtocolService` | Реализовано |
| 208-ФЗ (АО) | Дедлайн 3 календарных дня после заседания | Meeting → `MeetingService` | Реализовано |
| 14-ФЗ (ООО) | Учёт письменных мнений отсутствующих директоров | Meeting → `MeetingService` | Частично |
| 14-ФЗ (ООО) | Контроль кворума (ст. 32) | Meeting → `QuorumService` | Реализовано |
| 63-ФЗ (ЭП) | Блокировка входа до подписания Соглашения о ПЭП | Auth → `PepAgreementService` | Реализовано |
| 63-ФЗ (ЭП) | УКЭП через КриптоПро | Voting → `SignatureService` | Не реализовано |
| 63-ФЗ (ЭП) | Защита от манипуляций с временем (TSP) | Voting → `TspService` | Не реализовано |
| 152-ФЗ (ПДн) | Некорректируемый аудит-лог | Audit → `AuditLogService` | Реализовано |
| 152-ФЗ (ПДн) | Локализация данных на территории РФ | Infrastructure → PostgreSQL (РФ-хостинг) | Организационно |
| 152-ФЗ (ПДн) | Шифрование чувствительных данных | Domain/Infrastructure | Частично |

Подробное описание методологии ведения этой матрицы, её актуализации при
изменении законодательства и ответственных за каждую строку — в ADR-008 и
в `docs/legal-traceability.md`.
