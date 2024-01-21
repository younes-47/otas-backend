
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

    public string RegistrationNumber { get; set; } = null!;

    public int JobTitleId { get; set; }

    public string Department { get; set; } = null!;

    public int ManagerUserId { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual JobTitle JobTitle { get; set; } = null!;

    public virtual AvanceCaisse? AvanceCaisse { get; set; }

    public virtual DepenseCaisse? DepenseCaisse { get; set; }

    public virtual User OrderingUser { get; set; } = null!;

    public virtual User Manager { get; set; } = null!;

    public virtual OrdreMission? OrdreMission { get; set; }
}
