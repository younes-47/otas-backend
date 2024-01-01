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

    public decimal? ActualTotal { get; set; }

    public string? ReceiptsFileName { get; set; }

    public int? Result { get; set; }

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
}
