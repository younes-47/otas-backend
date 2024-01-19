using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class AvanceVoyageViewDTO
    {
        public int Id { get; set; }

        public ActualRequesterDTO? RequesterInfo { get; set; }

        public bool OnBehalf { get; set; }

        public int OrdreMissionId { get; set; }

        public string OrdreMissionDescription { get; set; } = null!;
        
        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public string Currency { get; set; } = null!;

        public string LatestStatus { get; set; } = null!;

        public virtual List<StatusHistoryDTO> StatusHistory { get; set; } = new List<StatusHistoryDTO>();

    }
}
