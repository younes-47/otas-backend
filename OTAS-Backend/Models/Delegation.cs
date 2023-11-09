using System;
using System.Collections.Generic;

namespace OTAS.Models;

public partial class Delegation
{
    public int Id { get; set; }

    public int DeciderUserId { get; set; }

    public int DelegateUserId { get; set; }

    public int IsCancelled { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public virtual User DeciderUser { get; set; } = null!;

    public virtual User DelegateUser { get; set; } = null!;
}
