namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

public interface IAuthProvider
{
    Task<AuthResult> AuthenticateAsync(string username, string password);
    Task<List<UserInfo>> GetUsersAsync();
    string ProviderName { get; }
}

public class AuthResult
{
    public bool Success { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> Claims { get; set; } = new();
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Role { get; set; }
}
