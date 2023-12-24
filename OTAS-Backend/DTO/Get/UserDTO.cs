namespace OTAS.DTO.Get
{
    public class UserDTO
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int Role { get; set; }

        public int SuperiorUserId { get; set; }

        public string PreferredLanguage { get; set; } = null!;

    }
}