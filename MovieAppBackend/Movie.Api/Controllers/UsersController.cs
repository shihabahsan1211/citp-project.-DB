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
            // check if username already exists
            var existing = await _auth.GetByUsernameAsync(dto.Username);
            if (existing != null)
                return BadRequest("User already exists");

            var user = new AppUser
            {
                Username = dto.Username,
                Email = dto.Email
            };

            var created = await _auth.CreateUserAsync(user, dto.Password);

            return Ok(new
            {
                created.Id,
                created.Username,
                created.Email
            });
        }

        // ✅ LOGIN (no refresh token, just validate user)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var user = await _auth.ValidateUserAsync(dto.Username, dto.Password);
            if (user == null)
                return Unauthorized("Invalid credentials");

            // simple response – user is valid
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email
            });
        }
    }
}
