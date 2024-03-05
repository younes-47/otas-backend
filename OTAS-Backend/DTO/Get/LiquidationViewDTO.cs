using Microsoft.EntityFrameworkCore.Diagnostics;

namespace OTAS.DTO.Get
{
    public class LiquidationViewDTO
    {
        public int Id { get; set; }

        public int RequestId { get; set; }

        public string RequestType { get; set; } = null!;

        public bool OnBehalf {  get; set; }

        public decimal ActualTotal { get; set; }

        public decimal Result {  get; set; }

        public byte[] ReceiptsFile { get; set; } = null!;

        public string ReceiptsFileName { get; set; } = null!;

        public string LatestStatus { get; set; } = null!;

        public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }

        public LiquidationRequestDetailsDTO RequestDetails { get; set; } = null!;

        public virtual List<StatusHistoryDTO> StatusHistory { get; set; } = new List<StatusHistoryDTO>();

        public virtual List<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO>();

        public virtual List<TripDTO> Trips { get; set; } = new List<TripDTO>();

    }
}
