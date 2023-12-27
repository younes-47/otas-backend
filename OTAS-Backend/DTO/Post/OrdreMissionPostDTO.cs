﻿

namespace OTAS.DTO.Post
{
    public class OrdreMissionPostDTO
    {
        public int Id { get; set; } //Need Id when resubmitting in case of return but it is not required in case of creation

        public string Description { get; set; } = null!;

        public bool Abroad { get; set; }

        public bool OnBehalf { get; set; }

        public List<TripPostDTO> Trips { get; set; } = new List<TripPostDTO>();

        public List<ExpensePostDTO> Expenses { get; set; } = new List<ExpensePostDTO>();

        public ActualRequesterPostDTO? ActualRequester { get; set; }
    }
}
