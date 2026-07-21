using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class RacketDetail
{
    public int ProductId { get; set; }

    public string? Color { get; set; }

    public decimal? MaxTensionLbs { get; set; }

    public decimal? FrameWeightGrams { get; set; }

    public virtual Product Product { get; set; } = null!;
}
