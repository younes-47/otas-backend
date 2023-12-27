using OTAS.DTO.Post;

namespace OTAS.DTO.Put
{
    public class OrdreMissionPutDTO
    {
        public int Id { get; set; }

        public string Action { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool Abroad { get; set; }

        public bool OnBehalf { get; set; }

        public List<TripPostDTO> Trips { get; set; } = new List<TripPostDTO>();

        public List<ExpensePostDTO> Expenses { get; set; } = new List<ExpensePostDTO>();

        public ActualRequesterPostDTO? ActualRequester { get; set; }
    }
}
