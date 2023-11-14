namespace OTAS.DTO.Get
{
    public class DelegationDTO
    {
        public int Id { get; set; }

        public int DeciderUserId { get; set; }

        public int DelegateUserId { get; set; }

        public int IsCancelled { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
