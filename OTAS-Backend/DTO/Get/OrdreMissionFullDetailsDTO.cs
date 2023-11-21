using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class OrdreMissionFullDetailsDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        //public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public bool Abroad { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public int LatestStatus { get; set; }

        //public int? DeciderUserId { get; set; }

        //public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual UserDTO User { get; set; } = null!;

        public virtual ActualRequesterDTO? ActualRequester { get; set; }

        public virtual ICollection<AvanceVoyageDTO> AvanceVoyages { get; set; } = new List<AvanceVoyageDTO>();

        public virtual ICollection<StatusHistoryDTO> StatusHistories { get; set; } = new List<StatusHistoryDTO>();

    }
}
