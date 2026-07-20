using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog: заменяет встроенный Microsoft.Extensions.Logging (ADR-021)
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
// HttpClient with BaseAddress for relative API calls
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
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
builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();

// Notification Service (US-009)
builder.Services.AddScoped<INotificationService, NotificationService>();

// File Storage (ADR-020)
builder.Services.AddFileStorage(builder.Configuration);

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

app.UseMiddleware<ExceptionLoggingMiddleware>();

// Логирование 404 + Referer
app.UseMiddleware<NotFoundLoggingMiddleware>();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

public record LoginCookieRequest(string Token);
