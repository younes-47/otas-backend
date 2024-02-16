namespace OTAS.DTO.Get
{
    public class ActualRequesterStaticInfoDTO
    {
        public List<ManagerInfoDTO> Managers { get; set; } = new List<ManagerInfoDTO>();
        public List<string> Departments { get; set; } = new List<string>();
        public List<string> JobTitles { get; set; } = new List<string>();
    }
}
