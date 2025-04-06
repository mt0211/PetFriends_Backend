using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string? Email { get; set; }

    public byte[]? Password { get; set; }

    public byte[]? Salt { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? Gender { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime? Dob { get; set; }

    public string? FullName { get; set; }

    public string? Role { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastLoggedIn { get; set; }

    public string? TypeGroup { get; set; }

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<ChatMessage> ChatMessageReceivers { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatMessage> ChatMessageSenders { get; set; } = new List<ChatMessage>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<ForumComment> ForumComments { get; set; } = new List<ForumComment>();

    public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();

    public virtual ICollection<OtpVerify> OtpVerifies { get; set; } = new List<OtpVerify>();

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

    public virtual ICollection<UserBookingSummary> UserBookingSummaries { get; set; } = new List<UserBookingSummary>();

    public virtual ICollection<UserCart> UserCarts { get; set; } = new List<UserCart>();

    public virtual ICollection<UserPostReaction> UserPostReactions { get; set; } = new List<UserPostReaction>();

    public virtual ICollection<VideoCall> VideoCallCallers { get; set; } = new List<VideoCall>();

    public virtual ICollection<VideoCall> VideoCallReceivers { get; set; } = new List<VideoCall>();
}
