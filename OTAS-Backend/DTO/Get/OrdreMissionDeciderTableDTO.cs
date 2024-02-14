using OTAS.Models;
using System.Diagnostics.CodeAnalysis;

namespace OTAS.DTO.Get
{
    public class OrdreMissionDeciderTableDTO :  IEqualityComparer<OrdreMissionDeciderTableDTO>
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public decimal RequestedAmountMAD { get; set; }

        public decimal RequestedAmountEUR { get; set; }

        public bool Abroad { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsDecidable { get; set; }


        public bool Equals(OrdreMissionDeciderTableDTO? x, OrdreMissionDeciderTableDTO? y)
        {
            return x != null && y != null && x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] OrdreMissionDeciderTableDTO obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
