using Microsoft.EntityFrameworkCore;
using OTAS.Data;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using System.Data;
using TMA_App.Services;

namespace OTAS.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        //private readonly IRoleRepository _roleRepository;
        private readonly IJWTManagerRepository _jwtManagerRepository;
        private readonly OtasContext _context;

        public AuthService(IUserRepository userRepository, IJWTManagerRepository jWTManagerRepository, OtasContext context)
        {
            _userRepository = userRepository;
            //_roleRepository = roleRepository;
            _jwtManagerRepository = jWTManagerRepository;
            _context = context;
        }
        public async Task<Tokens?> AuthenticateAsync(AuthRequest AuthRequest)
        {
            LdapAuthentication ldap = new();
            if (AuthRequest.Username == null) { return null; }
            if (AuthRequest.Password == null) { return null; }
            string username = AuthRequest.Username;
            string password = AuthRequest.Password;
            bool isAuthenticated = ldap.IsAuthenticated(username, password);
            if (isAuthenticated)
            {
                string roleName = await GetRoleAsync(username);
                string preferredLanguage = await GetLangAsync(username);
                Tokens tokens = _jwtManagerRepository.CreateToken(username, roleName, preferredLanguage);
                return tokens;
            }
            else { return null; }
        }

        public async Task<string> GetRoleAsync(string username)
        {
            //this method is used to GetRole of a user using username
            User? user = await _userRepository.FindUserByUsernameAsync(username);
            if (user == null)
            {
                User newUser = await CreateNewUserAsync(username) ?? throw new Exception($"error while creating new user {username}");
                RoleCode roleCode = await _context.RoleCodes.Where(rc => rc.RoleInt == newUser.Role).FirstAsync() ?? throw new Exception($"error while getting role id : {newUser.Role}");
                return roleCode.RoleString;
            }
            else
            {
                RoleCode roleCode = await _context.RoleCodes.Where(rc => rc.RoleInt == user.Role).FirstAsync() ?? throw new Exception($"error while getting role id : {user.Role}");
                return roleCode.RoleString;
            }

        }

        public async Task<string> GetLangAsync(string username)
        {
            //this method is used to GetRole of a user using username
            User? user = await _userRepository.FindUserByUsernameAsync(username);
            if (user == null)
            {
                throw new Exception($"error while getting preferredLng id : {username}");
            }
            return user.PreferredLanguage;
        }

        public async Task<User> CreateNewUserAsync(string username)
        {
            string[] userInfo;
            LdapAuthentication ldap = new();
            userInfo = ldap.GetUserInfo(username);
            User user = new()
            {
                Username = username,
                FirstName = userInfo[0],
                LastName = userInfo[1],
                Role = 1,
            };
            //User superiorUser=new();
            string superiorUserName = userInfo[2];
            if (superiorUserName == "")
            {
                //no superior exist user is the VP
            }
            else
            {
                User? superiorUser = await _userRepository.FindUserByUsernameAsync(superiorUserName);
                if (superiorUser == null)
                {
                    superiorUser = await CreateNewUserAsync(superiorUserName) ?? throw new Exception($"error while creating new user {superiorUserName}");
                    //Create new user and add superior FK if exists
                }
                else
                {
                    //superiorUser is in the database
                }
                user.SuperiorUserId = superiorUser.Id;

            }
            //This method is used to create a new user with minimal access using username as parameter and returning the created user as async response
            return await _userRepository.CreateUserAsync(user);
        }
    }
}
