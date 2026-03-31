using Backend.DTO.Auth;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService tokenService;
        private readonly DbModel db;

        public AuthController(ITokenService tokenService, DbModel db)
        {
            this.tokenService = tokenService;
            this.db = db;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Register(RegisterDTO registerDTO)
        {
            if (String.IsNullOrEmpty(registerDTO.Email) || String.IsNullOrEmpty(registerDTO.Password))
                return BadRequest(new { error = "400", message = "Invalid email, password or display name" });

            var existingUser = db.Users.FirstOrDefault(u => u.Email == registerDTO.Email);
            if (existingUser != null)
                return Conflict(new { error = "409", message = "Email already in use" });

            var user = new UserModel
            {
                Id = Guid.NewGuid().ToString(),
                Email = registerDTO.Email,
                Password = registerDTO.Password,
                DisplayName = registerDTO.DisplayName,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            db.SaveChanges();

            return StatusCode(201, new {
                user.Id,
                user.Email,
                user.DisplayName
            });
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login(LoginDTO loginDTO)
        {
            if (String.IsNullOrEmpty(loginDTO.Email) || String.IsNullOrEmpty(loginDTO.Password))
                return BadRequest(new { error = "400", message = "Invalid email or password" });

            var user = db.Users.FirstOrDefault(u => u.Email == loginDTO.Email && u.Password == loginDTO.Password);
            
            if (user == null) return Unauthorized(new { error = "401", message = "Invalid email or password" });

            return Ok(new {
                accessToken = tokenService.CreateAccessToken(user),
                tokenType = "Bearer",
                expiresIn = 3600,
                user = new {
                    user.Id,
                    user.Email,
                    user.DisplayName
                }
            });
        }
    }
}