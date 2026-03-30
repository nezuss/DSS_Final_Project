using Backend.DTO.Auth;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("/register")]
        public IActionResult Register(RegisterDTO registerDTO)
        {
            var user = new UserModel
            {
                Id = Guid.NewGuid().ToString(),
                Email = registerDTO.Email,
                Password = registerDTO.Password,
                DisplayName = registerDTO.DisplayName
            };

            return Ok(new {
                user.Id,
                user.Email,
                user.DisplayName
            });
        }

        [HttpPost("/login")]
        public IActionResult Login(LoginDTO loginDTO)
        {
            var user = new UserModel
            {
                Id = Guid.NewGuid().ToString(),
                Email = loginDTO.Email,
                Password = loginDTO.Password
            };

            return Ok(new {
                accessToken = Guid.NewGuid().ToString(),
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