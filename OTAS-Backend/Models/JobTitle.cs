namespace OTAS.Models
{
    public partial class JobTitle
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public virtual ICollection<ActualRequester> ActualRequesters { get; set; } = new List<ActualRequester>();

    }
}
