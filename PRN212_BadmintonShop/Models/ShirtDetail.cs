using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class ShirtDetail
{
    public int ProductId { get; set; }

    public string? Color { get; set; }

    public string? Size { get; set; }

    public string? Material { get; set; }

    public string? SleeveType { get; set; }

    public string? Gender { get; set; }

    public virtual Product Product { get; set; } = null!;
}
