using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class AvanceCaisseFullDetailsDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public int? ConfirmationNumber { get; set; }

        public string LatestStatus { get; set; } = null!;

        //public int? DeciderUserId { get; set; }

        //public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual ActualRequesterDTO? ActualRequester { get; set; }

        public virtual ICollection<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO>();

        public virtual ICollection<StatusHistoryDTO> StatusHistories { get; set; } = new List<StatusHistoryDTO>();

        public virtual UserDTO User { get; set; } = null!;

    }
}
