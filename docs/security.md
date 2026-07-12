# Вопросы безопасности

---

## Обзор

Платформа «Цифровой Совет Директоров» обрабатывает юридически значимые решения и персональные данные членов совета директоров. Безопасность является приоритетом на всех уровнях системы.

---

## Стандарты и compliance

| Стандарт | Описание | Статус |
|----------|----------|--------|
| **152-ФЗ** | Защита персональных данных | ✅ Соответствие |
| **63-ФЗ** | Электронная подпись | ✅ Соответствие |
| **208-ФЗ** | Акционерные общества | ✅ Соответствие |
| **14-ФЗ** | Общества с ограниченной ответственностью | ✅ Соответствие |
| **ISO 27001** | Управление информационной безопасностью | В процессе |

---

## Аутентификация и авторизация

### OAuth2 + JWT Bearer Tokens

```csharp
// Генерация токена
public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    /// <summary>
    /// Генерирует JWT access token для аутентифицированного пользователя.
    /// </summary>
    public string GenerateAccessToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

#### Формат имени файла аудита

Поле `SecurityAudit:FileNameFormat` поддерживает составные шаблоны даты/времени в фигурных скобках. Любая подстрока в `{...}` трактуется как стандартный формат `DateTime` и подставляется по времени UTC.

- Примеры:
  - `audit-{yyyy-MM}.log` → `audit-2026-07.log`
  - `audit-{yyyy-MM-dd}.log` → `audit-2026-07-12.log`
  - `audit-{yyyy-MM-dd_HH}.log` → `audit-2026-07-12_14.log`

Замечания:
- Используется `UTC` для консистентности во всех средах.
- Ротация файла происходит за счёт изменения имени по шаблону; период ротации фактически зависит от выбранного формата (месяц/день/час и т.д.).
- В среде разработки для Admin Console и Board Portal по умолчанию используется `"./logs/audit"` как `StoragePath`.

### RBAC (Role-Based Access Control)

| Роль | Описание | Разрешения |
|------|----------|------------|
| `SYS_ADMIN` | Системный администратор | Полный доступ к Admin Console |
| `CORP_SECRETARY` | Корпоративный секретарь | Организация заседаний, документы |
| `CHAIR_BOARD` | Председатель СД | Подписание протоколов, управление |
| `MEMBER_BOARD` | Член СД | Голосование, просмотр |
| `EXTERNAL_DIRECTOR` | Внешний/Независимый директор | Голосование, отчёты |
| `SHAREHOLDER` | Акционер | Требования о созыве |
| `COMMITTEE_CHAIR` | Председатель комитета | Управление комитетом |
| `COMMITTEE_MEMBER` | Член комитета | Участие в комитете |

### ПЭП (Простая электронная подпись)

```csharp
// Валидация SMS-кода для ПЭП
public class PepValidationService : IPepValidationService
{
    /// <summary>
    /// Проверяет SMS-код для подписания Соглашения о ПЭП.
    /// Блокировка первого входа до подписания Соглашения.
    /// </summary>
    public async Task<bool> ValidatePepCodeAsync(int userId, string smsCode)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        // Проверка SMS-кода
        var isValid = await _smsService.VerifyCodeAsync(user.Phone, smsCode);
        if (!isValid) return false;

        // Обновление статуса подписания
        user.PepAgreementSigned = true;
        user.PepSignedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Логирование в аудит
        await _auditLogService.LogAsync(userId, "PEP_AGREEMENT_SIGNED", "users", userId);

        return true;
    }
}
```

### УКЭП (Усиленная квалифицированная электронная подпись)

```csharp
// Интеграция с КриптоПро
public class UkepService : IUkepService
{
    /// <summary>
    /// Верифицирует электронную подпись через КриптоПро CSP.
    /// Используется для Председателя и Секретаря СД.
    /// </summary>
    public async Task<bool> VerifySignatureAsync(byte[] document, byte[] signature)
    {
        // Интеграция с КриптоПро ЭЦП Browser plug-in
        // Проверка сертификата в локальном хранилище
        var isValid = await _cryptoproProvider.VerifyAsync(document, signature);
        return isValid;
    }
}
```

---

## Защита данных

### Шифрование

#### At Rest

```csharp
// Шифрование чувствительных полей
public class EncryptedString : ValueObject
{
    private readonly byte[] _encryptedData;
    private readonly byte[] _iv;

    /// <summary>
    /// Создаёт зашифрованную строку с использованием AES-256.
    /// </summary>
    public static EncryptedString Create(string plainText, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var encrypted = encryptor.TransformFinalBlock(
            Encoding.UTF8.GetBytes(plainText), 0, plainText.Length);

        return new EncryptedString(encrypted, aes.IV);
    }

    /// <summary>
    /// Расшифровывает строку.
    /// </summary>
    public string Decrypt(byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(_encryptedData, 0, _encryptedData.Length);

        return Encoding.UTF8.GetString(decrypted);
    }
}
```

#### In Transit

- TLS 1.3 для всех соединений
- HSTS (HTTP Strict Transport Security)
- Certificate Pinning для веб-приложения

### PII (Personally Identifiable Information)

| Тип данных | Шифрование | Хранение | Доступ |
|------------|-----------|----------|--------|
| ФИО | AES-256 | PostgreSQL + шифрование | Auth Service |
| Email | AES-256 | PostgreSQL + шифрование | Auth Service |
| Телефон | AES-256 | PostgreSQL + шифрование | Auth Service |
| Электронные подписи | Hash + шифрование | Отдельное хранилище | Voting Service |

---

## Защита API

### Rate Limiting

```csharp
// Конфигурация rate limiting
public class RateLimitingConfig
{
    /// <summary>
    /// Определяет лимиты запросов для разных клиентов.
    /// </summary>
    public static void Configure(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
            });

            options.AddSlidingWindowLimiter("sliding", limiterOptions =>
            {
                limiterOptions.PermitLimit = 1000;
                limiterOptions.Window = TimeSpan.FromHours(1);
                limiterOptions.SegmentsPerWindow = 6;
            });
        });
    }
}
```

### Input Validation

```csharp
// Валидация входных данных
public class CreateMeetingCommandValidator : AbstractValidator<CreateMeetingCommand>
{
    /// <summary>
    /// Проверяет корректность входных данных для создания заседания.
    /// </summary>
    public CreateMeetingCommandValidator()
    {
        RuleFor(x => x.MeetingForm)
            .IsInEnum()
            .WithMessage("Invalid meeting form");

        RuleFor(x => x.VotingEndAt)
            .GreaterThan(DateTime.UtcNow.AddDays(3))
            .WithMessage("Voting end date must be at least 3 days after creation");

        RuleFor(x => x.ParticipantIds)
            .NotEmpty()
            .WithMessage("At least one participant is required");
    }
}
```

### SQL Injection Prevention

```csharp
// Использование параметризованных запросов
public class MeetingRepository : IMeetingRepository
{
    /// <summary>
    /// Ищет заседание по номеру (параметризованный запрос).
    /// </summary>
    public async Task<Meeting> FindByNumberAsync(string meetingNumber)
    {
        return await _context.Meetings
            .FromSqlRaw("SELECT * FROM meetings WHERE meeting_number = {0}", meetingNumber)
            .FirstOrDefaultAsync();
    }
}
```

---

## Аудит и логирование

### Audit Trail

```csharp
// Аудит всех изменений
public class AuditInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Перехватывает все изменения в базе данных и записывает аудит.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return ValueTask.FromResult(result);

        var auditEntries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable auditable)
            {
                var auditEntry = new AuditEntry
                {
                    EntityType = entry.Entity.GetType().Name,
                    EntityId = GetPrimaryKeyValue(entry),
                    Action = entry.State.ToString(),
                    Timestamp = DateTime.UtcNow,
                    UserId = _currentUserService.UserId
                };

                auditEntries.Add(auditEntry);
            }
        }

        context.Set<AuditEntry>().AddRange(auditEntries);
        return ValueTask.FromResult(result);
    }
}
```

### Security Logging

```csharp
// Логирование событий безопасности
public class SecurityLogger : ISecurityLogger
{
    /// <summary>
    /// Логирует попытки несанкционированного доступа.
    /// </summary>
    public void LogUnauthorizedAccess(string userId, string resource, string ipAddress)
    {
        _logger.LogWarning(
            "Unauthorized access attempt: User={UserId}, Resource={Resource}, IP={IpAddress}, Time={Time}",
            userId, resource, ipAddress, DateTime.UtcNow);
    }

    /// <summary>
    /// Логирует успешную аутентификацию.
    /// </summary>
    public void LogSuccessfulAuthentication(string userId, string ipAddress)
    {
        _logger.LogInformation(
            "Successful authentication: User={UserId}, IP={IpAddress}, Time={Time}",
            userId, ipAddress, DateTime.UtcNow);
    }
}
```

---

## Управление секретами

### HashiCorp Vault

```csharp
// Получение секретов из Vault
public class VaultSecretProvider : ISecretProvider
{
    /// <summary>
    /// Получает секрет по ключу из HashiCorp Vault.
    /// </summary>
    public async Task<string> GetSecretAsync(string key)
    {
        var client = new VaultClient(VaultClientSettings);
        var secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            path: key,
            mountPoint: "fiducia");

        return secret.Data.Data["value"].ToString();
    }
}
```

### Environment Variables

```env
# Не коммитить в репозиторий!
DATABASE_URL=postgresql://user:password@host:5432/dbname
REDIS_URL=redis://:password@host:6379
JWT_SECRET=your-super-secret-key
API_KEY=your-api-key
```

### .gitignore

```gitignore
# Секреты
*.env
*.env.local
secrets.json
appsettings.*.Local.json

# Ключи
*.pem
*.key
*.pfx
```

---

## Уязвимости и зависимости

### Автоматическая проверка

```bash
# Проверка зависимостей
dotnet list package --vulnerable

# Исправление
dotnet list package --outdated
dotnet add package <Package> --version <Latest>
```

### OWASP Top 10

| # | Уязвимость | Защита |
|---|------------|--------|
| A01 | Broken Access Control | RBAC + Authorization Policies |
| A02 | Cryptographic Failures | AES-256 + TLS 1.3 |
| A03 | Injection | Parameterized Queries + ORM |
| A04 | Insecure Design | Threat Modeling |
| A05 | Security Misconfiguration | Configuration Validation |
| A06 | Vulnerable Components | Dependency Scanning |
| A07 | Auth Failures | MFA + Rate Limiting |
| A08 | Data Integrity Failures | Digital Signatures |
| A09 | Logging Failures | Structured Logging + SIEM |
| A10 | SSRF | Input Validation + Whitelisting |

---

## УПД.15: Принудительное закрытие сессии (разлогинивание)

**Мера защиты персональных данных** в соответствии с требованиями 152-ФЗ.

### Назначение

Автоматическое завершение сессии пользователя при бездействии для предотвращения
несанкционированного доступа к персональным данным оставшейся без присмотра рабочей станции.

### Реализация

| Компонент | Описание |
|-----------|----------|
| **JWT cookie** | Серверный токен с временной меткой истечения (HMAC-SHA256) |
| **Idle-таймер** | JavaScript отслеживает активность: mousemove, keydown, click, scroll, touchstart |
| **Endpoint /api/session/logout** | Всегда возвращает HTTP 200 OK (даже при невалидных запросах) |
| **Конфигурация** | `Session:IdleTimeoutMinutes` в `appsettings.json` (дефолт: 5 минут) |

### Поток работы

1. Пользователь входит в систему → `Login.razor` вызывает `ISessionService.GenerateToken()` → JWT устанавливается в cookie
2. `MainLayout.razor` запрашивает `GET /api/session/config` → получает timeout → запускает `idleTimer.start(N)`
3. При каждой активности пользователя таймер сбрасывается
4. При бездействии > N минут → `idleTimer.logout()` → `POST /api/session/logout` → очистка localStorage и cookie → редирект на `/login`
5. Ручной выход → аналогичный процесс через `Logout.razor`

### Конфигурация

```json
{
  "Session": {
    "IdleTimeoutMinutes": 5,
    "JwtSecret": "<256-bit secret key>"
  }
}
```

Значение `IdleTimeoutMinutes` передаётся клиенту при старте сессии через `GET /api/session/config`.

### Ключевые особенности

- Endpoint выхода **всегда** возвращает 200 OK — клиент не зависает в неопределённом состоянии
- JWT-токен имеет срок жизни, равный timeout бездействия
- Таймер отслеживает 5 типов активности для корректного сброса
- Реализация для обоих порталов: Board Portal и Admin Console

---

## РСБ.2: Централизованная регистрация событий безопасности

**Мера защиты** в соответствии с требованиями безопасности信息系统 (ИБ).

### Назначение

Централизованная регистрация событий безопасности (вход, выход, доступ, голосование) в обособленном хранилище для обеспечения некорректируемости и аудита.

### Реализация

| Компонент | Описание |
|-----------|----------|
| **ISecurityAuditService** | Интерфейс сервиса регистрации событий безопасности |
| **SecurityAuditService** | Реализация: запись в PostgreSQL + файловый лог |
| **SecurityAuditFileWriter** | Роллинг-файловый логгер для аудита |
| **ApplicationLogWriter** | Роллинг-файловый логгер для событий приложений |
| **Audit.razor** | Просмотр журнала аудита (последние 100 записей) |

### События аудита

| Код события | Описание | Триггер |
|-------------|----------|---------|
| `LOGIN_SUCCESS` | Успешный вход | `Login.razor` |
| `LOGIN_FAILED` | Неудачная попытка входа | `Login.razor` |
| `LOGOUT` | Выход пользователя | `Logout.razor` |
| `ACCESS_DENIED` | Доступ запрещён | `Login.razor` |
| `VOTE_SUBMIT` | Голосование | `Voting.razor` |
| `DOCUMENT_VIEW` | Просмотр документа | `Documents.razor` |

---

## РСБ.3: Сбор, запись и хранение информации о событиях безопасности

**Мера защиты** в соответствии с требованиями безопасности信息系统 (ИБ).

### Назначение

Сбор, запись и хранение информации о событиях безопасности в течение установленного времени хранения. Хранение в обособленном хранилище, физически отделённом от ИСПДн.

### Два потока логирования

```
┌─────────────────────────────────────────────────────────────┐
│                    События аудита (безопасность)             │
│                                                             │
│  Login.razor ─┐                                             │
│  Logout.razor ┼──► ISecurityAuditService ──► PostgreSQL     │
│  Middleware ──┘    (security_audit_log)     + файлы .log    │
│                                                             │
│  Хранение: PostgreSQL + файлы в изолированной директории    │
│  Формат: JSON (structured)                                  │
│  Ретеншн: настраиваемый (по умолчанию 365 дней)             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                События приложений (логи сайтов)              │
│                                                             │
│  Program.cs ──► ILogger<T> ──► RollingFile / Console        │
│                                                             │
│  Хранение: файлы в настраиваемой директории                 │
│  Формат: текстовый (default) или JSON                       │
│  Роллинг: по размеру или по времени                         │
└─────────────────────────────────────────────────────────────┘
```

### Конфигурация

```json
{
  "SecurityAudit": {
    "Enabled": true,
    "StoragePath": "/var/log/fiducia/audit",
    "FileNameFormat": "audit-{yyyy-MM}.log",
    "RotationPeriod": "Monthly",
    "RetentionDays": 365,
    "WriteToDatabase": true,
    "WriteToFile": true
  },
  "ApplicationLogging": {
    "Enabled": true,
    "StoragePath": "/var/log/fiducia/app",
    "FileNameFormat": "app-{yyyy-MM-dd}.log",
    "RotationPeriod": "Daily",
    "FileSizeLimitMB": 100
  }
}
```

### Настройки в UI

Настройки аудита и логирования доступны в **Admin Console → Системные настройки**:

- **Аудит безопасности**: директория хранения, формат имени файла, периодичность ротации, срок хранения, запись в БД/файл
- **Логирование приложений**: директория хранения, формат имени файла, периодичность ротации, лимит размера файла

### Требования к хранилищу

- Обособленное хранилище, физически отделённое от ИСПДн
- Некорректируемость записей (append-only)
- Шифрование при передаче и хранении
- Резервное копирование

---

## Incident Response



### Процесс

1. **Обнаружение**: Мониторинг, алерты, ручные отчёты
2. **Оценка**: Определение серьёзности и влияния
3. **Сдерживание**: Изоляция affected систем
4. **Устранение**: Исправление уязвимости
5. **Восстановление**: Возврат в рабочее состояние
6. **Анализ**: Пост-mortem и улучшения

### Контакты

| Роль | Контакт |
|------|---------|
| Security Lead | security@samorodinkatech.ru |
| On-call | +7 (XXX) XXX-XX-XX |
| Escalation | cto@samorodinkatech.ru |

---

## Безопасность в разработке

### Code Review Checklist

- [ ] Нет hardcoded secrets
- [ ] Валидация всех входных данных
- [ ] Параметризованные запросы
- [ ] Корректная обработка ошибок (без утечки деталей)
- [ ] Логирование событий безопасности
- [ ] Шифрование чувствительных данных
- [ ] Авторизация проверена

### SAST (Static Application Security Testing)

```yaml
# .github/workflows/security.yml
name: Security Scan
on: [push, pull_request]

jobs:
  security:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Run security scan
        uses: securecodewarrior/github-action-add-sarif@v1
        with:
          sarif-file: security-scan-results.sarif

      - name: Run dependency check
        run: dotnet list package --vulnerable
```
