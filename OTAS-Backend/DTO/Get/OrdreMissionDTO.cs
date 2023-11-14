﻿using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class OrdreMissionDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public int Region { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public int LatestStatus { get; set; }

        public int? DeciderUserId { get; set; }

        public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }

    }
}
