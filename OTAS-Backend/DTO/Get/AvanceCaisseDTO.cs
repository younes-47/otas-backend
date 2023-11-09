namespace OTAS.DTO.Get
{
    public class AvanceCaisseDTO
    {
        public int Id { get; set; }

        public string RequesterUsername { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public int? ConfirmationNumber { get; set; }

        public int LatestStatus { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
