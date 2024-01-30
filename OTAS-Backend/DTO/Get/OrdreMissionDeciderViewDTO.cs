namespace OTAS.DTO.Get
{
    public class OrdreMissionDeciderViewDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public bool OnBehalf { get; set; }

        public virtual UserInfoDTO Requester { get; set; } = new UserInfoDTO();

        public virtual ActualRequesterDTO? ActualRequester { get; set; }

        public string Description { get; set; } = null!;

        public bool Abroad { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public virtual List<StatusHistoryDTO> StatusHistory { get; set; } = new List<StatusHistoryDTO>();

        public virtual List<OrdreMissionAvanceDetailsDTO> AvanceVoyagesDetails { get; set; } = new List<OrdreMissionAvanceDetailsDTO> { };
    }
}
