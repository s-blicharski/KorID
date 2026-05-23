using KorID.Data;
using KorID.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KorID.API.Controllers;

[Route("api/users/{userId:int}/roles")]
[ApiController]
[Authorize(Roles = "admin")]
public class UserRolesController : ControllerBase
{
    private readonly KorIdDbContext _db;
    public UserRolesController(KorIdDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetRoles(int userId)
    {
        var user = await _db.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();
        return Ok(user.Roles.Select(r => r.Name));
    }

    [HttpPut("{roleId:int}")]
    public async Task<IActionResult> Assign(int userId, int roleId)
    {
        var user = await _db.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == userId);
        var role = await _db.Roles.FindAsync(roleId);
        if (user == null || role == null) return NotFound();

        if (!user.Roles.Any(r => r.Id == roleId))
        {
            user.Roles.Add(role);
            await _db.SaveChangesAsync();
        }
        return NoContent();
    }

    [HttpDelete("{roleId:int}")]
    public async Task<IActionResult> Unassign(int userId, int roleId)
    {
        var user = await _db.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        var role = user.Roles.FirstOrDefault(r => r.Id == roleId);
        if (role != null)
        {
            user.Roles.Remove(role);
            await _db.SaveChangesAsync();
        }
        return NoContent();
    }
}