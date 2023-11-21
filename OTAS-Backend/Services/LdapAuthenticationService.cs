//using Microsoft.AspNetCore.Authentication;
//using System;
//using System.DirectoryServices.ActiveDirectory;
//using System.Threading.Tasks;

//public class LdapAuthenticationService
//{
//    private readonly string _domain;
//    private readonly string _ldapPath;

//    public LdapAuthenticationService(string domain, string ldapPath)
//    {
//        _domain = domain;
//        _ldapPath = ldapPath;
//    }

//    public async Task<bool> AuthenticateUserAsync(string username, string password)
//    {
//        try
//        {
//            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, _domain, _ldapPath))
//            {
//                return pc.ValidateCredentials(username, password);
//            }
//        }
//        catch
//        {
//            return false;
//        }
//    }
//}
