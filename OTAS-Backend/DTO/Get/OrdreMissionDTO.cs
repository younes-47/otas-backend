namespace OTAS.DTO.Get
{
    public class OrdreMissionDTO
    {
        public int Id { get; set; }

        public string RequesterUsername { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Region { get; set; } = null!;

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public int LatestStatus { get; set; }

        public DateTime? CreateDate { get; set; }

        public int? OnBehalf { get; set; }

    }
}
