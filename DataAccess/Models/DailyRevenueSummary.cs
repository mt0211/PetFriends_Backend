using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class DailyRevenueSummary
{
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    public decimal TotalRevenue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
