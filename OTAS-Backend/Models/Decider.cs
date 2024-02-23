
namespace OTAS.Models
{
    public class Decider
    {
        public int Id { get; set; }
        public string Level { get; set; } = null!;
        public int UserId { get; set; }
        public string Department { get; set; } = null!;
        public string? SignatureImageName { get; set; }
        public virtual User User { get; set; } = null!;

    }
}
