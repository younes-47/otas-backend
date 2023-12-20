namespace OTAS.DTO.Get
{
    public class AvanceVoyageTableDTO
    {
        public int Id { get; set; }

        //public int UserId { get; set; }

        //public int OrdreMissionId { get; set; }

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public string Currency { get; set; } = null!;

        public int? ConfirmationNumber { get; set; }

        public string LatestStatus { get; set; } = null!;

        //public int? DeciderUserId { get; set; }

        //public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
