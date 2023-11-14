namespace OTAS.DTO.Post
{
    public class DecisionOnRequestPostDTO
    {
        public int RequestId { get; set; }

        public int DeciderUserId { get; set; }

        public string? DeciderComment { get; set; }

        public int Decision { get; set; }
    }
}
