using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class AppointmentClinicService
{
    public Guid Id { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid ClinicServiceId { get; set; }

    public DateTime? DateGiven { get; set; }

    public string? Notes { get; set; }

    public decimal? Price { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual ClinicService ClinicService { get; set; } = null!;
}
