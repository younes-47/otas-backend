namespace OTAS.DTO.Get
{
    public class LiquidationTableDTO
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public int? AvanceVoyageId { get; set; }

        public string? AvanceVoyageDescription { get; set; }

        public int? AvanceCaisseId { get; set; }

        public string? AvanceCaisseDescription { get; set; }

        public decimal? ActualTotal { get; set; }

        public string? ReceiptsFileName { get; set; }

        public int? Result { get; set; }

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }

    }
}
