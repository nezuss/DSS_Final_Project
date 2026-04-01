using Backend.DTO.Auth;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

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

            Regex validateEmailRegex = new Regex("^\\S+@\\S+\\.\\S+$");
            if (registerDTO.Email.Length > 254 || validateEmailRegex.IsMatch(registerDTO.Email) == false)
                return BadRequest(new { error = "400", message = "Email must be at most 254 characters long" });

            if (registerDTO.Password.Length < 6 || registerDTO.Password.Length > 128)
                return BadRequest(new { error = "400", message = "Password must be at least 6 characters long" });

            var existingUser = db.Users.FirstOrDefault(u => u.Email == registerDTO.Email);
            if (existingUser != null)
                return Conflict(new { error = "409", message = "Email already in use" });

            var user = new UserModel
            {
                Id = Guid.NewGuid().ToString(),
                Email = registerDTO.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password),
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

            Regex validateEmailRegex = new Regex("^\\S+@\\S+\\.\\S+$");
            if (loginDTO.Email.Length > 254 || validateEmailRegex.IsMatch(loginDTO.Email) == false)
                return BadRequest(new { error = "400", message = "Email must be at most 254 characters long" });

            if (loginDTO.Password.Length < 6 || loginDTO.Password.Length > 128)
                return BadRequest(new { error = "400", message = "Password must be at least 6 characters long" });

            var user = db.Users.FirstOrDefault(u => u.Email == loginDTO.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.Password))
                return Unauthorized(new { error = "401", message = "Invalid email or password" });

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