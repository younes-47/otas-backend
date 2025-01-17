﻿namespace OTAS.DTO.Post
{
    public class TripPostDTO
    {
        public string DeparturePlace { get; set; } = null!;

        public string Destination { get; set; } = null!;

        public DateTime DepartureDate { get; set; }

        public DateTime ArrivalDate { get; set; }

        public string TransportationMethod { get; set; } = null!;

        public string Unit { get; set; } = null!;

        public decimal Value { get; set; }

        public decimal HighwayFee { get; set; }
    }
}
