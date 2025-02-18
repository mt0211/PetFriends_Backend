using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Vaccine
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int? NumberOfDoses { get; set; }

    public string? Recommendation { get; set; }

    public byte? Status { get; set; }

    public virtual ICollection<PetVaccine> PetVaccines { get; set; } = new List<PetVaccine>();

    public virtual ICollection<VaccineDose> VaccineDoses { get; set; } = new List<VaccineDose>();
}
