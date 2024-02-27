using System.Diagnostics.CodeAnalysis;

namespace OTAS.DTO.Get
{
    public class DepenseCaisseDTO : IEqualityComparer<DepenseCaisseDTO>
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal Total { get; set; }

        public string ReceiptsFileName { get; set; } = null!;

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public bool Equals(DepenseCaisseDTO? x, DepenseCaisseDTO? y)
        {
            return x != null && y != null && x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] DepenseCaisseDTO obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
