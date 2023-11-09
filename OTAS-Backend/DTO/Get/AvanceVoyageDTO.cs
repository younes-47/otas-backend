namespace OTAS.DTO.Get
{
    public class AvanceVoyageDTO
    {
        public int Id { get; set; }

        public string RequesterUsername { get; set; } = null!;

        public int OrdreMissionId { get; set; }

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public int? ConfirmationNumber { get; set; }

        public int LatestStatus { get; set; }

        public DateTime? CreateDate { get; set; }

    }
}
