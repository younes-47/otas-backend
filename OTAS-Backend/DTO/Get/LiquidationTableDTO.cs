using Microsoft.AspNetCore.Http.Features;

namespace OTAS.DTO.Get
{
    public class LiquidationTableDTO
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public int RequestId { get; set; }

        public string RequestType { get; set; } = null!;

        public string? Description { get; set; }

        public decimal? ActualTotal { get; set; }

        public string Currency { get; set; } = null!;

        public string? ReceiptsFileName { get; set; }

        public int? Result { get; set; }

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }

    }
}
