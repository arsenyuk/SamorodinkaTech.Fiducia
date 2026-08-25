using System.Diagnostics;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для управления пользователями в OpenLDAP.
/// </summary>
public static class LdapHelper
{
    private const string PhpLdapAdminUrl = "http://localhost:8082";
    private const string LdapBaseDn = "dc=bryansk-arsenal,dc=local";
    private const string LdapAdminDn = "cn=admin,dc=bryansk-arsenal,dc=local";
    private const string LdapAdminPassword = "ldappassword";
    private const string UsersOu = "ou=users";
    private const string GroupsOu = "ou=groups";
    private const int DefaultTimeout = 15_000;

    /// <summary>
    /// Удалить ВСЕ тестовые учётные записи из LDAP (ou=users) и очистить группы.
    /// Вызывается самым первым шагом каждого сценария для обеспечения чистоты теста.
    /// </summary>
    public static async Task DeleteAllTestUsersAsync()
    {
        // 1. Получить список всех DN в ou=users
        var userDns = await LdapSearchAsync($"{UsersOu},{LdapBaseDn}", "(objectClass=inetOrgPerson)");

        // 2. Удалить пользователя из всех групп (чтобы не нарушить ссылочную целостность)
        foreach (var userDn in userDns)
        {
            await LdapRemoveMemberFromAllGroupsAsync(userDn);
        }

        // 3. Удалить каждого пользователя
        foreach (var userDn in userDns)
        {
            await LdapDeleteAsync(userDn);
        }
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
    /// Создать пользователя в OpenLDAP через phpLDAPadmin.
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
        await LoginToPhpLdapAdminAsync(page);

        // Navigate to user creation form
        var createUserLink = page.Locator("a:has-text('Создать'), a:has-text('Create'), a:has-text('Add')").First;
        await createUserLink.ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        // Select inetOrgPerson object class
        var inetOrgPersonOption = page.Locator("option:has-text('inetOrgPerson'), input[value*='inetOrgPerson']");
        if (await inetOrgPersonOption.CountAsync() > 0)
        {
            await inetOrgPersonOption.First.ClickAsync();
            var nextButton = page.Locator("input[type='submit'], button[type='submit']").First;
            await nextButton.ClickAsync();
            await page.WaitForTimeoutAsync(1000);
        }

        // Fill in the user attributes
        await FillLdapFieldAsync(page, "uid", uid);
        await FillLdapFieldAsync(page, "cn", cn);
        await FillLdapFieldAsync(page, "sn", sn);
        await FillLdapFieldAsync(page, "givenName", givenName);
        await FillLdapFieldAsync(page, "userPassword", password);

        // Submit the form
        var submitButton = page.Locator("input[type='submit'][value*='Create'], input[type='submit'][value*='Создать'], button:has-text('Create'), button:has-text('Создать')").First;
        await submitButton.ClickAsync();
        await page.WaitForTimeoutAsync(2000);

        if (addToBoardGroup)
        {
            await AddUserToGroupAsync(page, uid, "BoardOfDirectors");
        }
    }

    /// <summary>
    /// Добавить пользователя в LDAP-группу.
    /// </summary>
    public static async Task AddUserToGroupAsync(IPage page, string userUid, string groupName)
    {
        await page.GotoAsync($"{PhpLdapAdminUrl}/?cn={groupName},{GroupsOu},{LdapBaseDn}&submit=Найти");
        await page.WaitForTimeoutAsync(1000);

        var memberInput = page.Locator("input[name*='member'], textarea[name*='member']");
        if (await memberInput.CountAsync() > 0)
        {
            var dn = $"uid={userUid},{UsersOu},{LdapBaseDn}";
            await memberInput.First.FillAsync(dn);
        }
    }

    // ── LDAP CLI operations ──────────────────────────────────────────────

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
            if (trimmed.StartsWith("dn:", StringComparison.OrdinalIgnoreCase))
            {
                var dn = trimmed[3..].Trim();
                if (!string.IsNullOrEmpty(dn))
                    dns.Add(dn);
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
    /// </summary>
    private static async Task LdapRemoveMemberFromAllGroupsAsync(string userDn)
    {
        var groupDns = await LdapSearchAsync($"{GroupsOu},{LdapBaseDn}",
            $"(&(objectClass=groupOfNames)(member={userDn}))");

        foreach (var groupDn in groupDns)
        {
            var ldif = $"dn: {groupDn}\nchangetype: modify\ndel: member\nmember: {userDn}\n";

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
            await process.WaitForExitAsync();
        }
    }

    // ── phpLDAPadmin UI operations ───────────────────────────────────────

    private static async Task LoginToPhpLdapAdminAsync(IPage page)
    {
        await page.GotoAsync(PhpLdapAdminUrl);
        await page.WaitForTimeoutAsync(2000);

        var loginInput = page.Locator("input[name='login'], input[name='dn'], input#login");
        if (await loginInput.CountAsync() > 0 && await loginInput.First.IsVisibleAsync())
        {
            await loginInput.First.FillAsync(LdapAdminDn);

            var passwordInput = page.Locator("input[name='password'], input[type='password']");
            if (await passwordInput.CountAsync() > 0)
            {
                await passwordInput.First.FillAsync(LdapAdminPassword);
            }

            var submitButton = page.Locator("input[type='submit'], button[type='submit']").First;
            await submitButton.ClickAsync();
            await page.WaitForTimeoutAsync(2000);
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
