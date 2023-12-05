using System;
using System.Collections.Generic;

namespace OTAS.Models;

public partial class StatusHistory
{
    public int Id { get; set; }

    public int? AvanceVoyageId { get; set; }

    public int? AvanceCaisseId { get; set; }

    public int? DepenseCaisseId { get; set; }

    public int? OrdreMissionId { get; set; }

    public int? LiquidationId { get; set; }

    public int Status { get; set; }

    public int? DeciderUserId { get; set; }

    public string? DeciderComment { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual AvanceCaisse? AvanceCaisse { get; set; }

    public virtual AvanceVoyage? AvanceVoyage { get; set; }

    public virtual User? Decider { get; set; }

    public virtual DepenseCaisse? DepenseCaisse { get; set; }

    public virtual Liquidation? Liquidation { get; set; }

    public virtual OrdreMission? OrdreMission { get; set; }

    public virtual StatusCode StatusNavigation { get; set; } = null!;
}
