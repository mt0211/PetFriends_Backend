using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Appointment
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid PetId { get; set; }

    public Guid ClinicServiceId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public virtual Clinicservice ClinicService { get; set; } = null!;

    public virtual Pet Pet { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
