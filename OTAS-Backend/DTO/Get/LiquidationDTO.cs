namespace OTAS.DTO.Get
{
    public class LiquidationDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? AvanceVoyageId { get; set; }

        public int? AvanceCaisseId { get; set; }

        public decimal? ActualTotal { get; set; }

        public string? ReceiptsFilePath { get; set; }

        public string? LiquidationOption { get; set; }

        public int? Result { get; set; }

        public int LatestStatus { get; set; }

        public int? DeciderUserId { get; set; }

        public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }


    }
}
