using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class AvanceCaisseDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public int? ConfirmationNumber { get; set; }

        public int LatestStatus { get; set; }

        public int? DeciderUserId { get; set; }

        public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }

    }
}
