using OTAS.Models;
using System.Diagnostics.CodeAnalysis;

namespace OTAS.DTO.Get
{
    public class AvanceCaisseDTO : IEqualityComparer<AvanceCaisseDTO>
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public int? ConfirmationNumber { get; set; }

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public bool Equals(AvanceCaisseDTO? x, AvanceCaisseDTO? y)
        {
            return x != null && y != null && x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] AvanceCaisseDTO obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
