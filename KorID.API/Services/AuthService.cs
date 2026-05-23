using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KorID.API.Models;
using KorID.Data.Model;
using KorID.Data.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace KorID.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse?> AuthenticateAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        var roles = user.Roles.Select(r => r.Name).ToArray();
        var token = GenerateToken(user.Username, roles, user.Id, user.Email);
        var expiresAt = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60));

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            Username = user.Username,
            Roles = roles
        };
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.GetByUsernameAsync(request.Username) != null)
        {
            return false;
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password)
        };

        await _userRepository.AddAsync(user);
        return true;
    }

    public string HashPassword(string password) => _passwordHasher.Hash(password);

    public bool VerifyPassword(string password, string passwordHash)
    {
        // Furtka demo: hash zaczynający się od "$DEMO$HASH$" traktujemy jako plaintext.
        if (!string.IsNullOrEmpty(passwordHash) && passwordHash.StartsWith("$DEMO$HASH$"))
        {
            var demoPlain = passwordHash.Substring("$DEMO$HASH$".Length);
            return password == demoPlain;
        }

        return _passwordHasher.Verify(password, passwordHash);
    }

    public string GenerateToken(string username, IEnumerable<string> roles, int userId = 0, string? email = null)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "KorID";
        var audience = jwtSettings["Audience"] ?? "KorID";
        var expirationMinutes = jwtSettings.GetValue<int>("ExpirationMinutes", 60);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(email))
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));

        // Claim roli pod krótką nazwą "role". Walidacja ustawia RoleClaimType = "role"
        // i MapInboundClaims = false, więc nie ma mapowania na długie URI.
        foreach (var role in roles.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct())
            claims.Add(new Claim("role", role));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}