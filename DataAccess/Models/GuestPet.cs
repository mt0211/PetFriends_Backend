using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class GuestPet
{
    public Guid Id { get; set; }

    public Guid GuestUserId { get; set; }

    public string? Name { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Gender { get; set; }

    public string? Species { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual GuestUser GuestUser { get; set; } = null!;
}
