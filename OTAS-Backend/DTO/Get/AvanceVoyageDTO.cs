using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class AvanceVoyageDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        //public int OrdreMissionId { get; set; }

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public string Currency { get; set; } = null!;

        public int? ConfirmationNumber { get; set; }

        public int LatestStatus { get; set; }

        public int? DeciderUserId { get; set; }

        public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual ICollection<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO>();

        public virtual ICollection<TripDTO> Trips { get; set; } = new List<TripDTO>();

    }
}
