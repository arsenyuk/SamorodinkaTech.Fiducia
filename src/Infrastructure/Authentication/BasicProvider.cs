using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Authentication;

public class BasicProvider : IAuthProvider
{
    private readonly FiduciaDbContext _dbContext;

    public string ProviderName => "Basic";

    public BasicProvider(FiduciaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        // Basic SSO: login = user login field
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Login == username && !u.IsSystem);

        if (user == null)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Пользователь не найден"
            };
        }

        // Определяем роли: возвращаем все роли пользователя через запятую
        var allRoles = string.Join(",", user.UserRoles
            .Select(ur => ur.Role?.Code)
            .Where(rc => rc is not null));

        var roleName = string.IsNullOrEmpty(allRoles) ? "MEMBER_BOARD" : allRoles;

        // Проверяем наличие подписанного ПЭП для внешнего директора
        var hasPep = false;
        if (user.IsExternal && user.PersonId.HasValue)
        {
            hasPep = await _dbContext.PepAgreements
                .AnyAsync(a => a.PersonId == user.PersonId.Value && a.AgreementSigned);
        }

        return new AuthResult
        {
            Success = true,
            UserId = user.Id,
            UserName = $"{user.LastName} {user.FirstName} {user.MiddleName}",
            Login = user.Login,
            Claims = new Dictionary<string, string>
            {
                ["role"] = roleName,
                ["email"] = user.Email,
                ["is_external"] = user.IsExternal.ToString(),
                ["pep_signed"] = hasPep.ToString()
            }
        };
    }

    public async Task<List<UserInfo>> GetUsersAsync()
    {
        return await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.LastName)
            .Select(u => new UserInfo
            {
                Id = u.Id,
                DisplayName = $"{u.LastName} {u.FirstName} {u.MiddleName}",
                Username = u.Login,
                Email = u.Email,
                Role = u.UserRoles.Select(ur => ur.Role!.Code).FirstOrDefault() ?? "MEMBER_BOARD"
            })
            .ToListAsync();
    }
}
