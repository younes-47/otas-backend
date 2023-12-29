namespace OTAS.DTO.Post
{
    public class DecisionOnRequestPostDTO
    {
        public int RequestId { get; set; }

        public string? DeciderComment { get; set; }

        public string DecisionString { get; set; } = null!;

        public bool ReturnedToFMByTR { get; set; }

        public bool ReturnedToTRByFM { get; set; }

        public bool ReturnedToRequesterByTR { get; set; }
    }
}
