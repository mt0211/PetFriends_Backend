using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class UserCart
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? Datebook { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<UserCartItem> UserCartItems { get; set; } = new List<UserCartItem>();
}
