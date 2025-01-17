﻿using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class AvanceCaisseViewDTO
    {
        public int Id { get; set; }

        public virtual ActualRequesterDTO? RequesterInfo { get; set; }

        public bool OnBehalf { get; set; }

        public string? DeciderComment { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public decimal? ActualTotal { get; set; }

        //public int? ConfirmationNumber { get; set; }

        public string LatestStatus { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public virtual List<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO>();

        public virtual List<StatusHistoryDTO> StatusHistory { get; set; } = new List<StatusHistoryDTO>();

    }
}
