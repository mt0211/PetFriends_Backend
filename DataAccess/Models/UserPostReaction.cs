using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class UserPostReaction
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid PostId { get; set; }

    public bool IsLike { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ForumPost Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
