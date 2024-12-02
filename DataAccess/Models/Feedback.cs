using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Feedback
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
