namespace OTAS.DTO.Post
{
    public class TripLiquidationPostDTO
    {
        public int TripId { get; set; }
        public decimal ActualValue { get; set; }
        public decimal ActualHighwayFee { get; set; } = 0.0m;

    }
}
