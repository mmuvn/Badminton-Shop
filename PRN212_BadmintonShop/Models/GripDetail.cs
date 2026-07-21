using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class GripDetail
{
    public int ProductId { get; set; }

    public decimal? ThicknessMm { get; set; }

    public string? Material { get; set; }

    public string? Color { get; set; }

    public virtual Product Product { get; set; } = null!;
}
