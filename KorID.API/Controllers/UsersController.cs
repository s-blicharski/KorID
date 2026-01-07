﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KorID.API.Models;
using KorID.Data.Model;
using KorID.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KorID.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;

        public UsersController(IUserRepository repo)
        {
            _repo = repo;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _repo.GetAllAsync();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            });
            return Ok(userDtos);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _repo.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UpdateUserRequest request)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return NotFound();

            var existingUserByName = await _repo.GetByUsernameAsync(request.Username);
            if (existingUserByName != null && existingUserByName.Id != id)
            {
                return BadRequest(new { message = "Nazwa użytkownika jest już zajęta." });
            }

            var existingUserByEmail = await _repo.GetByEmailAsync(request.Email);
            if (existingUserByEmail != null && existingUserByEmail.Id != id)
            {
                return BadRequest(new { message = "Email jest już zajęty." });
            }

            user.Username = request.Username;
            user.Email = request.Email;

            await _repo.UpdateAsync(user);

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            await _repo.AddAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
