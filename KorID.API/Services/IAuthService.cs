using KorID.API.Models;

namespace KorID.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(string username, string password);
    Task<bool> RegisterAsync(RegisterRequest request);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string GenerateToken(string username, IEnumerable<string> roles, int userId = 0, string? email = null);

}
