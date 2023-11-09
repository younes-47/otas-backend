using OTAS.DTO.Get;
using OTAS.Models;

namespace OTAS.DTO.Post
{
    public class OrdreMissionRequestDTO
    {
        public int UserId { get; set; }

        public string Description { get; set; } = null!;

        public string Region { get; set; } = null!;

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public int LatestStatus { get; set; }

        public int? OnBehalf { get; set; }

        public ICollection<OrdreMissionTripOnRequestDTO> Trips { get; set; } = new List<OrdreMissionTripOnRequestDTO>();

        public ICollection<OrdreMissionExpenseOnRequestDTO> Expenses { get; set; } = new List<OrdreMissionExpenseOnRequestDTO>();

    }
}
