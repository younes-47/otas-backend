using System.Diagnostics.CodeAnalysis;

namespace OTAS.DTO.Get
{
    public class AvanceCaisseDeciderTableDTO : IEqualityComparer<AvanceCaisseDeciderTableDTO>
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public bool IsDecidable { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public DateTime CreateDate { get; set; }

        public bool Equals(AvanceCaisseDeciderTableDTO? x, AvanceCaisseDeciderTableDTO? y)
        {
            return x != null && y != null && x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] AvanceCaisseDeciderTableDTO obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
