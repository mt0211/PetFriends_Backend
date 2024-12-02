using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Promotion
{
    public Guid Id { get; set; }

    public decimal DiscountRate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public byte IsActive { get; set; }
}
