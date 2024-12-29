using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class ClinicService
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual ICollection<AppointmentClinicService> AppointmentClinicServices { get; set; } = new List<AppointmentClinicService>();
}
