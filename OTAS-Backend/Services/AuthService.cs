﻿//using OTAS.Interfaces.IRepository;
//using OTAS.Interfaces.IService;
//using OTAS.Models;
//using System.Data;
//using TMA_App.Services;

//namespace OTAS.Services
//{
//    public class AuthService : IAuthService
//    {
//        private readonly IUserRepository _userRepository;
//        private readonly IJWTManagerRepository _jwtManagerRepository;

//        public AuthService(IUserRepository userRepository, IJWTManagerRepository jWTManagerRepository)
//        {
//            _userRepository = userRepository;
//            _jwtManagerRepository = jWTManagerRepository;
//        }
//        public async Task<Tokens?> AuthenticateAsync(AuthRequest AuthRequest)
//        {
//            LdapAuthentication ldap = new();
//            if (AuthRequest.Username == null) { return null; }
//            if (AuthRequest.Password == null) { return null; }
//            string username = AuthRequest.Username;
//            string password = AuthRequest.Password;
//            bool isAuthenticated = ldap.IsAuthenticated(username, password);
//            if (isAuthenticated)
//            {
//                string role = await _userRepository.GetUserRoleByUsernameAsync(username);
//                Tokens tokens = _jwtManagerRepository.CreateToken(username, role);
//                return tokens;
//            }
//            else { return null; }
//        }
//        public async Task<ServiceResult> CreateNewUserAsync(string username)
//        {
//            string[] userInfo;
//            LdapAuthentication ldap = new();
//            userInfo = ldap.GetUserInfo(username);
//            User user = new()
//            {
//                Username = username,
//                FirstName = userInfo[0],
//                LastName = userInfo[1],
//            };
//            //This method is used to create a new user with minimal access using username as parameter and returning the created user as async response
//            return await _userRepository.AddUserAsync(user);
//        }
//    }
//}
