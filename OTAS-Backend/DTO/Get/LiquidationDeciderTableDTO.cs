using System.Diagnostics.CodeAnalysis;

namespace OTAS.DTO.Get
{
    public class LiquidationDeciderTableDTO : IEqualityComparer<LiquidationDeciderTableDTO>
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public int RequestId { get; set; }

        public string RequestType { get; set; } = null!;

        public string? Description { get; set; }

        public decimal ActualTotal { get; set; }

        public string Currency { get; set; } = null!;

        public bool IsDecidable { get; set; }

        public string? ReceiptsFileName { get; set; }

        public decimal Result { get; set; }

        public DateTime CreateDate { get; set; }

        public bool Equals(LiquidationDeciderTableDTO? x, LiquidationDeciderTableDTO? y)
        {
            return x != null && y != null && x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] LiquidationDeciderTableDTO obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
