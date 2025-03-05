using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Diagnosis
{
    public int Id { get; set; }

    public string Label { get; set; } = null!;

    public string? Description { get; set; }

    public string? Symptoms { get; set; }

    public string? FirstAid { get; set; }
}
