using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Feedback
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? Rating { get; set; }

    public Guid? AppointmentId { get; set; }

    public string? Sentiment { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual User User { get; set; } = null!;
}
