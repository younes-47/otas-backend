namespace OTAS.DTO.Get
{
    public class AvanceVoyageDeciderViewDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public bool OnBehalf { get; set; }

        public virtual UserInfoDTO Requester { get; set; } = new UserInfoDTO();

        public virtual ActualRequesterDTO? ActualRequester { get; set; }

        public int OrdreMissionId { get; set; }

        public string OrdreMissionDescription { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public string Currency { get; set; } = null!;

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public virtual List<TripDTO> Trips { get; set; } = new List<TripDTO>();
        public virtual List<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO>();
        public virtual List<StatusHistoryDTO> StatusHistory { get; set; } = new List<StatusHistoryDTO>();


    }
}
