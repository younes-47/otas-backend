using Microsoft.IdentityModel.Tokens;
using OTAS.Interfaces.IRepository;
using OTAS.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OTAS.Repository
{
    public class JWTManagerRepository : IJWTManagerRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        public JWTManagerRepository(IConfiguration Iconfiguration, IUserRepository userRepository)
        {
            _configuration = Iconfiguration;
            _userRepository = userRepository;
        }
        public Tokens CreateToken(string username, string role, string preferredLanguage)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]) ;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new Tokens { Token = tokenHandler.WriteToken(token), Role = role, PreferredLanguage = preferredLanguage};
        }

        public  async Task<Tokens> CreateTokenByLevel(string level)
        {
            string deciderUsername = await _userRepository.GetDeciderUsernameByDeciderLevel(level);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim(ClaimTypes.Name, deciderUsername),
                        new Claim(ClaimTypes.Role, "decider")
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new Tokens { Token = tokenHandler.WriteToken(token), Role = "decider", PreferredLanguage = "en" };
        }
    }
}
