using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class OrdreMissionDTO
    {
        public int Id { get; set; }

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public string NextDeciderUserName { get; set; } = null!;

        public bool Abroad { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }

    }
}
