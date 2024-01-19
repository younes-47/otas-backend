using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class OrdreMissionViewDTO
    {
        public int Id { get; set; }

        public virtual ActualRequesterDTO? RequesterInfo { get; set; }

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public bool Abroad { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public virtual List<StatusHistoryDTO> StatusHistory { get; set; } = new List<StatusHistoryDTO>();

        public virtual List<OrdreMissionAvanceDetailsDTO> AvanceVoyagesDetails { get; set; } = new List<OrdreMissionAvanceDetailsDTO> { };

    }
}
