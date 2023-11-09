namespace OTAS.DTO.Get
{
    public class LiquidationDTO
    {
        public int Id { get; set; }

        public string RequesterUsername { get; set; } = null!;

        public int? AvanceVoyageId { get; set; }

        public int? AvanceCaisseId { get; set; }

        public decimal? ActualTotal { get; set; }

        public string? ReceiptsFilePath { get; set; }

        public string? LiquidationOption { get; set; }

        public int? Result { get; set; }

        public int LatestStatus { get; set; }

        public DateTime? CreateDate { get; set; }


    }
}
