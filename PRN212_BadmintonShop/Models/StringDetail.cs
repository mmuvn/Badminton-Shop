using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class StringDetail
{
    public int ProductId { get; set; }

    public string? Color { get; set; }

    public byte Durability { get; set; }

    public byte Repulsion { get; set; }

    public byte Sound { get; set; }

    public byte Control { get; set; }

    public byte ShockAbsorption { get; set; }

    public virtual Product Product { get; set; } = null!;
}
