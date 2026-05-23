using System.Collections.Generic;
using System.Threading.Tasks;
using KorID.Data;
using KorID.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KorID.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]  
    public class UserApplicationsController : ControllerBase
    {
        private readonly KorIdDbContext _context;

        public UserApplicationsController(KorIdDbContext context)
        {
            _context = context;
        }

        // GET: api/UserApplications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserApplication>>> GetUserApplications()
        {
            return Ok(await _context.UserApplications
                .Include(ua => ua.User)
                .Include(ua => ua.Application)
                .ToListAsync());
        }

        // GET: api/UserApplications/5/10
        [HttpGet("{userId:int}/{applicationId:int}")]
        public async Task<ActionResult<UserApplication>> GetUserApplication(int userId, int applicationId)
        {
            var ua = await _context.UserApplications
                .Include(x => x.User)
                .Include(x => x.Application)
                .SingleOrDefaultAsync(x => x.UserId == userId && x.ApplicationId == applicationId);

            if (ua == null) return NotFound();
            return ua;
        }

        // POST: api/UserApplications
        [HttpPost]
        public async Task<ActionResult<UserApplication>> PostUserApplication(UserApplication userApplication)
        {
            _context.UserApplications.Add(userApplication);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserApplication),
                new { userId = userApplication.UserId, applicationId = userApplication.ApplicationId }, userApplication);
        }

        // PUT: api/UserApplications/5/10
        [HttpPut("{userId:int}/{applicationId:int}")]
        public async Task<IActionResult> PutUserApplication(int userId, int applicationId, UserApplication userApplication)
        {
            if (userId != userApplication.UserId || applicationId != userApplication.ApplicationId) return BadRequest();

            // Update only allowed fields (e.g. GrantedAt). For simplicity replace the entity state.
            _context.Entry(userApplication).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserApplicationExists(userId, applicationId)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/UserApplications/5/10
        [HttpDelete("{userId:int}/{applicationId:int}")]
        public async Task<IActionResult> DeleteUserApplication(int userId, int applicationId)
        {
            var ua = await _context.UserApplications.FindAsync(userId, applicationId);
            if (ua == null) return NotFound();

            _context.UserApplications.Remove(ua);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<bool> UserApplicationExists(int userId, int applicationId) =>
            _context.UserApplications.AnyAsync(e => e.UserId == userId && e.ApplicationId == applicationId);
    }
}