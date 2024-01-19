namespace OTAS.DTO.Get
{
    public class OrdreMissionAvanceDetailsDTO
    {
        public int Id { get; set; }

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public string Currency { get; set; } = null!;

        public string LatestStatus { get; set; } = null!;

        public virtual List<TripDTO> Trips { get; set; } = new List<TripDTO>();

        public virtual List<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO> { };

    }
}
