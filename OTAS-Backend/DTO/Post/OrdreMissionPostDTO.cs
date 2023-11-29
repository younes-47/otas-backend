using OTAS.DTO.Get;
using OTAS.Models;

namespace OTAS.DTO.Post
{
    public class OrdreMissionPostDTO
    {
        public int Id { get; set; } //Need Id when resubmitting in case of return

        public int UserId { get; set; }

        public string Description { get; set; } = null!;

        public bool Abroad { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public int LatestStatus { get; set; } //Default is 99 (draft) but needed in case of submit

        public bool OnBehalf { get; set; }

        public List<TripPostDTO> Trips { get; set; } = new List<TripPostDTO>();

        public List<ExpensePostDTO> Expenses { get; set; } = new List<ExpensePostDTO>();

        public ActualRequesterPostDTO? ActualRequester { get; set; }
    }
}
