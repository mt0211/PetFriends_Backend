using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class ClinicService
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? CreateAt { get; set; }

    public Guid? Category { get; set; }

    public decimal? Price { get; set; }

    public string? Status { get; set; }

    public string? EstimateTime { get; set; }

    public decimal? DiscountAmount { get; set; }

    public DateTime? DiscountFrom { get; set; }

    public DateTime? DiscountTo { get; set; }

    public string? Image { get; set; }

    public decimal? DiscountedPrice { get; set; }

    public virtual ICollection<AppointmentClinicService> AppointmentClinicServices { get; set; } = new List<AppointmentClinicService>();

    public virtual Category? CategoryNavigation { get; set; }
}
