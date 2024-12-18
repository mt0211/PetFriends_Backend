using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class GuestUser
{
    public Guid Id { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<GuestPet> GuestPets { get; set; } = new List<GuestPet>();
}
