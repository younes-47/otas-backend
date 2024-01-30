using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OTAS.DTO.Get;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Services;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OTAS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IJWTManagerRepository _jWTManagerRepository;

        public AuthController(IAuthService authService, IConfiguration configuration, IJWTManagerRepository jWTManagerRepository)
        {
            
            _authService = authService;
            _configuration = configuration;
            _jWTManagerRepository = jWTManagerRepository;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest usersdata)
        {
            Tokens? token = await _authService.AuthenticateAsync(usersdata);

            if (token == null)
            {
                return Unauthorized();
            }

            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("CreateDeciderToken")]
        public async Task<IActionResult> CreateToken(string deciderLevel)
        {

            Tokens? token = await _jWTManagerRepository.CreateTokenByLevel(deciderLevel);

            return Ok(token);
        }

    }
}
