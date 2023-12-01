using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using OTAS.DTO.Get;

namespace OTAS.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly LdapAuthenticationService _ldapAuthService;

        public AuthController(LdapAuthenticationService ldapAuthService)
        {
            _ldapAuthService = ldapAuthService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] string username, [FromBody] string password)
        {
            if (username == null || password == null) return BadRequest("Username and Password are required!");
            if (await _ldapAuthService.AuthenticateUserAsync(username, password))
            {
                // Successful authentication, generate and return JWT token
                var token = JwtTokenGenerator.GenerateToken(username, role);
                return Ok(new { Token = token });
            }

            // Authentication failed
            return Unauthorized(new { Message = "Invalid credentials" });
        }
    }
}
