using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using KorID.API.Services;
using KorID.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AccessController : ControllerBase
{
    private readonly IDistributedCache _cache;
    private readonly QrCodeService _qrCodeService;
    private readonly IAuthService _authService;
    private readonly IUserRepository _users;
    private readonly IConfiguration _configuration;

    public AccessController(IDistributedCache cache, QrCodeService qrCodeService,
                            IAuthService authService, IUserRepository users, IConfiguration configuration)
    {
        _cache = cache;
        _qrCodeService = qrCodeService;
        _authService = authService;
        _users = users;
        _configuration = configuration;
    }

    public class GenerateQrRequest
    {
        public string? Username { get; set; }
        public string? Role { get; set; }
    }

    private record QrPayload(string? Username, string[] Roles);

    [HttpPost("generate")]
    [Authorize]  // Wymaga zalogowania
    public async Task<IActionResult> GenerateAccessQr([FromBody] GenerateQrRequest? request)
    {   
        var targetUsername = request?.Username ?? User.Identity?.Name;

        if (string.IsNullOrEmpty(targetUsername))
            return Unauthorized(new { Message = "Nie uda³o siê pobraæ danych u¿ytkownika." });

        // Wczytaj u¿ytkownika dla którego chcemy wygenerowaæ QR
        var user = await _users.GetByUsernameAsync(targetUsername);
        if (user == null)
            return Unauthorized(new { Message = "U¿ytkownik nie znaleziony." });

        // Pobierz role u¿ytkownika
        var roles = user.Roles.Select(r => r.Name).ToArray();
        if (roles.Length == 0)
            roles = new[] { "user" };

        var token = _authService.GenerateToken(user.Username, roles, user.Id, user.Email);
        var qrCodeImage = _qrCodeService.GenerateQrCode(token);
        return File(qrCodeImage, "image/png");
    }

    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyAccessQr([FromBody] VerifyRequest request)
    {
        if (TryValidateJwtToken(request.Token, out var jwtUsername, out var jwtRoles, out var jwtUserId, out var jwtEmail))
        {
            return Ok(new
            {
                Message = "Dostêp przyznany",
                Token = request.Token,
                Username = jwtUsername,
                Roles = jwtRoles
            });
        }

        var cacheKey = $"qr_token:{request.Token}";
        var raw = await _cache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(raw))
            return Unauthorized(new { Message = "Kod QR wygas³ lub jest nieprawid³owy." });

        await _cache.RemoveAsync(cacheKey);

        var payload = JsonSerializer.Deserialize<QrPayload>(raw)!;

        string username;
        string[] roles;
        int userId = 0;
        string? email = null;

        if (!string.IsNullOrWhiteSpace(payload.Username))
        {
            var user = await _users.GetByUsernameAsync(payload.Username);
            if (user == null)
                return Unauthorized(new { Message = "U¿ytkownik z kodu QR nie istnieje." });

            username = user.Username;
            email = user.Email;
            userId = user.Id;
            roles = user.Roles.Select(r => r.Name).ToArray();
            if (roles.Length == 0) roles = payload.Roles;
        }
        else
        {
            username = $"{string.Join(",", payload.Roles)}.qr";
            roles = payload.Roles;
        }

        var jwt = _authService.GenerateToken(username, roles, userId, email);

        return Ok(new
        {
            Message = "Dostêp przyznany",
            Token = jwt,
            Username = username,
            Roles = roles
        });
    }

    private bool TryValidateJwtToken(string token, out string username, out string[] roles, out int userId, out string? email)
    {
        username = string.Empty;
        roles = Array.Empty<string>();
        userId = 0;
        email = null;

        if (string.IsNullOrWhiteSpace(token) || token.Count(c => c == '.') < 2)
            return false;

        try
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey not configured");

            var handler = new JwtSecurityTokenHandler{ 
                MapInboundClaims = false 
            };
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                RoleClaimType = "role",
                NameClaimType = JwtRegisteredClaimNames.UniqueName,
                ClockSkew = TimeSpan.FromMinutes(1)
            }, out _);

            username = principal.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                ?? principal.Identity?.Name
                ?? string.Empty;
            roles = principal.FindAll("role").Select(c => c.Value).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().ToArray();
            int.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out userId);
            email = principal.FindFirstValue(JwtRegisteredClaimNames.Email);

            return !string.IsNullOrWhiteSpace(username) && roles.Length > 0;
        }
        catch
        {
            return false;
        }
    }
}

public class VerifyRequest
{
    public string Token { get; set; } = null!;
}