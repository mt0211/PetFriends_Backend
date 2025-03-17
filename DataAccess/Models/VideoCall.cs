using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class VideoCall
{
    public Guid Id { get; set; }

    public Guid CallerId { get; set; }

    public Guid ReceiverId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string Status { get; set; } = null!;

    public int? Duration { get; set; }

    public string CallType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User Caller { get; set; } = null!;

    public virtual User Receiver { get; set; } = null!;
}
