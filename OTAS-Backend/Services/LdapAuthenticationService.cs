using OTAS.DTO.Get;
using OTAS.Interfaces.IService;
using System.DirectoryServices;

namespace TMA_App.Services
{
    public class LdapAuthenticationService : ILdapAuthenticationService
    {
        private string _path = "LDAP://dicastalma.com";
        private static readonly string _domain = "dicastalma";
        public LdapAuthenticationService() { }
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
            string[] userInfo = new string[6];
            string domainAndUsername = $"{_domain}\\{username}";

            try
            {
                DirectoryEntry entry = new DirectoryEntry(_path);
                // Create a DirectorySearcher object to search for the user's information.
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = $"(SAMAccountName={username})";
                search.PropertiesToLoad.Add("givenName"); // 0 => First name
                search.PropertiesToLoad.Add("sn");  // 1 => lastname
                search.PropertiesToLoad.Add("manager"); // 2 => manager username
                search.PropertiesToLoad.Add("employeeID"); // 3 => registration number
                search.PropertiesToLoad.Add("title"); // 4 => job title
                search.PropertiesToLoad.Add("department"); // 5 => department

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


                    if (result.Properties.Contains("title") && result.Properties["title"].Count > 0)
                    {
                        userInfo[4] = result.Properties["title"][0].ToString();
                    }
                    else { throw new Exception($"error while retrieving user:\"{username}\" title"); };


                    if (result.Properties.Contains("department") && result.Properties["department"].Count > 0)
                    {
                        userInfo[5] = result.Properties["department"][0].ToString();
                    }
                    else { throw new Exception($"error while retrieving user:\"{username}\" department"); };


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
        public UserInfoDTO GetUserInformation(string username)
        {
            UserInfoDTO userInfo = new();
            try
            {
                DirectoryEntry entry = new DirectoryEntry(_path);
                // Create a DirectorySearcher object to search for the user's information.
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = $"(SAMAccountName={username})";
                search.PropertiesToLoad.Add("givenName"); // first name
                search.PropertiesToLoad.Add("sn"); // last name
                search.PropertiesToLoad.Add("manager"); // manager username
                search.PropertiesToLoad.Add("employeeID"); // registration number
                search.PropertiesToLoad.Add("title"); //  job title
                search.PropertiesToLoad.Add("department"); //  department

                SearchResult result = search.FindOne();

                if (result != null)
                {
                    if (result.Properties.Contains("givenName") && result.Properties["givenName"].Count > 0)
                    {
                        userInfo.FirstName = result.Properties["givenName"][0].ToString();
                    }
                    else 
                    { 
                        throw new Exception($"error while retrieving user:\"{username}\" givenName"); 
                    };

                    if (result.Properties.Contains("sn") && result.Properties["sn"].Count > 0)
                    {
                        userInfo.LastName = result.Properties["sn"][0].ToString();
                    }
                    else 
                    { 
                        throw new Exception($"error while retrieving user:\"{username}\" sn"); 
                    };

                    if (result.Properties.Contains("employeeID") && result.Properties["employeeID"].Count > 0)
                    {
                        userInfo.RegistrationNumber = result.Properties["employeeID"][0].ToString();
                    }
                    else
                    {
                        throw new Exception($"error while retrieving user:\"{username}\" sn");
                    };

                    if (result.Properties.Contains("manager") && result.Properties["manager"].Count > 0)
                    {
                        string managerFullField = result.Properties["manager"][0].ToString();
                        string managerDN = GetManagerCN(managerFullField);
                        userInfo.ManagerUserName = GetUsernameByDisplayName(managerDN);
                    }
                    else
                    {
                        if(userInfo.RegistrationNumber.Trim() != "1")
                        {
                            throw new Exception($"error while retrieving user:\"{username}\" sn");
                        }
                    };

                    if (result.Properties.Contains("title") && result.Properties["title"].Count > 0)
                    {
                        userInfo.JobTitle = result.Properties["title"][0].ToString();
                    }
                    else
                    {
                        throw new Exception($"error while retrieving user:\"{username}\" sn");
                    };

                    if (result.Properties.Contains("department") && result.Properties["department"].Count > 0)
                    {
                        userInfo.Department = result.Properties["department"][0].ToString();
                    }
                    else
                    {
                        throw new Exception($"error while retrieving user:\"{username}\" sn");
                    };
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

        public List<string> GetJobTitles()
        {
            List<string> jobTitles = new();
            try
            {
                DirectoryEntry entry = new(_path);
                DirectorySearcher search = new(entry);
                search.PropertiesToLoad.Add("title"); //  job title


                SearchResultCollection results = search.FindAll();

                if (results.Count > 0)
                {
                    foreach (SearchResult result in results)
                    {
                        if (result.Properties.Contains("title") && result.Properties["title"].Count > 0)
                        {
                            jobTitles.Add(result.Properties["title"][0].ToString());
                        }
                        
                    }

                }
                return jobTitles;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("JobTitles could not be retrieved", ex);
            }
        }

        public List<string> GetDepartments()
        {
            List<string> departments = new();
            try
            {
                DirectoryEntry entry = new(_path);
                DirectorySearcher search = new(entry);
                search.PropertiesToLoad.Add("department"); //  job title


                SearchResultCollection results = search.FindAll();

                if (results.Count > 0)
                {
                    foreach (SearchResult result in results)
                    {
                        if (result.Properties.Contains("department") && result.Properties["department"].Count > 0)
                        {
                            departments.Add(result.Properties["department"][0].ToString());
                        }

                    }

                }
                return departments;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("departments could not be retrieved", ex);
            }
        }
    }
}
