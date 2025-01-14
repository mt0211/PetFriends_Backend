using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Promotion
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public byte? Type { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? TargetGroup { get; set; }

    public Guid? CategoryId { get; set; }

    public int UsageLimit { get; set; }

    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? DiscountDetail { get; set; }

    public virtual Category? Category { get; set; }
}
