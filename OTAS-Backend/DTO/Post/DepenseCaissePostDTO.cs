﻿using OTAS.Models;

namespace OTAS.DTO.Post
{
    public class DepenseCaissePostDTO
    {
        public int Id { get; set; } 

        public int UserId { get; set; }

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public Byte[] ReceiptsFile { get; set; } = null!;

        public virtual ActualRequesterPostDTO? ActualRequester { get; set; }

        public virtual List<ExpenseNoCurrencyPostDTO> Expenses { get; set; } = new List<ExpenseNoCurrencyPostDTO>();

    }
}