using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Pet
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Gender { get; set; }

    public string? Species { get; set; }

    public string? Breed { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public decimal? Weight { get; set; }

    public Guid? UserId { get; set; }

    public string UserPhoneNumber { get; set; } = null!;

    public byte Vaccinated { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<PetVaccine> PetVaccines { get; set; } = new List<PetVaccine>();

    public virtual User? User { get; set; }

    public virtual ICollection<UserCartItem> UserCartItems { get; set; } = new List<UserCartItem>();

    public virtual ICollection<UserPetVaccine> UserPetVaccines { get; set; } = new List<UserPetVaccine>();
}
