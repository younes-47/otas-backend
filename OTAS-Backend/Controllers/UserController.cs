using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OTAS.Data;
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
        private readonly IDeciderRepository _deciderRepository;
        private readonly ILdapAuthenticationService _ldapAuthenticationService;
        private readonly OtasContext _context;
        private readonly IMapper _mapper;
        public UserController(IUserRepository userRepository,
            IDeciderRepository deciderRepository,
            ILdapAuthenticationService ldapAuthenticationService,
            OtasContext context,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _deciderRepository = deciderRepository;
            _ldapAuthenticationService = ldapAuthenticationService;
            _context = context;
            _mapper = mapper;
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("info")]
        public async Task<UserInfoDTO> GetUserInformation()
        {
            string username = HttpContext.User.Identity.Name;
            var userInfo =  _ldapAuthenticationService.GetUserInformation(username);
            var userId = await _userRepository.GetUserIdByUsernameAsync(username);
            userInfo.Level = await _deciderRepository.GetDeciderLevelByUserId(userId);
            return userInfo;
        }


        [Authorize(Roles = "requester,decider")]
        [HttpGet("JobTitles/All")]
        public async Task<IActionResult> GetJobTitles()
        {
            List<string> jobTitles = await _context.JobTitles.Select(jt => jt.Title).ToListAsync();
            return Ok(jobTitles);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("Departments/All")]
        public async Task<IActionResult> GetDepartments()
        {

            var departments = await _context.Departments.Select(dp => dp.Name).ToListAsync();
            return Ok(departments);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("ManagersUsernames/All")]
        public async Task<IActionResult> GetManagersUsernames()
        {
            List<string> managersUsersanmes = await _deciderRepository.GetManagersUsernames();
            return Ok(managersUsersanmes);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("ActualRequesterStaticInfo/All")]
        public async Task<IActionResult> GetActualRequesterStaticInfo()
        {
            ActualRequesterStaticInfoDTO staticData = new();
            staticData.JobTitles = await _context.JobTitles.Select(jt => jt.Title).ToListAsync();
            staticData.Departments= await _context.Departments.Select(dp => dp.Name).ToListAsync();
            staticData.ManagersUsernames =  await _deciderRepository.GetManagersUsernames();

            return Ok(staticData);
        }

    }
}
