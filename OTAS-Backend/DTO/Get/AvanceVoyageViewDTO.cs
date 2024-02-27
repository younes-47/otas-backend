using OTAS.Models;
using System.Diagnostics.CodeAnalysis;

namespace OTAS.DTO.Get
{
    public class AvanceVoyageViewDTO : IEqualityComparer<AvanceVoyageViewDTO>
    {
        public int Id { get; set; }

        public ActualRequesterDTO? RequesterInfo { get; set; }

        public bool OnBehalf { get; set; }

        public string? DeciderComment { get; set; }

        public int OrdreMissionId { get; set; }

        public string OrdreMissionDescription { get; set; } = null!;
        
        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        public string Currency { get; set; } = null!;

        public string LatestStatus { get; set; } = null!;

        public virtual List<TripDTO> Trips { get; set; } = new List<TripDTO>();
        public virtual List<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO>();
        public virtual List<StatusHistoryDTO> StatusHistory { get; set; } = new List<StatusHistoryDTO>();

        public bool Equals(AvanceVoyageViewDTO? x, AvanceVoyageViewDTO? y)
        {
            return x != null && y != null && x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] AvanceVoyageViewDTO obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
