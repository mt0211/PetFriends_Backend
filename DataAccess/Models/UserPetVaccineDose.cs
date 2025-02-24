using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class UserPetVaccineDose
{
    public Guid Id { get; set; }

    public Guid? UserPetVaccineId { get; set; }

    public int? DoseNumber { get; set; }

    public DateTime? DateGiven { get; set; }

    public virtual UserPetVaccine? UserPetVaccine { get; set; }
}
