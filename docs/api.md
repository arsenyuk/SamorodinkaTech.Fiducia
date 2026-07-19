# API Reference

> ⚠️ **Статус: целевой документ (Фаза 3).** Описанный ниже REST API **не реализован** в текущей версии. Система работает на Blazor Server — взаимодействие клиент-сервер происходит через SignalR-соединение, без REST-эндпоинтов. Документ сохранён как проектная спецификация для будущей реализации API-слоя.

---

## Обзор

Платформа «Цифровой Совет Директоров» предоставляет REST API для взаимодействия с веб-интерфейсами Board Portal и Admin Console.

**Base URL**: `https://board.samorodinkatech.ru/api/v1`

**Content-Type**: `application/json`

---

## Аутентификация

### OAuth2 + JWT Bearer Token

```http
Authorization: Bearer <token>
```

### Получение токена

```http
POST /api/v1/auth/token
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "secure_password",
  "totp_code": "123456"
}
```

**Ответ**:
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "user_id": "550e8400-e29b-41d4-a716-446655440000",
  "roles": ["CHAIR_BOARD"]
}
```

### Проверка ПЭП (SMS)

```http
POST /api/v1/auth/pep/verify
Content-Type: application/json

{
  "user_id": 123,
  "sms_code": "123456"
}
```

---

## User API

### Регистрация пользователя

```http
POST /api/v1/users
Authorization: Bearer <token>
Content-Type: application/json

{
  "last_name": "Иванов",
  "first_name": "Иван",
  "middle_name": "Иванович",
  "email": "ivanov@example.com",
  "phone": "+79001234567",
  "is_external": false,
  "roles": ["MEMBER_BOARD"]
}
```

**Ответ** (201 Created):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "last_name": "Иванов",
  "first_name": "Иван",
  "email": "ivanov@example.com",
  "created_at": "2026-01-15T10:30:00Z"
}
```

### Получение списка пользователей

```http
GET /api/v1/users?page=1&size=20&is_external=false
Authorization: Bearer <token>
```

**Ответ** (200 OK):
```json
{
  "items": [
    {
      "id": 456,
      "last_name": "Иванов",
      "first_name": "Иван",
      "email": "ivanov@example.com",
      "is_external": false,
      "pep_agreement_signed": true
    }
  ],
  "total": 50,
  "page": 1,
  "size": 20
}
```

---

## Meeting API

### Создание заседания

```http
POST /api/v1/meetings
Authorization: Bearer <token>
Content-Type: application/json

{
  "meeting_form": "OCHN",
  "meeting_number": "СД-2026-001",
  "voting_start_at": "2026-02-01T10:00:00Z",
  "voting_end_at": "2026-02-04T10:00:00Z",
  "participant_ids": [1, 2, 3, 4, 5]
}
```

**Ответ** (201 Created):
```json
{
  "id": 789,
  "meeting_number": "СД-2026-001",
  "meeting_form": "OCHN",
  "status": "DRAFT",
  "voting_end_at": "2026-02-04T10:00:00Z",
  "created_at": "2026-01-15T10:30:00Z"
}
```

### Получение статуса заседания

```http
GET /api/v1/meetings/{id}
Authorization: Bearer <token>
```

**Ответ** (200 OK):
```json
{
  "id": 789,
  "meeting_number": "СД-2026-001",
  "meeting_form": "OCHN",
  "status": "VOTING",
  "quorum_achieved": true,
  "participants_count": 7,
  "voted_count": 5,
  "voting_end_at": "2026-02-04T10:00:00Z",
  "questions": [
    {
      "id": 1,
      "sequence_number": 1,
      "question_text": "Об утверждении годового отчёта",
      "status": "VOTED"
    }
  ]
}
```

### Проверка кворума

```http
GET /api/v1/meetings/{id}/quorum
Authorization: Bearer <token>
```

**Ответ** (200 OK):
```json
{
  "meeting_id": 789,
  "total_board_members": 7,
  "participated": 5,
  "quorum_achieved": true,
  "quorum_percentage": 71.4,
  "required_percentage": 50
}
```

---

## Voting API

### Создание вопроса повестки

```http
POST /api/v1/meetings/{meeting_id}/questions
Authorization: Bearer <token>
Content-Type: application/json

{
  "sequence_number": 1,
  "question_text": "Об утверждении годового отчёта за 2025 год",
  "proposed_resolution": "Утвердить годовой отчёт АО «Ромашка» за 2025 год"
}
```

**Ответ** (201 Created):
```json
{
  "id": 1,
  "meeting_id": 789,
  "sequence_number": 1,
  "question_text": "Об утверждении годового отчёта за 2025 год",
  "status": "PENDING"
}
```

### Подача голоса

```http
POST /api/v1/bulletins
Authorization: Bearer <token>
Content-Type: application/json

{
  "agenda_question_id": 1,
  "vote_value": "ZA",
  "signature_type": "PEP",
  "signature_value": "hash_of_signature"
}
```

**Ответ** (201 Created):
```json
{
  "id": 101,
  "agenda_question_id": 1,
  "user_id": 456,
  "vote_value": "ZA",
  "signature_type": "PEP",
  "signed_at": "2026-02-01T10:15:00Z",
  "status": "RECORDED"
}
```

### Отмена голоса

```http
DELETE /api/v1/bulletins/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "cancellation_reason": "Обнаружена ошибка в подсчёте"
}
```

**Ответ** (200 OK):
```json
{
  "id": 101,
  "status": "CANCELLED",
  "cancellation_reason": "Обнаружена ошибка в подсчёте",
  "cancelled_at": "2026-02-01T10:20:00Z"
}
```

### Получение результатов голосования

```http
GET /api/v1/meetings/{meeting_id}/results
Authorization: Bearer <token>
```

**Ответ** (200 OK):
```json
{
  "meeting_id": 789,
  "questions": [
    {
      "id": 1,
      "question_text": "Об утверждении годового отчёта",
      "results": {
        "za": 5,
        "protiv": 0,
        "vozderzhalsya": 2,
        "total": 7
      },
      "decision": "Принято",
      "quorum_achieved": true
    }
  ]
}
```

---

## Committee API

### Создание комитета

```http
POST /api/v1/committees
Authorization: Bearer <token>
Content-Type: application/json

{
  "code": "AUDIT",
  "name": "Аудиторский комитет",
  "behavior_type": "CONTROL",
  "chair_id": 456,
  "secretary_id": 789,
  "member_ids": [456, 789, 101]
}
```

**Ответ** (201 Created):
```json
{
  "id": 1,
  "code": "AUDIT",
  "name": "Аудиторский комитет",
  "behavior_type": "CONTROL",
  "is_active": true,
  "chair_id": 456,
  "secretary_id": 789,
  "created_at": "2026-01-15T10:30:00Z"
}
```

### Назначение поручения комитету

```http
POST /api/v1/committees/{id}/tasks
Authorization: Bearer <token>
Content-Type: application/json

{
  "agenda_question_id": 1,
  "task_description": "Провести аудит финансовой отчётности",
  "deadline_at": "2026-03-01T10:00:00Z"
}
```

**Ответ** (201 Created):
```json
{
  "id": 1,
  "committee_id": 1,
  "task_description": "Провести аудит финансовой отчётности",
  "deadline_at": "2026-03-01T10:00:00Z",
  "status": "IN_WORK",
  "created_by": 456,
  "created_at": "2026-01-15T10:30:00Z"
}
```

### Получение статуса поручений

```http
GET /api/v1/committees/{id}/tasks
Authorization: Bearer <token>
```

**Ответ** (200 OK):
```json
{
  "committee_id": 1,
  "tasks": [
    {
      "id": 1,
      "task_description": "Провести аудит финансовой отчётности",
      "deadline_at": "2026-03-01T10:00:00Z",
      "status": "IN_WORK",
      "overdue": false
    }
  ],
  "active_count": 1,
  "completed_count": 3
}
```

---

## Document API

### Генерация протокола

```http
POST /api/v1/meetings/{meeting_id}/protocol
Authorization: Bearer <token>
Content-Type: application/json

{
  "template": "standard",
  "language": "ru"
}
```

**Ответ** (202 Accepted):
```json
{
  "protocol_id": "protocol-guid",
  "status": "generating",
  "estimated_time_seconds": 30
}
```

### Скачивание протокола

```http
GET /api/v1/documents/{document_id}/download
Authorization: Bearer <token>
```

**Ответ** (200 OK):
- Content-Type: application/pdf
- Content-Disposition: attachment; filename="protocol.pdf"

### Получение списка документов

```http
GET /api/v1/meetings/{meeting_id}/documents
Authorization: Bearer <token>
```

**Ответ** (200 OK):
```json
{
  "documents": [
    {
      "id": "doc-guid",
      "type": "protocol",
      "filename": "protocol.pdf",
      "status": "ready",
      "download_url": "/api/v1/documents/doc-guid/download"
    }
  ]
}
```

---

## Audit API

### Получение журнала аудита

```http
GET /api/v1/audit?from=2026-01-01&to=2026-01-31&action_code=LOGIN_SUCCESS&page=1&size=50
Authorization: Bearer <token>
```

**Ответ** (200 OK):
```json
{
  "items": [
    {
      "id": 12345,
      "user_id": 456,
      "user_ip": "192.168.1.100",
      "action_code": "LOGIN_SUCCESS",
      "entity_name": "users",
      "entity_id": 456,
      "description": "Успешная авторизация",
      "log_timestamp": "2026-01-15T10:30:00Z"
    }
  ],
  "total": 150,
  "page": 1,
  "size": 50
}
```

---

## Коды ошибок

| HTTP | Описание | Решение |
|------|----------|---------|
| 400 | Bad Request | Проверьте формат данных |
| 401 | Unauthorized | Проверьте токен |
| 403 | Forbidden | Нет прав доступа |
| 404 | Not Found | Ресурс не найден |
| 409 | Conflict | Дубликат операции |
| 422 | Validation Error | Ошибка валидации полей |
| 429 | Too Many Requests | Превышен лимит запросов |
| 500 | Internal Server Error | Повторите запрос или обратитесь в поддержку |

### Формат ошибки

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid request data",
    "details": [
      {
        "field": "voting_end_at",
        "message": "Voting end date must be at least 3 days after meeting"
      }
    ]
  }
}
```

---

## Лимиты

| Ресурс | Лимит |
|--------|-------|
| API Requests | 1000/мин |
| File Upload | 100 МБ |
| Concurrent Connections | 100 |
| Voting Tokens | 10/час |

---

## Версионирование

API использует версионирование через URL path: `/v1/`, `/v2/`.

Текущая стабильная версия: **v1**

**Политика**:
- Мажорные версии: 12 месяцев поддержки
- Минорные версии: обратно совместимые изменения
- Патчи: исправления без изменения контракта
