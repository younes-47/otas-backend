namespace OTAS.DTO.Get
{
    public class Signatory
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Level { get; set; } = null!;

        public string? SignatureImageName { get; set; }

        public DateTime SignDate { get; set; }
    }
}
