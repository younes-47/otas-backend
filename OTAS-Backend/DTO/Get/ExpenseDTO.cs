namespace OTAS.DTO.Get
{
    public class ExpenseDTO
    {
        public int Id { get; set; }

        public int? AvanceVoyageId { get; set; }

        public int? AvanceCaisseId { get; set; }

        public int? DepenseCaisseId { get; set; }

        public int? LiquidationId { get; set; }

        public decimal EstimatedFee { get; set; }

        public decimal? ActualFee { get; set; }

        public string Description { get; set; } = null!;

        public DateTime ExpenseDate { get; set; }

        public string Currency { get; set; } = null!;

    }
}
