namespace OTAS.DTO.Get
{
    public class OrdreMissionDocumentDetailsDTO
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime SubmitDate { get; set; }

        public List<Signatory> Signers { get; set; } = new List<Signatory>();

        public virtual List<OrdreMissionAvanceDetailsDTO> AvanceVoyagesDetails { get; set; } = new List<OrdreMissionAvanceDetailsDTO> { };
    }
}
