using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class ForumPost
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? PostContent { get; set; }

    public byte? Status { get; set; }

    public int? LikeCount { get; set; }

    public int? DislikeCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<ForumComment> ForumComments { get; set; } = new List<ForumComment>();

    public virtual User? User { get; set; }

    public virtual ICollection<UserPostReaction> UserPostReactions { get; set; } = new List<UserPostReaction>();
}
