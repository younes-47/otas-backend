
namespace OTAS.Models;

public partial class ActualRequester
{
    public int Id { get; set; }

    public int OrderingUserId { get; set; }

    public int? AvanceCaisseId { get; set; }

    public int? DepenseCaisseId { get; set; }

    public int? OrdreMissionId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int RegistrationNumber { get; set; }

    public string JobTitle { get; set; } = null!;

    public DateTime HiringDate { get; set; }

    public string Department { get; set; } = null!;

    public int Manager { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual AvanceCaisse? AvanceCaisse { get; set; }

    public virtual DepenseCaisse? DepenseCaisse { get; set; }

    public virtual User OrderingUser { get; set; } = null!;

    public virtual OrdreMission? OrdreMission { get; set; }
}
