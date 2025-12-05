using Microsoft.AspNetCore.Mvc;
using Movie.Domain.DTOs;
using Movie.Infrastructure.Models;
using Movie.Infrastructure.Repositories;

namespace Movie.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthRepository _auth;

        public UsersController(IAuthRepository auth)
        {
            _auth = auth;
        }

        // ✅ REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            var existing = await _auth.GetByUsernameAsync(dto.Username);
            if (existing != null)
                return BadRequest("User already exists");

            var user = new AppUser
            {
                Username = dto.Username,
                Email = dto.Email
            };

            var created = await _auth.CreateUserAsync(user, dto.Password);

            var response = new UserResponseDto
            {
                Id = created.Id,
                Username = created.Username,
                Email = created.Email
            };

            return Ok(response);
        }

        // ✅ LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var user = await _auth.ValidateUserAsync(dto.Username, dto.Password);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var response = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };

            return Ok(response);
        }
    }
}
