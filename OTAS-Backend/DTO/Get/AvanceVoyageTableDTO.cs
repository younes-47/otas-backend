namespace OTAS.DTO.Get
{
    public class AvanceVoyageTableDTO
    {
        public int Id { get; set; }

        public decimal EstimatedTotal { get; set; }

        public string NextDeciderUserName { get; set; } = null!;

        public bool OnBehalf { get; set; }

        public int OrdreMissionId { get; set; }

        public string OrdreMissionDescription { get; set; } = null!;

        public decimal? ActualTotal { get; set; }

        public string Currency { get; set; } = null!;

        public int? ConfirmationNumber { get; set; }

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }
    }
}
