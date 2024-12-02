using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class OtpVerify
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? OtpCode { get; set; }

    public byte IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public virtual User User { get; set; } = null!;
}
