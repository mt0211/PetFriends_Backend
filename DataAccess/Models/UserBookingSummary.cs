using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class UserBookingSummary
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public int? NumOfBook { get; set; }

    public decimal? Amount { get; set; }

    public virtual User? User { get; set; }
}
