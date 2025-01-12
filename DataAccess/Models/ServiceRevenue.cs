using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class ServiceRevenue
{
    public Guid Id { get; set; }

    public Guid ClinicServiceId { get; set; }

    public DateOnly Date { get; set; }

    public decimal Revenue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ClinicService ClinicService { get; set; } = null!;
}
