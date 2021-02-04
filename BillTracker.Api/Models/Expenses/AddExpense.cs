﻿using System;
using System.ComponentModel.DataAnnotations;

namespace BillTracker.Api.Models.Expenses
{
    public class AddExpenseRequest
    {
        [Required]
        public string Name { get; set; }

        public decimal Amount { get; set; }

        public DateTimeOffset? AddedAt { get; set; }
    }
}
