using System.Diagnostics.CodeAnalysis;

namespace OTAS.DTO.Get
{
    public class AvanceVoyageDeciderTableDTO : IEqualityComparer<AvanceVoyageDeciderTableDTO>
    {
        public int Id { get; set; }

        public decimal EstimatedTotal { get; set; }

        public bool OnBehalf { get; set; }

        public int OrdreMissionId { get; set; }

        public string OrdreMissionDescription { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public bool IsDecidable { get; set; }


        public DateTime CreateDate { get; set; }

        public bool Equals(AvanceVoyageDeciderTableDTO? x, AvanceVoyageDeciderTableDTO? y)
        {
            return x != null && y != null && x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] AvanceVoyageDeciderTableDTO obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
