using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class UserCartItem
{
    public Guid Id { get; set; }

    public Guid? CartId { get; set; }

    public Guid? ClinicServiceId { get; set; }

    public Guid? PetId { get; set; }

    public decimal? Price { get; set; }

    public virtual UserCart? Cart { get; set; }

    public virtual ClinicService? ClinicService { get; set; }

    public virtual Pet? Pet { get; set; }
}
