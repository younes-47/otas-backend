namespace OTAS.DTO.Get
{
    public class StatsDTO
    {

        // Pending approval requests

        public int PendingOrdreMissionsCount { get; set; } = 0;

        public int PendingAvanceVoyagesCount { get; set; } = 0;

        public int PendingAvanceCaissesCount { get; set; } = 0;

        public int PendingDepenseCaissesCount { get; set; } = 0;

        public int PendingLiquidationsCount { get; set; } = 0;

        // Finalized requests

        public int FinalizedAvanceVoyagesCount { get; set; } = 0;

        public decimal FinalizedAvanceVoyagesMADCount { get; set; } = 0.0m;

        public decimal FinalizedAvanceVoyagesEURCount { get; set; } = 0.0m;

        public int FinalizedAvanceCaissesCount { get; set; } = 0;

        public decimal FinalizedAvanceCaissesMADCount { get; set; } = 0.0m;

        public decimal FinalizedAvanceCaissesEURCount { get; set; } = 0.0m;

        public int FinalizedDepenseCaissesCount { get; set; } = 0;

        public decimal FinalizedDepenseCaissesMADCount { get; set; } = 0.0m;

        public decimal FinalizedDepenseCaissesEURCount { get; set; } = 0.0m;

        // Liquidations

        public int ToLiquidateRequestsCount { get; set; } = 0;

        public decimal ToLiquidateRequestsMADCount { get; set; } = 0.0m;

        public decimal ToLiquidateRequestsEURCount { get; set; } = 0.0m;

        public int OngoingLiquidationsCount { get; set; } = 0;

        public decimal OngoingLiquidationsMADCount { get; set; } = 0.0m;

        public decimal OngoingLiquidationsEURCount { get; set; } = 0.0m;

        public int FinalizedLiquidationsCount { get; set; } = 0;

        public decimal FinalizedLiquidationsMADCount { get; set; } = 0.0m;

        public decimal FinalizedLiquidationsEURCount { get; set; } = 0.0m;


    }
}
