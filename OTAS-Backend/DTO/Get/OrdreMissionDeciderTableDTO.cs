namespace OTAS.DTO.Get
{
    public class OrdreMissionDeciderTableDTO
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public decimal RequestedAmountMAD { get; set; }

        public decimal RequestedAmountEUR { get; set; }

        public string NextDeciderUserName { get; set; } = null!;

        public bool Abroad { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
