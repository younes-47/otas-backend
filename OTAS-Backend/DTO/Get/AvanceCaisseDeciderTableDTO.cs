namespace OTAS.DTO.Get
{
    public class AvanceCaisseDeciderTableDTO
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public string NextDeciderUserName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }
    }
}
