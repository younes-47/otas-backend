namespace OTAS.DTO.Get
{
    public class UserDTO
    {
        public string Username { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int Role { get; set; }
    }
}