using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Appointment
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? PetId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public Guid? GuestUserId { get; set; }

    public Guid? GuestPetId { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? FinalAmount { get; set; }

    public bool? IsReminderSent { get; set; }

    public bool? IsReminder1HourSent { get; set; }

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    public virtual ICollection<AppointmentClinicService> AppointmentClinicServices { get; set; } = new List<AppointmentClinicService>();

    public virtual ICollection<AppointmentPromotion> AppointmentPromotions { get; set; } = new List<AppointmentPromotion>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual GuestPet? GuestPet { get; set; }

    public virtual GuestUser? GuestUser { get; set; }

    public virtual Pet? Pet { get; set; }

    public virtual User? User { get; set; }
}
