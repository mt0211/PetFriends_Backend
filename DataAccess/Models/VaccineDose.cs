using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class VaccineDose
{
    public Guid Id { get; set; }

    public Guid VaccineId { get; set; }

    public int? DoseNumber { get; set; }

    public int? DaysAfterPrevious { get; set; }

    public virtual Vaccine Vaccine { get; set; } = null!;
}
