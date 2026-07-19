using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Auditing;
using SamorodinkaTech.Fiducia.Infrastructure.Authentication;
using SamorodinkaTech.Fiducia.Infrastructure.Notifications;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;
using SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;
using SamorodinkaTech.Fiducia.Infrastructure.Middleware;
using SamorodinkaTech.Fiducia.Infrastructure.Services;
using SamorodinkaTech.Fiducia.Infrastructure.FileStorage;
using SamorodinkaTech.Fiducia.AdminConsole.Contracts;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog: заменяет встроенный Microsoft.Extensions.Logging (ADR-021)
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
// Для Blazor Server настраиваем HttpClient с BaseAddress, чтобы работали относительные URI ("/api/...")
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

// Database
// Configure DbContextOptions as Singleton so it can be consumed by the
// singleton IDbContextFactory without lifetime conflicts.
builder.Services.AddDbContext<IApplicationDbContext, FiduciaDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    contextLifetime: ServiceLifetime.Scoped,
    optionsLifetime: ServiceLifetime.Singleton);
// DbContext factory for Blazor concurrency-safe per-operation contexts
builder.Services.AddDbContextFactory<FiduciaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication Provider
var authMethod = builder.Configuration["Auth:Method"] ?? "Basic";
if (authMethod == "ActiveDirectory")
{
    builder.Services.AddScoped<IAuthProvider, ActiveDirectoryProvider>();
}
else if (authMethod == "LDAP")
{
    // LDAP должен быть включён в конфигурации
    if (!builder.Configuration.GetValue<bool>("Ldap:Enabled"))
    {
        Log.Warning("Auth:Method = LDAP, но Ldap:Enabled = false. Переключаюсь на Basic.");
        builder.Services.AddScoped<IAuthProvider, BasicProvider>();
        // Пропускаем LDAP-регистрацию
    }
    else
    {

    // LdapAuthProvider использует ILdapService, который регистрируется ниже
    builder.Services.AddScoped<IAuthProvider>(sp =>
    {
        var ldap = sp.GetRequiredService<ILdapService>();
        var logger = sp.GetRequiredService<ILogger<LdapAuthProvider>>();
        var sysAdminGroupDn = builder.Configuration["Ldap:SysAdminGroupDn"]
                             ?? "cn=SysAdmins,ou=Groups,dc=bryansk-arsenal,dc=local";
        var boardGroupDn = builder.Configuration["Ldap:BoardGroupDn"]
                          ?? "cn=BoardOfDirectors,ou=Groups,dc=bryansk-arsenal,dc=local";
        return new LdapAuthProvider(ldap, logger, sysAdminGroupDn, boardGroupDn);
    });
    }
}
else
{
    builder.Services.AddScoped<IAuthProvider, BasicProvider>();
}

// Session Service (УПД.15)
builder.Services.AddSingleton<ISessionService, SessionService>();

// Security Audit Service (РСБ.2 + РСБ.3)
// Файловая запись аудита — через Serilog sub-logger (фильтр по SourceContext)
builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();

// Notification Service (US-009)
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<ILegalEntityGosaIntervalService, LegalEntityGosaIntervalService>();

// File Storage (ADR-020)
builder.Services.AddFileStorage(builder.Configuration);

// SPARK API — проверка ЮЛ по ИНН, карточка компании и данные о гендиректоре
// Все настройки — в appsettings.json, секция Spark (ADR-022)
builder.Services.Configure<SparkOptions>(builder.Configuration.GetSection("Spark"));
builder.Services.AddScoped<ISparkApiClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<SparkOptions>>().Value;
    var logger = sp.GetRequiredService<ILogger<SparkApiClient>>();
    return new SparkApiClient(new HttpClient(), logger, options.BaseUrl, options.ApiKey);
});

// LDAP — корпоративный каталог для синхронизации состава СД (опционально)
if (builder.Configuration.GetValue<bool>("Ldap:Enabled"))
{
    builder.Services.AddSingleton<ILdapService>(sp =>
    {
        var cfg = builder.Configuration.GetSection("Ldap");
        var logger = sp.GetRequiredService<ILogger<LdapService>>();
        return new LdapService(
            cfg["Server"] ?? "localhost",
            int.TryParse(cfg["Port"], out var port) ? port : 389,
            cfg["BaseDn"] ?? "dc=bryansk-arsenal,dc=local",
            cfg["BindUser"],
            cfg["BindPassword"],
            logger);
    });

    builder.Services.AddSingleton<IBoardMemberLdapService>(sp =>
    {
        var ldap = sp.GetRequiredService<ILdapService>();
        var logger = sp.GetRequiredService<ILogger<BoardMemberLdapService>>();
        var boardGroupDn = builder.Configuration["Ldap:BoardGroupDn"]
                           ?? "cn=BoardOfDirectors,ou=Groups,dc=bryansk-arsenal,dc=local";
        return new BoardMemberLdapService(ldap, logger, boardGroupDn);
    });
}

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["Session:JwtSecret"];
        if (string.IsNullOrEmpty(secretKey))
        {
            if (builder.Environment.IsDevelopment())
            {
                Log.Warning("Session:JwtSecret не задан — используется dev-ключ");
                secretKey = "Fiducia-dev-secret-key-change-in-production";
            }
            else
            {
                throw new InvalidOperationException("Session:JwtSecret is not configured");
            }
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Fiducia",
            ValidAudience = "Fiducia",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Cookies["SessionToken"];
                if (!string.IsNullOrEmpty(token))
                    ctx.Token = token;
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                Log.Debug("[JWT] Token validated for {Path}", ctx.Request.Path);
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                Log.Debug("[JWT] Auth failed: {Error}", ctx.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

// Serilog request logging (заменяет ручной ApplicationLogWriter для HTTP-запросов)
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000} ms";
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Централизованное логирование исключений
app.UseMiddleware<ExceptionLoggingMiddleware>();

// Логирование 404 + Referer
app.UseMiddleware<NotFoundLoggingMiddleware>();

Log.Information("AdminConsole starting ({Environment})", app.Environment.EnvironmentName);

app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("AdminConsole stopping ({Environment})", app.Environment.EnvironmentName);
    Log.CloseAndFlush();
});

// Session API (УПД.15)
app.MapGet("/api/session/config", (ISessionService svc) =>
    Results.Ok(new { idleTimeoutMinutes = svc.GetIdleTimeoutMinutes() }));

app.MapPost("/api/session/logout", () =>
    Results.Ok(new { message = "Logged out" }));

// Установка cookie с JWT на стороне сервера (HttpOnly)
// Нельзя установить HttpOnly cookie из JS. Этот endpoint принимает токен и выставляет cookie корректно.
app.MapPost("/api/session/login", (HttpContext http, ISessionService sessionService, LoginCookieRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Token))
        return Results.BadRequest(new { error = "Token is required" });

    var expires = DateTimeOffset.UtcNow.AddMinutes(sessionService.GetIdleTimeoutMinutes());
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = !http.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                 !http.Request.Host.Host.Equals("127.0.0.1"),
        SameSite = SameSiteMode.Strict,
        Expires = expires,
        Path = "/"
    };

    http.Response.Cookies.Append("SessionToken", req.Token, cookieOptions);
    return Results.Ok(new { message = "Login cookie set", expires });
});

// Admin: Legal Entities search (для формы ЛОСВ)
// q: часть наименования/ИНН/ОГРН, limit: макс. записей (по умолчанию 15)
app.MapGet("/api/admin/legal-entities/search", async (
    IApplicationDbContext db,
    string q,
    int? limit,
    CancellationToken ct) =>
{
    var take = Math.Clamp(limit ?? 15, 1, 50);
    q = q.Trim();

    var query = db.LegalEntities
        .Where(le =>
            (le.Name != null && EF.Functions.ILike(le.Name, $"%{q}%")) ||
            (le.ShortName != null && EF.Functions.ILike(le.ShortName, $"%{q}%")) ||
            (le.Inn != null && EF.Functions.ILike(le.Inn, $"%{q}%")) ||
            (le.Ogrn != null && EF.Functions.ILike(le.Ogrn, $"%{q}%")))
        .OrderBy(le => le.Name)
        .Take(take)
        .Select(le => new
        {
            le.Id,
            le.Name,
            le.ShortName,
            le.Inn,
            le.Ogrn,
            Okopf = le.Okopf != null ? new { le.Okopf.Code, le.Okopf.Name } : null
        });

    var list = await query.ToListAsync(ct);
    return Results.Ok(list);
}).RequireAuthorization(policy => policy.RequireRole("SYS_ADMIN"));

// Admin: чтение/сохранение руководителя ЮЛ (influential_people — singleton, BDR‑007)
app.MapGet("/api/admin/influential-people/list", async (IApplicationDbContext db, CancellationToken ct) =>
{
    var ip = await db.CurrentWorkplaces.FirstOrDefaultAsync(ct);
    return Results.Ok(ip is null ? new { } : new { ip.Id, ip.FullName, ip.Position });
}).RequireAuthorization(policy => policy.RequireRole("SYS_ADMIN"));

app.MapPost("/api/admin/influential-people", async (
    IApplicationDbContext db,
    CurrentWorkplace dto,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(dto.FullName))
        return Results.BadRequest(new { message = "FullName is required" });

    var existing = await db.CurrentWorkplaces.FirstOrDefaultAsync(ct);
    if (existing != null)
    {
        existing.FullName = dto.FullName.Trim();
        existing.Position = string.IsNullOrWhiteSpace(dto.Position) ? null : dto.Position.Trim();
    }
    else
    {
        var ip = new CurrentWorkplace
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName.Trim(),
            Position = string.IsNullOrWhiteSpace(dto.Position) ? null : dto.Position.Trim()
        };
        await db.CurrentWorkplaces.AddAsync(ip, ct);
    }

    if (db is DbContext ef)
        await ef.SaveChangesAsync(ct);

    return Results.NoContent();
}).RequireAuthorization(policy => policy.RequireRole("SYS_ADMIN"));

// Admin: чтение интервала ГОСА (глобальные настройки СД, BDR‑007)
app.MapGet("/api/admin/legal-entities/{id:guid}/gosa-window", async (
    Guid id,
    IApplicationDbContext db,
    ILegalEntityGosaIntervalService svc,
    CancellationToken ct) =>
{
    var le = await db.LegalEntities.Include(x => x.Okopf).FirstOrDefaultAsync(x => x.Id == id, ct);
    if (le == null) return Results.NotFound(new { message = "LegalEntity not found" });

    var settings = await db.LegalEntityBoardSettings.FirstOrDefaultAsync(ct);
    var (defStart, defEnd) = svc.GetDefaultWindow();

    var start = settings?.GosaWindowStart ?? defStart;
    var end = settings?.GosaWindowEnd ?? defEnd;
    var isPao = svc.IsPao(le.Okopf?.Code);

    return Results.Ok(new { start, end, isPao });
}).RequireAuthorization(policy => policy.RequireRole("SYS_ADMIN"));

// Admin: сохранение интервала ГОСА (глобальные настройки СД, BDR‑007)
app.MapPost("/api/admin/legal-entities/{id:guid}/gosa-window", async (
    Guid id,
    IApplicationDbContext db,
    ILegalEntityGosaIntervalService svc,
    GosaWindowDto dto,
    CancellationToken ct) =>
{
    var le = await db.LegalEntities.Include(x => x.Okopf).FirstOrDefaultAsync(x => x.Id == id, ct);
    if (le == null) return Results.NotFound(new { message = "LegalEntity not found" });

    var start = dto.Start;
    var end = dto.End;
    if (!svc.ValidateForOkopf(le.Okopf?.Code, start, end))
        return Results.BadRequest(new { message = "Недопустимый интервал ГОСА для данной ОПФ" });

    var settings = await db.LegalEntityBoardSettings.FirstOrDefaultAsync(ct);
    if (settings == null)
    {
        settings = new SamorodinkaTech.Fiducia.Domain.Entities.LegalEntityBoardSettings
        {
            Id = Guid.NewGuid(),
            GosaWindowStart = start,
            GosaWindowEnd = end
        };
        await db.LegalEntityBoardSettings.AddAsync(settings, ct);
    }
    else
    {
        settings.GosaWindowStart = start;
        settings.GosaWindowEnd = end;
    }

    if (db is DbContext ef)
        await ef.SaveChangesAsync(ct);

    return Results.NoContent();
}).RequireAuthorization(policy => policy.RequireRole("SYS_ADMIN"));

// Admin: добавление члена Совета директоров — валидация флага на бекенде
app.MapPost("/api/admin/board/members", (BoardMemberRequest req) =>
{
    if (!req.HasBoardOfDirectors)
        return Results.BadRequest(new { message = "Для данного юрлица Совет директоров отключён" });

    // Место для будущей реализации записи состава Совета директоров.
    // Пока делаем no-op с успешным кодом.
    return Results.NoContent();
}).RequireAuthorization(policy => policy.RequireRole("SYS_ADMIN"));

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

// Используется минимальным API выше для установки cookie
public record LoginCookieRequest(string Token);
public record BoardMemberRequest(Guid LegalEntityId, bool HasBoardOfDirectors);
