﻿namespace OTAS.Models;

public partial class AvanceVoyage
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool OnBehalf { get; set; }

    public int OrdreMissionId { get; set; }

    public decimal EstimatedTotal { get; set; }

    public decimal ActualTotal { get; set; }

    public string Currency { get; set; } = null!;

    public string? AdvanceOption { get; set; }

    public int? ConfirmationNumber { get; set; }

    public int LatestStatus { get; set; }

    public int? DeciderUserId { get; set; }

    public int? NextDeciderUserId { get; set; }

    public string? DeciderComment { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual StatusCode LatestStatusNavigation { get; set; } = null!;

    public virtual Liquidation? Liquidation { get; set; }

    public virtual OrdreMission OrdreMission { get; set; } = null!;

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();

    public virtual User User { get; set; } = null!;
    public virtual User? Decider { get; set; }
    public virtual User? NextDecider { get; set; }
}
