﻿using OTAS.Models;

namespace OTAS.DTO.Post
{
    public class ActualRequesterPostDTO
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string RegistrationNumber { get; set; } = null!;

        public string JobTitle { get; set; } = null!;

        public string Department { get; set; } = null!;

        public string ManagerUserName { get; set; } = null!;

    }
}
