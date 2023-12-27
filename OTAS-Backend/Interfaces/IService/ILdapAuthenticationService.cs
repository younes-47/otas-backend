using OTAS.DTO.Get;

namespace OTAS.Interfaces.IService
{
    public interface ILdapAuthenticationService
    {
        bool IsAuthenticated(string username, string password);
        string[] GetUserInfo(string username);
        UserInfoDTO GetUserInformation(string username);
        string GetUsernameByDisplayName(string displayName);
        string GetManagerCN(string managerFullField);
    }
}
