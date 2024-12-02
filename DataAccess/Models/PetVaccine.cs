using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class PetVaccine
{
    public Guid Id { get; set; }

    public Guid PetId { get; set; }

    public Guid VaccineId { get; set; }

    public DateTime? DateGiven { get; set; }

    public string? Notes { get; set; }

    public virtual Pet Pet { get; set; } = null!;

    public virtual Vaccine Vaccine { get; set; } = null!;
}
