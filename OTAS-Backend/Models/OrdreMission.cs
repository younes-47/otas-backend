
namespace OTAS.Models;

public partial class OrdreMission
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int OnBehalf { get; set; }

    public string Description { get; set; } = null!;

    public int Region { get; set; }

    public DateTime DepartureDate { get; set; }

    public DateTime ReturnDate { get; set; }

    public int LatestStatus { get; set; }

    public int? DeciderUserId { get; set; }

    public string? DeciderComment { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual ICollection<ActualRequester> ActualRequesters { get; set; } = new List<ActualRequester>();

    public virtual ICollection<AvanceVoyage> AvanceVoyages { get; set; } = new List<AvanceVoyage>();

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public virtual User User { get; set; } = null!;

    public virtual StatusCode StatusNavigation { get; set; } = null!;

}
