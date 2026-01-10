using System.Threading.Tasks;
using KorID.API.Models;
using KorID.API.Services;
using KorID.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KorID.API.Controllers;

[Route("api/external")]
[ApiController]
public class ExternalLoginController : ControllerBase
{
    private readonly KorIdDbContext _db;
    private readonly IAuthService _authService;

    public ExternalLoginController(KorIdDbContext db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    public class ExternalLoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public int ApplicationId { get; set; }
        public string? RedirectUrl { get; set; }
    }

    public class ExternalLoginResult
    {
        public bool Success { get; set; }
        public string? Username { get; set; }
        public string? RedirectUrl { get; set; }
        public string? Message { get; set; }
    }

    // simple ping for debugging
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { ok = true, now = System.DateTime.UtcNow });

    // Changed route attribute to relative path so controller route prefix is honored
    [HttpPost("login")]
    public async Task<ActionResult<ExternalLoginResult>> Login([FromBody] ExternalLoginRequest req)
    {
        if (req == null) return BadRequest(new ExternalLoginResult { Success = false, Message = "Brak danych ¿¹dania." });

        var app = await _db.Applications
            .Include(a => a.Organization)
            .SingleOrDefaultAsync(a => a.Id == req.ApplicationId);

        if (app == null)
        {
            return NotFound(new ExternalLoginResult { Success = false, Message = "Aplikacja nie znaleziona." });
        }

        if (app.OrganizationId == null)
        {
            return BadRequest(new ExternalLoginResult { Success = false, Message = "Aplikacja nie jest powi¹zana z organizacj¹." });
        }

        var user = await _db.Users
            .Include(u => u.UserApplications)
            .SingleOrDefaultAsync(u => u.Email == req.Email && u.OrganizationId == app.OrganizationId);

        if (user == null)
        {
            return Unauthorized(new ExternalLoginResult { Success = false, Message = "U¿ytkownik nie istnieje w organizacji powi¹zanej z aplikacj¹." });
        }

        // verify password using auth service (supports demo hash marker)
        if (!_authService.VerifyPassword(req.Password, user.PasswordHash))
        {
            return Unauthorized(new ExternalLoginResult { Success = false, Message = "Nieprawid³owe dane logowania." });
        }

        // check grant
        var hasGrant = await _db.UserApplications.AnyAsync(ua => ua.UserId == user.Id && ua.ApplicationId == app.Id);
        if (!hasGrant)
        {
            return Forbid();
        }

        // success - choose redirect
        var redirect = req.RedirectUrl ?? app.RedirectUri;

        return Ok(new ExternalLoginResult
        {
            Success = true,
            Username = user.Username,
            RedirectUrl = redirect
        });
    }
}
