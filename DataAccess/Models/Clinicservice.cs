using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Clinicservice
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
