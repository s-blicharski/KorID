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
    public class ApplicationsController : ControllerBase
    {
        private readonly KorIdDbContext _context;

        public ApplicationsController(KorIdDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Application>>> GetApplications()
        {
            return Ok(await _context.Applications
                .Include(a => a.Organization)
                .ToListAsync());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Application>> GetApplication(int id)
        {
            var app = await _context.Applications
                .Include(a => a.Organization)
                .SingleOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound();
            return app;
        }

        [HttpPost]
        [Authorize(Roles = "admin")] 
        public async Task<ActionResult<Application>> PostApplication(Application application)
        {
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] 
        public async Task<IActionResult> PutApplication(int id, Application application)
        {
            if (id != application.Id) return BadRequest();

            _context.Entry(application).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ApplicationExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] 
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();

            _context.Applications.Remove(app);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<bool> ApplicationExists(int id) =>
            _context.Applications.AnyAsync(e => e.Id == id);
    }
}