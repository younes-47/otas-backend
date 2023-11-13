
namespace OTAS.Models;

public partial class Liquidation
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? AvanceVoyageId { get; set; }

    public int? AvanceCaisseId { get; set; }

    public decimal? ActualTotal { get; set; }

    public string? ReceiptsFilePath { get; set; }

    public string? LiquidationOption { get; set; }

    public int? Result { get; set; }

    public int LatestStatus { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual AvanceCaisse? AvanceCaisse { get; set; }

    public virtual AvanceVoyage? AvanceVoyage { get; set; }

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public virtual User User { get; set; } = null!;
    public virtual StatusCode StatusNavigation { get; set; } = null!;

}
