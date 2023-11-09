using OTAS.DTO.Get;
using OTAS.Models;

namespace OTAS.DTO.Post
{
    public class OrdreMissionPostDTO
    {
        public int UserId { get; set; }

        public string Description { get; set; } = null!;

        public string Region { get; set; } = null!;

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public int LatestStatus { get; set; }

        public int? OnBehalf { get; set; }

        public List<TripPostDTO> Trips { get; set; } = new List<TripPostDTO>();

        public List<ExpensePostDTO> Expenses { get; set; } = new List<ExpensePostDTO>();

    }
}
