using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class VwWeeklyRevenue
{
    public int? OrderYear { get; set; }

    public int? OrderWeek { get; set; }

    public decimal? TotalRevenue { get; set; }

    public int? OrderCount { get; set; }
}
