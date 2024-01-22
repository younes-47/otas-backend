using OTAS.DTO.Get;
using System.DirectoryServices;

namespace OTAS.Interfaces.IService
{
    public interface ILdapAuthenticationService
    {
        bool IsAuthenticated(string username, string password);
        string[] GetUserInfo(string username);
        UserInfoDTO GetUserInformation(string username);
        string GetUsernameByDisplayName(string displayName);
        string GetManagerCN(string managerFullField);
        List<string> GetJobTitles();
        List<string> GetDepartments();
    }
}
