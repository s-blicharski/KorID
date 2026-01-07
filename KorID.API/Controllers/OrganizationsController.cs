using System.Collections.Generic;
using System.Threading.Tasks;
using KorID.Data;
using KorID.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KorID.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly KorIdDbContext _context;

        public OrganizationsController(KorIdDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations()
        {
            return Ok(await _context.Organizations.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Organization>> GetOrganization(int id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null) return NotFound();
            return org;
        }

        [HttpPost]
        public async Task<ActionResult<Organization>> PostOrganization(Organization organization)
        {
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, organization);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrganization(int id, Organization organization)
        {
            if (id != organization.Id) return BadRequest();

            _context.Entry(organization).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await OrganizationExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null) return NotFound();

            _context.Organizations.Remove(org);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<bool> OrganizationExists(int id) =>
            _context.Organizations.AnyAsync(e => e.Id == id);
    }
}