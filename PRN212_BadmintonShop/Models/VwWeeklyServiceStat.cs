using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class VwWeeklyServiceStat
{
    public int? RequestYear { get; set; }

    public int? RequestWeek { get; set; }

    public string StatusName { get; set; } = null!;

    public int? RequestCount { get; set; }
}
