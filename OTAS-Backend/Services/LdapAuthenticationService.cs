using System.DirectoryServices;

namespace TMA_App.Services
{
    public class LdapAuthentication
    {
        private string _path = "LDAP://dicastalma.com";
        private static readonly string _domain = "dicastalma";
        public LdapAuthentication() { }
        public bool IsAuthenticated(string username, string password)
        {
            string domainAndUsername = $"{_domain}\\{username}";
            DirectoryEntry entry = new(_path, domainAndUsername, password);
            try
            {
                DirectorySearcher search = new DirectorySearcher(entry);

                search.Filter = $"(SAMAccountName={username})";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();
                if (result == null)
                {
                    return false;
                }
                // Update the new path to the user in the directory.
                _path = result.Path;
                //_filterAttribute = (string)result.Properties["cn"][0];
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        public string[] GetUserInfo(string username)
        {
            string[] userInfo = new string[4];
            string domainAndUsername = $"{_domain}\\{username}";

            try
            {
                DirectoryEntry entry = new DirectoryEntry(_path);
                // Create a DirectorySearcher object to search for the user's information.
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = $"(SAMAccountName={username})";
                search.PropertiesToLoad.Add("givenName");
                search.PropertiesToLoad.Add("sn");
                search.PropertiesToLoad.Add("manager");
                search.PropertiesToLoad.Add("employeeID");

                SearchResult result = search.FindOne();

                if (result != null)
                {
                    string managerFullField;
                    if (result.Properties.Contains("givenName") && result.Properties["givenName"].Count > 0)
                    {
                        userInfo[0] = result.Properties["givenName"][0].ToString();
                    }
                    else { throw new Exception($"error while retrieving user:\"{username}\" givenName"); };
                    if (result.Properties.Contains("sn") && result.Properties["sn"].Count > 0)
                    {
                        userInfo[1] = result.Properties["sn"][0].ToString();
                    }
                    else { throw new Exception($"error while retrieving user:\"{username}\" sn"); };
                    if (result.Properties.Contains("employeeID") && result.Properties["employeeID"].Count > 0)
                    {
                        userInfo[3] = result.Properties["employeeID"][0].ToString();
                    }
                    else { throw new Exception($"error while retrieving user:\"{username}\" employeeID"); };
                    if (result.Properties.Contains("manager") && result.Properties["manager"].Count > 0)
                    {
                        managerFullField = result.Properties["manager"][0].ToString();
                        string managerDN = GetManagerCN(managerFullField);
                        userInfo[2] = GetUsernameByDisplayName(managerDN);
                    }
                    else
                    {
                        userInfo[2] = "";
                    }
                }
                return userInfo;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("UserInfo could not be retrieved", ex);
            }

        }
        public string GetUsernameByDisplayName(string displayName)
        {//Get username using displayname
            DirectoryEntry entry = new DirectoryEntry(_path);
            DirectorySearcher search = new DirectorySearcher(entry);
            search.Filter = $"(displayName={displayName})";
            search.PropertiesToLoad.Add("SAMAccountName");

            SearchResult result = search.FindOne();

            if (result != null)
            {
                string username = result.Properties["SAMAccountName"][0].ToString();
                return username;
            }
            else { return ""; }
        }
        public string GetManagerCN(string managerFullField)
        {//select manager display name from full manager Field of Users

            string cnPrefix = "CN=";
            int cnIndex = managerFullField.IndexOf(cnPrefix);
            if (cnIndex != -1)
            {
                string cnValue = managerFullField.Substring(cnIndex + cnPrefix.Length).Split(',')[0];
                return cnValue;
            }
            else { return ""; }
        }

    }
}
