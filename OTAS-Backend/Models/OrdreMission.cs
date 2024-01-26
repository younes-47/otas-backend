using System;
using System.Collections.Generic;

namespace OTAS.Models;

public partial class OrdreMission
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool OnBehalf { get; set; }

    public string Description { get; set; } = null!;

    public bool Abroad { get; set; }

    public DateTime DepartureDate { get; set; }

    public DateTime ReturnDate { get; set; }

    public int LatestStatus { get; set; }

    public int? DeciderUserId { get; set; }

    public int? NextDeciderUserId { get; set; }

    public string? DeciderComment { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual ActualRequester? ActualRequester { get; set; }

    public virtual ICollection<AvanceVoyage> AvanceVoyages { get; set; } = new List<AvanceVoyage>();

    public virtual StatusCode LatestStatusString { get; set; } = null!;

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public virtual User User { get; set; } = null!;

    public virtual User? NextDecider { get; set; }

    public virtual User? Decider { get; set; }
}
