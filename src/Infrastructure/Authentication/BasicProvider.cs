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
        // Basic SSO: username = user ID
        if (!Guid.TryParse(username, out var userId))
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Некорректный идентификатор пользователя"
            };
        }

        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Пользователь не найден"
            };
        }

        // Определяем роль: если есть SYS_ADMIN — даём доступ администратора
        var roleName = user.UserRoles
            .Select(ur => ur.Role?.Code)
            .FirstOrDefault(rc => rc == "SYS_ADMIN")
            ?? user.UserRoles.Select(ur => ur.Role?.Code).FirstOrDefault()
            ?? "MEMBER_BOARD";

        return new AuthResult
        {
            Success = true,
            UserId = user.Id,
            UserName = $"{user.LastName} {user.FirstName} {user.MiddleName}",
            Claims = new Dictionary<string, string>
            {
                ["role"] = roleName,
                ["email"] = user.Email,
                ["is_external"] = user.IsExternal.ToString()
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
                Username = u.Id.ToString(),
                Email = u.Email,
                Role = u.UserRoles.Select(ur => ur.Role!.Code).FirstOrDefault() ?? "MEMBER_BOARD"
            })
            .ToListAsync();
    }
}
