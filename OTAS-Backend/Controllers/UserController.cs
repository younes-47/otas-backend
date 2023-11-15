using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.Interfaces.IRepository;
using OTAS.Models;
using OTAS.Repository;
using OTAS.Services;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddUserAsync([FromBody] UserDTO user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var mappedUser = _mapper.Map<User>(user);
            ServiceResult result = await _userRepository.AddUserAsync(mappedUser);

            return Ok(result.Message);
        }


        [HttpGet("All")]
        public IActionResult GetAllUsers()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            ICollection<UserDTO> users = _mapper.Map<List<UserDTO>>(_userRepository.GetAllUsersAsync());

            if (users.Count <= 0) return NotFound("No User found");
            
            return Ok(users);
        }

    }

    


}
