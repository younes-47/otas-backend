using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.Interfaces.IRepository;
using OTAS.Models;

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
        public IActionResult AddUser([FromBody] UserDTO user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var mappedUser = _mapper.Map<User>(user);

            if (!_userRepository.AddUser(mappedUser))
            {
                ModelState.AddModelError("", "Something went wrong while adding the user");
                return BadRequest(ModelState);
            }

            return Ok("User added successfully");
        }


        [HttpGet("All")]
        public IActionResult GetAllUsers()
        {

            ICollection<UserDTO> users = _mapper.Map<List<UserDTO>>(_userRepository.GetAllUsers());

            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (users.Count <= 0) return NotFound("No User found");
            
            return Ok(users);
        }

    }

    


}
