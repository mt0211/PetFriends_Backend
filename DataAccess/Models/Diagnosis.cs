using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Diagnosis
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string ResultImagePath { get; set; } = null!;

    public string Label { get; set; } = null!;

    public double Confidence { get; set; }

    public DateTime? CreatedAt { get; set; }
}
