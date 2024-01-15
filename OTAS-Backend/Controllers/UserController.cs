using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Repository;

namespace OTAS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILdapAuthenticationService _ldapAuthenticationService;
        private readonly IMapper _mapper;
        public UserController(IUserRepository userRepository,
            ILdapAuthenticationService ldapAuthenticationService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _ldapAuthenticationService = ldapAuthenticationService;
            _mapper = mapper;
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("info")]
        public UserInfoDTO GetUserInformation()
        {
            string username = HttpContext.User.Identity.Name;
            var userInfo =  _ldapAuthenticationService.GetUserInformation(username);
            // Log claims for debugging
            foreach (var claim in HttpContext.User.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }
            return userInfo;
        }


    }
}
