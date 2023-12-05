﻿using System;
using System.Collections.Generic;

namespace OTAS.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int Role { get; set; }

    public virtual ICollection<ActualRequester> ActualRequesters { get; set; } = new List<ActualRequester>();

    public virtual ICollection<AvanceCaisse> AvanceCaisses { get; set; } = new List<AvanceCaisse>();

    public virtual ICollection<AvanceVoyage> AvanceVoyages { get; set; } = new List<AvanceVoyage>();

    public virtual ICollection<Delegation>? DelegationDeciderUsers { get; set; }

    public virtual ICollection<Delegation>? DelegationDelegateUsers { get; set; }

    public virtual ICollection<DepenseCaisse> DepenseCaisses { get; set; } = new List<DepenseCaisse>();

    public virtual ICollection<Liquidation> Liquidations { get; set; } = new List<Liquidation>();

    public virtual ICollection<OrdreMission> OrdreMissions { get; set; } = new List<OrdreMission>();

    public virtual RoleCode RoleString { get; set; } = null!;

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
}
