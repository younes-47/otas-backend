

namespace OTAS.DTO.Post
{
    public class OrdreMissionPostDTO
    {
        public string Description { get; set; } = null!;

        public bool Abroad { get; set; }

        public bool OnBehalf { get; set; }

        public List<TripPostDTO> Trips { get; set; } = new List<TripPostDTO>();

        public List<ExpensePostDTO> Expenses { get; set; } = new List<ExpensePostDTO>();

        public ActualRequesterPostDTO? ActualRequester { get; set; }
    }
}
