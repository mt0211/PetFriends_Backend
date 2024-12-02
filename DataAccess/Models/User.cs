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

    public Guid? OwnerId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();

    public virtual ICollection<OtpVerify> OtpVerifies { get; set; } = new List<OtpVerify>();

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
