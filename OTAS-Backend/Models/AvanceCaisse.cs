
namespace OTAS.Models;

public partial class AvanceCaisse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool OnBehalf { get; set; }

    public string Description { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public decimal EstimatedTotal { get; set; }

    public decimal ActualTotal { get; set; }

    public string? AdvanceOption { get; set; }

    public int? ConfirmationNumber { get; set; }

    public int LatestStatus { get; set; }

    public int? DeciderUserId { get; set; }

    public string? DeciderComment { get; set; }

    public int? NextDeciderUserId { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual ActualRequester? ActualRequester { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual StatusCode LatestStatusNavigation { get; set; } = null!;

    public virtual ICollection<Liquidation> Liquidations { get; set; } = new List<Liquidation>();

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public virtual User User { get; set; } = null!;
    public virtual User? Decider { get; set; }
    public virtual User? NextDecider { get; set; }
}
