using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Appointment
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? PetId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public Guid? GuestUserId { get; set; }

    public Guid? GuestPetId { get; set; }

    public virtual ICollection<AppointmentClinicService> AppointmentClinicServices { get; set; } = new List<AppointmentClinicService>();

    public virtual GuestPet? GuestPet { get; set; }

    public virtual GuestUser? GuestUser { get; set; }

    public virtual Pet? Pet { get; set; }

    public virtual User? User { get; set; }
}
