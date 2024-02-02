namespace OTAS.DTO.Get
{
    public class AvanceCaisseDeciderViewDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public bool OnBehalf { get; set; }

        public virtual UserInfoDTO Requester { get; set; } = new UserInfoDTO();

        public virtual ActualRequesterDTO? ActualRequester { get; set; }

        public string Description { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public string Currency { get; set; } = null!;

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public virtual List<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO>();
        public virtual List<StatusHistoryDTO> StatusHistory { get; set; } = new List<StatusHistoryDTO>();
    }
}
