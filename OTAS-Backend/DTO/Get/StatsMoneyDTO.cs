namespace OTAS.DTO.Get
{
    public class StatsMoneyDTO
    {
        public decimal AllTimeAmountMAD { get; set; } = 0.0m;
        public decimal AllTimeAmountEUR { get; set; } = 0.0m;

        public decimal AvanceVoyagesAllTimeAmountMAD { get; set; } = 0.0m;
        public decimal AvanceVoyagesAllTimeAmountEUR { get; set; } = 0.0m;

        public decimal AvanceCaissesAllTimeAmountMAD { get; set; } = 0.0m;
        public decimal AvanceCaissesAllTimeAmountEUR { get; set; } = 0.0m;

        public decimal DepenseCaissesAllTimeAmountMAD { get; set; } = 0.0m;
        public decimal DepenseCaissesAllTimeAmountEUR { get; set; } = 0.0m;
    }
}