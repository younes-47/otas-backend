using OTAS.Models;

namespace OTAS.DTO.Post
{
    public class ActualRequesterPostDTO
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public int RegistrationNumber { get; set; }

        public string JobTitle { get; set; } = null!;

        public DateTime HiringDate { get; set; }

        public string Department { get; set; } = null!;

        public string Manager { get; set; } = null!;

    }
}
