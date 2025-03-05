using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class AppointmentPromotion
{
    public Guid Id { get; set; }

    public Guid? AppointmentId { get; set; }

    public Guid? PromotionId { get; set; }

    public decimal? DiscountAmount { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual Promotion? Promotion { get; set; }
}
