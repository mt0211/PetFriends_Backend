using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Activity
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? UserId { get; set; }

    public Guid? AppointmentId { get; set; }

    public Guid? PetId { get; set; }

    public Guid? ClinicServiceId { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual ClinicService? ClinicService { get; set; }

    public virtual Pet? Pet { get; set; }

    public virtual User? User { get; set; }
}
