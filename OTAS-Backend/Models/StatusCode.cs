using System;
using System.Collections.Generic;

namespace OTAS.Models;

public partial class StatusCode
{
    public int StatusInt { get; set; }

    public string? StatusString { get; set; }

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
}
