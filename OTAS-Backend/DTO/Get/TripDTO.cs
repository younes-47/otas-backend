using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class TripDTO
    {
        public int Id { get; set; }

        public int AvanceVoyageId { get; set; }

        public string DeparturePlace { get; set; } = null!;

        public string Destination { get; set; } = null!;

        public DateTime DepartureDate { get; set; }

        public string TransportationMethod { get; set; } = null!;

        public string Unit { get; set; } = null!;

        public decimal Value { get; set; }

        public decimal? HighwayFee { get; set; }

        public decimal EstimatedFee { get; set; }

        public decimal ActualFee { get; set; }

    }
}
