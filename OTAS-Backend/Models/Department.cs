namespace OTAS.Models
{
    public partial class Department
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<ActualRequester> ActualRequesters { get; set; } = new List<ActualRequester>();
    }
}
