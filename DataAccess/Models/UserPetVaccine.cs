using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class UserPetVaccine
{
    public Guid Id { get; set; }

    public Guid? PetId { get; set; }

    public Guid? VaccineId { get; set; }

    public string? Name { get; set; }

    public int? NumberOfDoses { get; set; }

    public string? Recommendation { get; set; }

    public virtual Pet? Pet { get; set; }

    public virtual ICollection<UserPetVaccineDose> UserPetVaccineDoses { get; set; } = new List<UserPetVaccineDose>();

    public virtual Vaccine? Vaccine { get; set; }
}
