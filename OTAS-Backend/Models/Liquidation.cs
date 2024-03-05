using System;
using System.Collections.Generic;

namespace OTAS.Models;

public partial class Liquidation
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool OnBehalf { get; set; }

    public int? AvanceVoyageId { get; set; }

    public int? AvanceCaisseId { get; set; }

    public decimal ActualTotal { get; set; }

    public string Currency { get; set; } = null!;

    public string ReceiptsFileName { get; set; } = null!;

    public decimal Result { get; set; }

    public int LatestStatus { get; set; }

    public int? DeciderUserId { get; set; }

    public int? NextDeciderUserId { get; set; }

    public string? DeciderComment { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual AvanceCaisse? AvanceCaisse { get; set; }

    public virtual AvanceVoyage? AvanceVoyage { get; set; }

    public virtual StatusCode LatestStatusNavigation { get; set; } = null!;

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public virtual User User { get; set; } = null!;
    public virtual User? Decider { get; set; }
    public virtual User? NextDecider { get; set; }
    public virtual List<Expense> Expenses { get; set; } = new List<Expense>();
    public virtual List<Trip> Trips { get; set; } = new List<Trip>();
}
