using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Vaccine
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PetVaccine> PetVaccines { get; set; } = new List<PetVaccine>();
}
