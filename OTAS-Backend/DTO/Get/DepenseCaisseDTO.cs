namespace OTAS.DTO.Get
{
    public class DepenseCaisseDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal Total { get; set; }

        public string ReceiptsFilePath { get; set; } = null!;

        public int? ConfirmationNumber { get; set; }

        public int LatestStatus { get; set; }

        public DateTime? CreateDate { get; set; }

    }
}
