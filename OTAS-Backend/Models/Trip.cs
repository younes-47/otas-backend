using System;
using System.Collections.Generic;

namespace OTAS.Models;

public partial class Trip
{
    public int Id { get; set; }

    public int AvanceVoyageId { get; set; }

    public string DeparturePlace { get; set; } = null!;

    public string Destination { get; set; } = null!;

    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }

    public string TransportationMethod { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public decimal Value { get; set; }

    public decimal HighwayFee { get; set; }

    public decimal EstimatedFee { get; set; }

    public decimal? ActualFee { get; set; }

    public DateTime CreateDate { get; set; }

    /* We need update time here because Trip & Expense don't get tracked in StatusHistory 
     * and we want to know when the user has provided the ActualFee */
    public DateTime? UpdateDate { get; set; }

    public virtual AvanceVoyage AvanceVoyage { get; set; } = null!;
}
