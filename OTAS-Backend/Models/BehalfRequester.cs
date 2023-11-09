using System;
using System.Collections.Generic;

namespace OTAS.Models;

public partial class BehalfRequester
{
    public int Id { get; set; }

    public int? AvanceCaisseId { get; set; }

    public int? DepenseCaisseId { get; set; }

    public int? OrdreMissionId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int RegistrationNumber { get; set; }

    public string JobTitle { get; set; } = null!;

    public DateTime HiringDate { get; set; }

    public string Department { get; set; } = null!;

    public string Manager { get; set; } = null!;

    public virtual AvanceCaisse? AvanceCaisse { get; set; }

    public virtual DepenseCaisse? DepenseCaisse { get; set; }

    public virtual OrdreMission? OrdreMission { get; set; }
}
