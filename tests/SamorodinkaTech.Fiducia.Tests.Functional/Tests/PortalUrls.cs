using Microsoft.Extensions.Configuration;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>Портал системы.</summary>
public enum Portal { BoardPortal, AdminConsole }

/// <summary>
/// Адреса порталов из конфигурации (appsettings.json).
/// Изменяются в конфиге — без перекомпиляции тестов.
/// </summary>
public static class PortalUrls
{
    private static readonly string _boardPortal;
    private static readonly string _adminConsole;

    static PortalUrls()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "FIDUCIA_");
        var config = builder.Build();

        _boardPortal = config["TestUrl:BoardPortal"]
                       ?? Environment.GetEnvironmentVariable("TESTURL_BPORTAL")
                       ?? "http://localhost:5002";
        _adminConsole = config["TestUrl:AdminConsole"]
                        ?? Environment.GetEnvironmentVariable("TESTURL_ADMINCONSOLE")
                        ?? "http://localhost:5001";
    }

    /// <summary>Полный URL к указанному пути портала.</summary>
    public static string GetUrl(Portal portal, string path = "/") =>
        $"{GetBase(portal)}{EnsureLeadingSlash(path)}";

    private static string GetBase(Portal portal) =>
        portal == Portal.BoardPortal ? _boardPortal : _adminConsole;

    private static string EnsureLeadingSlash(string p) =>
        p.StartsWith('/') ? p : '/' + p;
}
