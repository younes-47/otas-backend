namespace OTAS.DTO.Get
{
    public class StatsRequestsDTO
    {
        public int AllTimeCount { get; set; } = 0;

        public int AllOngoingRequestsCount { get; set; } = 0;

        public int HoursPassedSinceLastRequest { get; set; } = 0;

        public int OrdreMissionsAllTimeCount { get; set; } = 0;

        public int AvanceVoyagesAllTimeCount { get; set; } = 0;

        public int AvanceCaissesAllTimeCount { get; set; } = 0;

        public int DepenseCaissesAllTimeCount { get; set; } = 0;
    }
}
