using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class VwWeeklyTopProduct
{
    public int? OrderYear { get; set; }

    public int? OrderWeek { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int? UnitsSold { get; set; }

    public decimal? Revenue { get; set; }
}
