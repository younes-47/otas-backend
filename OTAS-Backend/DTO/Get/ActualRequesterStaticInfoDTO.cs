namespace OTAS.DTO.Get
{
    public class ActualRequesterStaticInfoDTO
    {
        public List<string> ManagersUsernames { get; set; } = new List<string>();
        public List<string> Departments { get; set; } = new List<string>();
        public List<string> JobTitles { get; set; } = new List<string>();
    }
}
