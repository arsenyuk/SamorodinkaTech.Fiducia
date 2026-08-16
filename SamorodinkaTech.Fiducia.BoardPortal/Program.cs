using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SamorodinkaTech.Fiducia.BoardPortal;
using SamorodinkaTech.Fiducia.BoardPortal.Data;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Auditing;
using SamorodinkaTech.Fiducia.Infrastructure.Authentication;
using SamorodinkaTech.Fiducia.Infrastructure.Notifications;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;
using SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;
using SamorodinkaTech.Fiducia.Infrastructure.Middleware;
using SamorodinkaTech.Fiducia.Domain.Entities;
using Microsoft.AspNetCore.Http;
using SamorodinkaTech.Fiducia.Infrastructure.FileStorage;
using SamorodinkaTech.Fiducia.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog: заменяет встроенный Microsoft.Extensions.Logging (ADR-021)
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddHttpContextAccessor();
// HttpClient with BaseAddress and JWT token from cookie
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddHttpClient("BoardPortal")
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var client = factory.CreateClient("BoardPortal");
    client.BaseAddress = new Uri(nav.BaseUri);
    return client;
});

// Database
// DbContextOptions as Singleton so IDbContextFactory can consume them without lifetime conflicts
builder.Services.AddDbContext<IApplicationDbContext, FiduciaDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    contextLifetime: ServiceLifetime.Scoped,
    optionsLifetime: ServiceLifetime.Singleton);
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
    if (!builder.Configuration.GetValue<bool>("Ldap:Enabled"))
    {
        Log.Warning("Auth:Method = LDAP, но Ldap:Enabled = false. Переключаюсь на Basic.");
        builder.Services.AddScoped<IAuthProvider, BasicProvider>();
    }
    else
    {

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
builder.Services.AddSingleton<ISecurityAuditService, SecurityAuditService>();

// Notification Service (US-009)
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IElectionNominationService, ElectionNominationService>();

// Time provider (SOLID: DIP) — абстракция системного времени для тестируемости
builder.Services.AddSingleton<ITimeProvider, SystemTimeProvider>();

// File Storage (ADR-020)
builder.Services.AddFileStorage(builder.Configuration);

// Chunked Upload (BDR-011)
builder.Services.Configure<FileUploadOptions>(builder.Configuration.GetSection("FileUpload"));
builder.Services.AddScoped<IChunkedUploadService, ChunkedUploadService>();

// GOSA interval service — расчёт интервала ГОСА (BDR-007)
builder.Services.AddSingleton<ILegalEntityGosaIntervalService, LegalEntityGosaIntervalService>();

// Template instantiation — подстановка данных в шаблоны документов
builder.Services.AddScoped<ITemplateInstantiationService, TemplateInstantiationService>();

// Meeting services — сохранение и загрузка данных собраний (OsaMeeting + BoardOfDirectors + BoardMembers)
builder.Services.AddScoped<IMeetingSaveService, MeetingSaveService>();
builder.Services.AddScoped<IMeetingLoadService, MeetingLoadService>();

// TrueConf Server API — видеоконференцсвязь для заседаний СД (опционально)
// Все настройки — в appsettings.json, секция TrueConf (ADR-022)
builder.Services.Configure<TrueConfOptions>(builder.Configuration.GetSection("TrueConf"));
if (builder.Configuration.GetValue<bool>("TrueConf:Enabled"))
{
    builder.Services.AddScoped<ITrueConfApiClient>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<TrueConfOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<TrueConfApiClient>>();

        // В Dev-режиме: игнорирование SSL-сертификата для self-signed сертификатов TrueConf Server
        var handler = new HttpClientHandler();
        if (builder.Environment.IsDevelopment())
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }
        var httpClient = new HttpClient(handler);

        var inner = new TrueConfApiClient(httpClient, logger, options.BaseUrl);
        var auditService = sp.GetRequiredService<ISecurityAuditService>();
        var auditLogger = sp.GetRequiredService<ILogger<AuditTrueConfDecorator>>();
        return new AuditTrueConfDecorator(inner, auditService, auditLogger);
    });
}

// MTS Link (Webinar.ru) — вебинарная платформа для заседаний СД (опционально)
// Все настройки — в appsettings.json, секция MtsLink (ADR-022)
builder.Services.Configure<MtsLinkOptions>(builder.Configuration.GetSection("MtsLink"));
if (builder.Configuration.GetValue<bool>("MtsLink:Enabled"))
{
    builder.Services.AddScoped<IMtsLinkApiClient>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<MtsLinkOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<MtsLinkApiClient>>();
        var inner = new MtsLinkApiClient(new HttpClient(), logger, options.BaseUrl, options.ApiToken);
        var auditService = sp.GetRequiredService<ISecurityAuditService>();
        var auditLogger = sp.GetRequiredService<ILogger<AuditMtsLinkDecorator>>();
        return new AuditMtsLinkDecorator(inner, auditService, auditLogger);
    });
}

// SPARK (Интерфакс) — поиск компаний по ИНН
// Все настройки — в appsettings.json, секция Spark (ADR-022)
builder.Services.Configure<SparkOptions>(builder.Configuration.GetSection("Spark"));
builder.Services.AddScoped<ISparkApiClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<SparkOptions>>().Value;
    var logger = sp.GetRequiredService<ILogger<SparkApiClient>>();
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
    }
    var httpClient = new HttpClient(handler);
    var inner = new SparkApiClient(httpClient, logger, options.BaseUrl, options.Login, options.Password);
    var auditService = sp.GetRequiredService<ISecurityAuditService>();
    var auditLogger = sp.GetRequiredService<ILogger<AuditSparkDecorator>>();
    return new AuditSparkDecorator(inner, auditService, auditLogger);
});

builder.Services.AddScoped<ISparkDataService, SparkDataService>();

// CBR FinOrg API — справочник участников финансового рынка (опционально)
// Все настройки — в appsettings.json, секция CbrFinOrg (ADR-022)
builder.Services.Configure<CbrFinOrgOptions>(builder.Configuration.GetSection("CbrFinOrg"));
if (builder.Configuration.GetValue<bool>("CbrFinOrg:Enabled"))
{
    builder.Services.AddScoped<ICbrFinOrgClient>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<CbrFinOrgOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<CbrFinOrgApiClient>>();
        var inner = new CbrFinOrgApiClient(new HttpClient(), logger, options.BaseUrl);
        var auditService = sp.GetRequiredService<ISecurityAuditService>();
        var auditLogger = sp.GetRequiredService<ILogger<AuditCbrFinOrgDecorator>>();
        return new AuditCbrFinOrgDecorator(inner, auditService, auditLogger);
    });

    // Сервис кэширования данных ЦБ РФ — загрузка из API, сохранение в ext_cbr_finorg_*, TTL 24ч
    builder.Services.AddScoped<ICbrFinOrgDataService, CbrFinOrgDataService>();
}

// LDAP — корпоративный каталог для синхронизации состава СД (опционально)
if (builder.Configuration.GetValue<bool>("Ldap:Enabled"))
{
    builder.Services.AddSingleton<ILdapService>(sp =>
    {
        var cfg = builder.Configuration.GetSection("Ldap");
        var logger = sp.GetRequiredService<ILogger<LdapService>>();
        var inner = new LdapService(
            cfg["Server"] ?? "localhost",
            int.TryParse(cfg["Port"], out var port) ? port : 389,
            cfg["BaseDn"] ?? "dc=bryansk-arsenal,dc=local",
            cfg["BindUser"],
            cfg["BindPassword"],
            logger);
        var auditService = sp.GetRequiredService<ISecurityAuditService>();
        var auditLogger = sp.GetRequiredService<ILogger<AuditLdapDecorator>>();
        return new AuditLdapDecorator(inner, auditService, auditLogger);
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
                // First check Authorization header (for loopback FileUpload requests)
                var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Token = authHeader["Bearer ".Length..].Trim();
                    return Task.CompletedTask;
                }
                // Then cookie (for browser requests)
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

app.UseMiddleware<ExceptionLoggingMiddleware>();

// Логирование 404 + Referer
app.UseMiddleware<NotFoundLoggingMiddleware>();

// Аудит доступа к страницам (PAGE_ACCESS, PAGE_ACCESS_DENIED, PAGE_NOT_FOUND)
app.UseMiddleware<PageAccessAuditMiddleware>();

Log.Information("BoardPortal starting ({Environment})", app.Environment.EnvironmentName);

app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("BoardPortal stopping ({Environment})", app.Environment.EnvironmentName);
    Log.CloseAndFlush();
});

// Session API (УПД.15)
app.MapGet("/api/session/config", (ISessionService svc) =>
    Results.Ok(new { idleTimeoutMinutes = svc.GetIdleTimeoutMinutes() }));

app.MapPost("/api/session/logout", () =>
    Results.Ok(new { message = "Logged out" }));

// Установка cookie с JWT на стороне сервера (HttpOnly)
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

// File upload/download endpoints (BDR-011)
var fileGroup = app.MapGroup("/api/files").RequireAuthorization().WithTags("Files");

fileGroup.MapPost("/upload", async (HttpContext http, IChunkedUploadService uploadService) =>
{
    var request = await http.Request.ReadFromJsonAsync<UploadInitRequest>();
    if (request == null) return Results.BadRequest(new { error = "Invalid request" });
    try
    {
        var uploadId = await uploadService.InitiateUploadAsync(request.FileName, request.ContentType, request.TotalSizeBytes);
        return Results.Ok(new { uploadId, maxChunkSize = 512 * 1024 });
    }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

fileGroup.MapPost("/upload/chunk", async (HttpContext http, IChunkedUploadService uploadService) =>
{
    var form = await http.Request.ReadFormAsync();
    var uploadId = form["uploadId"].FirstOrDefault();
    var chunkIndexStr = form["chunkIndex"].FirstOrDefault();
    var chunkFile = http.Request.Form.Files["chunk"];

    if (string.IsNullOrEmpty(uploadId) || chunkIndexStr == null || chunkFile == null)
        return Results.BadRequest(new { error = "Missing parameters" });

    if (!int.TryParse(chunkIndexStr, out var chunkIndex))
        return Results.BadRequest(new { error = "Invalid chunkIndex" });

    try
    {
        await using var stream = chunkFile.OpenReadStream();
        await uploadService.UploadChunkAsync(uploadId, chunkIndex, stream);
        return Results.Ok();
    }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

fileGroup.MapPost("/upload/complete", async (HttpContext http, IChunkedUploadService uploadService) =>
{
    var request = await http.Request.ReadFromJsonAsync<UploadCompleteRequest>();
    if (request == null) return Results.BadRequest(new { error = "Invalid request" });
    try
    {
        var fileEntry = await uploadService.CompleteUploadAsync(request.UploadId);
        return Results.Ok(new { fileId = fileEntry.Id, originalName = fileEntry.OriginalName, sizeBytes = fileEntry.SizeBytes });
    }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

fileGroup.MapGet("/upload/{uploadId}/chunks", async (string uploadId, IChunkedUploadService uploadService) =>
{
    var chunks = await uploadService.GetUploadedChunksAsync(uploadId);
    return Results.Ok(new { uploadId, chunkIndices = chunks.ToList() });
});

fileGroup.MapDelete("/upload/{uploadId}", async (string uploadId, IChunkedUploadService uploadService) =>
{
    await uploadService.AbortUploadAsync(uploadId);
    return Results.Ok();
});

fileGroup.MapGet("/{id}/download", async (Guid id, IApplicationDbContext db, IFileStorage fileStorage) =>
{
    var fileEntry = db.Files.FirstOrDefault(f => f.Id == id);
    if (fileEntry == null) return Results.NotFound();
    try
    {
        var stream = await fileStorage.OpenReadAsync(fileEntry.StorageKeyOrPath);
        return Results.File(stream, fileEntry.ContentType ?? "application/octet-stream", fileEntry.OriginalName);
    }
    catch (FileNotFoundException) { return Results.NotFound(); }
});

fileGroup.MapDelete("/{id}", async (Guid id, IApplicationDbContext db, IFileStorage fileStorage) =>
{
    var fileEntry = db.Files.FirstOrDefault(f => f.Id == id);
    if (fileEntry == null) return Results.NotFound();
    await fileStorage.DeleteAsync(fileEntry.StorageKeyOrPath);
    db.Files.Remove(fileEntry);
    await ((FiduciaDbContext)db).SaveChangesAsync();
    return Results.Ok();
});

// ── AO Contractors API (Регистратор / Информационное агентство) ──────────
app.MapAoContractorEndpoints();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

// DTO records для file upload API
public record UploadInitRequest(string FileName, string? ContentType, long TotalSizeBytes);
public record UploadCompleteRequest(string UploadId);

public record LoginCookieRequest(string Token);
