using System;
using System.Collections.Generic;

namespace OTAS.Models;

public partial class AvanceCaisse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Description { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public decimal EstimatedTotal { get; set; }

    public decimal? ActualTotal { get; set; }

    public int? ConfirmationNumber { get; set; }

    public int LatestStatus { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public virtual ICollection<ActualRequester> ActualRequesters { get; set; } = new List<ActualRequester>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<Liquidation> Liquidations { get; set; } = new List<Liquidation>();

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public virtual User User { get; set; } = null!;
}
