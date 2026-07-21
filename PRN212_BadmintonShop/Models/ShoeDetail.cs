using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class ShoeDetail
{
    public int ProductId { get; set; }

    public string? Color { get; set; }

    public string? Size { get; set; }

    public virtual Product Product { get; set; } = null!;
}
