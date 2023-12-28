namespace OTAS.DTO.Get
{
    public class DepenseCaisseDTO
    {
        public int Id { get; set; }

        //public int UserId { get; set; }

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal Total { get; set; }

        public string ReceiptsFileName { get; set; } = null!;

        public string LatestStatus { get; set; } = null!;

        //public int? DeciderUserId { get; set; }

        //public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }

    }
}
