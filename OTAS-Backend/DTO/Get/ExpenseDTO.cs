﻿namespace OTAS.DTO.Get
{
    public class ExpenseDTO
    {
        public int Id { get; set; }

        //public int? AvanceVoyageId { get; set; }

        //public int? AvanceCaisseId { get; set; }

        //public int? DepenseCaisseId { get; set; }

        public string Currency { get; set; } = null!;

        public decimal EstimatedFee { get; set; }

        public decimal? ActualFee { get; set; }

        public string Description { get; set; } = null!;

        public DateTime ExpenseDate { get; set; }

        public DateTime CreateDate { get; set; }

        /* We need update time here because Trip & Expense don't get tracked in StatusHistory 
         * and we want to know when the user has provided the ActualFee */
        public DateTime? UpdateDate { get; set; }

    }
}
