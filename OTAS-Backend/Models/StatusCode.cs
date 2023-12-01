
namespace OTAS.Models;

public partial class StatusCode
{
    public int StatusInt { get; set; }

    public string StatusString { get; set; } = null!;

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
    public virtual ICollection<AvanceCaisse> AvanceCaisses { get; set; } = new List<AvanceCaisse>();
    public virtual ICollection<AvanceVoyage> AvanceVoyages { get; set; } = new List<AvanceVoyage>();
    public virtual ICollection<OrdreMission> OrdreMissions { get; set; } = new List<OrdreMission>();
    public virtual ICollection<Liquidation> Liquidations { get; set; } = new List<Liquidation>();
    public virtual ICollection<DepenseCaisse> DepenseCaisses { get; set; } = new List<DepenseCaisse>();

}
