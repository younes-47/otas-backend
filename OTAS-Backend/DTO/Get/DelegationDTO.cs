namespace OTAS.DTO.Get
{
    public class DelegationDTO
    {
        public int Id { get; set; }

        public string DeciderUsername { get; set; } = null!;

        public string DelegateUsername { get; set; } = null!;

        public int IsCancelled { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
