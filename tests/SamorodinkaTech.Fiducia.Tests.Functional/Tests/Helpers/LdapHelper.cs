using System.Diagnostics;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для управления пользователями в OpenLDAP.
/// </summary>
public static class LdapHelper
{
    private const string PhpLdapAdminUrl = "http://localhost:8082";
    private const string LdapBaseDn = "dc=fiducia,dc=local";
    private const string LdapAdminDn = "cn=admin,dc=fiducia,dc=local";
    private const string LdapAdminPassword = "admin";
    private const string UsersOu = "ou=users";
    private const string GroupsOu = "ou=groups";
    private const int DefaultTimeout = 15_000;

    /// <summary>
    /// Удалить ВСЕ тестовые учётные записи из LDAP (ou=users).
    /// Группы не трогаем — удаление пользователя автоматически исключает его из groupOfNames.
    /// </summary>
    public static async Task DeleteAllTestUsersAsync()
    {
        var userDns = await LdapSearchAsync($"{UsersOu},{LdapBaseDn}", "(objectClass=inetOrgPerson)");
        Console.WriteLine($"[LDAP] Найдено {userDns.Count} пользователей для удаления.");

        foreach (var userDn in userDns)
        {
            Console.WriteLine($"[LDAP] Удаление: {userDn}");
            await LdapDeleteAsync(userDn);
        }
        Console.WriteLine("[LDAP] Все тестовые пользователи удалены.");
    }

    /// <summary>
    /// Удалить конкретного пользователя по uid.
    /// </summary>
    public static async Task DeleteUserAsync(string uid)
    {
        var dn = $"uid={uid},{UsersOu},{LdapBaseDn}";
        await LdapRemoveMemberFromAllGroupsAsync(dn);
        await LdapDeleteAsync(dn);
    }

    /// <summary>
    /// Удалить пользователя через phpLDAPadmin UI (fallback).
    /// </summary>
    public static async Task DeleteUserViaUIAsync(IPage page, string uid)
    {
        await LoginToPhpLdapAdminAsync(page);

        var userDn = $"uid={uid},{UsersOu},{LdapBaseDn}";
        await page.GotoAsync($"{PhpLdapAdminUrl}/?dn={Uri.EscapeDataString(userDn)}&submit=Найти");
        await page.WaitForTimeoutAsync(2000);

        // Click delete link
        await page.EvaluateAsync(
            @"() => {
                const links = document.querySelectorAll('a');
                for (const link of links) {
                    const href = link.href || '';
                    const text = link.textContent || '';
                    if (text.includes('Delete') || text.includes('Удалить') || href.includes('delete')) {
                        link.click(); return true;
                    }
                }
                return false;
            }");
        await page.WaitForTimeoutAsync(1000);

        // Confirm deletion
        await page.EvaluateAsync(
            @"() => {
                const buttons = document.querySelectorAll('input[type=submit], button');
                for (const btn of buttons) {
                    const text = btn.textContent || btn.value || '';
                    if (text.includes('Delete') || text.includes('Удалить') || text.includes('Yes') || text.includes('Да')) {
                        btn.click(); return true;
                    }
                }
                return false;
            }");
        await page.WaitForTimeoutAsync(2000);
    }

    /// <summary>
    /// Создать пользователя в OpenLDAP через ldapadd CLI.
    /// </summary>
    public static async Task CreateUserAsync(
        IPage page,
        string uid,
        string cn,
        string sn,
        string givenName,
        string password,
        bool addToBoardGroup = true)
    {
        var userDn = $"uid={uid},{UsersOu},{LdapBaseDn}";

        var ldif = $@"dn: {userDn}
objectClass: inetOrgPerson
objectClass: organizationalPerson
objectClass: person
objectClass: top
uid: {uid}
cn: {cn}
sn: {sn}
givenName: {givenName}
userPassword: {password}
";

        await LdapAddAsync(ldif);

        if (addToBoardGroup)
        {
            await AddUserToGroupAsync(uid, "BoardOfDirectors");
        }
    }

    /// <summary>
    /// Добавить пользователя в LDAP-группу через ldapmodify CLI.
    /// </summary>
    public static async Task AddUserToGroupAsync(string userUid, string groupName)
    {
        var groupDn = $"cn={groupName},{GroupsOu},{LdapBaseDn}";
        var userDn = $"uid={userUid},{UsersOu},{LdapBaseDn}";

        var ldif = $"""
            dn: {groupDn}
            changetype: modify
            add: member
            member: {userDn}
            """;

        var ldifWithNewline = ldif + "\n";

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ldapmodify",
                Arguments = $"-x -H ldap://localhost -D \"{LdapAdminDn}\" -w \"{LdapAdminPassword}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        process.Start();
        await process.StandardInput.WriteAsync(ldifWithNewline);
        await process.StandardInput.FlushAsync();
        await process.StandardInput.DisposeAsync();
        await process.WaitForExitAsync();
    }

    // ── LDAP CLI operations ──────────────────────────────────────────────

    /// <summary>
    /// Добавить LDAP-запись через ldapadd CLI.
    /// </summary>
    private static async Task LdapAddAsync(string ldif)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ldapadd",
                Arguments = $"-x -H ldap://localhost -D \"{LdapAdminDn}\" -w \"{LdapAdminPassword}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        process.Start();
        await process.StandardInput.WriteAsync(ldif);
        await process.StandardInput.FlushAsync();
        await process.StandardInput.DisposeAsync(); // Закрываем stdin — это сигнал EOF для ldapadd

        // Используем таймаут для ожидания завершения процесса
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill();
            process.WaitForExit();
            throw new TimeoutException("ldapadd timed out after 10 seconds");
        }

        if (process.ExitCode != 0)
        {
            var stderr = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"ldapadd failed (exit {process.ExitCode}): {stderr}");
        }
    }

    /// <summary>
    /// Выполнить LDAP search через ldapsearch CLI и вернуть список DN.
    /// </summary>
    private static async Task<List<string>> LdapSearchAsync(string baseDn, string filter)
    {
        var dns = new List<string>();

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ldapsearch",
                Arguments = $"-x -H ldap://localhost -b \"{baseDn}\" -D \"{LdapAdminDn}\" -w \"{LdapAdminPassword}\" \"{filter}\" dn",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        foreach (var line in stdout.Split('\n'))
        {
            var trimmed = line.Trim();

            // Обычный DN: dn: cn=user01,ou=users,dc=fiducia,dc=local
            if (trimmed.StartsWith("dn:", StringComparison.OrdinalIgnoreCase))
            {
                var dn = trimmed[3..].Trim();
                if (!string.IsNullOrEmpty(dn))
                    dns.Add(dn);
            }
            // Base64-encoded DN: dn:: Y2490JjQstCw0L3QvtCyINCY0LLQsNC9...
            else if (trimmed.StartsWith("dn::", StringComparison.OrdinalIgnoreCase))
            {
                var base64Value = trimmed[4..].Trim();
                if (!string.IsNullOrEmpty(base64Value))
                {
                    try
                    {
                        var bytes = Convert.FromBase64String(base64Value);
                        var dn = System.Text.Encoding.UTF8.GetString(bytes);
                        dns.Add(dn);
                    }
                    catch
                    {
                        // Если не удалось декодировать, пропускаем
                    }
                }
            }
        }

        return dns;
    }

    /// <summary>
    /// Удалить LDAP-запись по DN через ldapdelete CLI.
    /// </summary>
    private static async Task LdapDeleteAsync(string dn)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ldapdelete",
                Arguments = $"-x -H ldap://localhost -D \"{LdapAdminDn}\" -w \"{LdapAdminPassword}\" \"{dn}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        process.Start();
        await process.WaitForExitAsync();
    }

    /// <summary>
    /// Удалить пользователя из всех групп, в которых он состоит.
    /// groupOfNames требует хотя бы один member — если удаляем последнего, пропускаем.
    /// </summary>
    private static async Task LdapRemoveMemberFromAllGroupsAsync(string userDn)
    {
        var groupDns = await LdapSearchAsync($"{GroupsOu},{LdapBaseDn}",
            $"(&(objectClass=groupOfNames)(member={userDn}))");

        foreach (var groupDn in groupDns)
        {
            var ldif = $"""
                dn: {groupDn}
                changetype: modify
                delete: member
                member: {userDn}

                """;

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ldapmodify",
                    Arguments = $"-x -H ldap://localhost -D \"{LdapAdminDn}\" -w \"{LdapAdminPassword}\"",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                }
            };

            process.Start();
            await process.StandardInput.WriteAsync(ldif);
            await process.StandardInput.FlushAsync();
            await process.StandardInput.DisposeAsync();

            // Используем таймаут для ожидания завершения процесса
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill();
                process.WaitForExit(); // Ждём завершения после Kill
            }

            // groupOfNames требует хотя бы один member — ошибка при удалении последнего ожидаема
            if (process.ExitCode != 0)
            {
                var stderr = await process.StandardError.ReadToEndAsync();
                if (!stderr.Contains("requires attribute 'member'"))
                {
                    Console.WriteLine($"[LDAP] Warning: ldapmodify failed for {groupDn}: {stderr}");
                }
            }
        }
    }

    // ── phpLDAPadmin UI operations ───────────────────────────────────────

    private static async Task LoginToPhpLdapAdminAsync(IPage page)
    {
        // Переходим на страницу входа напрямую (phpLDAPadmin использует AJAX)
        await page.GotoAsync($"{PhpLdapAdminUrl}/cmd.php?cmd=login_form&server_id=1");
        await page.WaitForTimeoutAsync(2000);

        // Ищем поле DN для входа
        var dnInput = page.Locator("input[name='login'], input[name='dn'], input#login, input[name='ldap_login_id']");
        if (await dnInput.CountAsync() > 0 && await dnInput.First.IsVisibleAsync())
        {
            await dnInput.First.FillAsync(LdapAdminDn);

            var passwordInput = page.Locator("input[name='password'], input[type='password']");
            if (await passwordInput.CountAsync() > 0)
            {
                await passwordInput.First.FillAsync(LdapAdminPassword);
            }

            var submitButton = page.Locator("input[type='submit'], button[type='submit']").First;
            await submitButton.ClickAsync();
            await page.WaitForTimeoutAsync(3000);
        }
    }

    private static async Task FillLdapFieldAsync(IPage page, string fieldName, string value)
    {
        var input = page.Locator(
            $"input[name='{fieldName}'], " +
            $"input[name*='{fieldName}'], " +
            $"textarea[name='{fieldName}'], " +
            $"input[placeholder*='{fieldName}']");

        if (await input.CountAsync() > 0)
        {
            await input.First.FillAsync(value);
        }
    }
}
