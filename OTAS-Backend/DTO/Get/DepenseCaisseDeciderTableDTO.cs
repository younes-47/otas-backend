using System.Diagnostics.CodeAnalysis;

namespace OTAS.DTO.Get
{
    public class DepenseCaisseDeciderTableDTO : IEqualityComparer<DepenseCaisseDeciderTableDTO>
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public string NextDeciderUserName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal Total { get; set; }

        public string ReceiptsFileName { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public bool Equals(DepenseCaisseDeciderTableDTO? x, DepenseCaisseDeciderTableDTO? y)
        {
            return x != null && y != null && x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] DepenseCaisseDeciderTableDTO obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
