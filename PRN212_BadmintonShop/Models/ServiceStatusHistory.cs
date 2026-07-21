using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class ServiceStatusHistory
{
    public int HistoryId { get; set; }

    public int ServiceRequestId { get; set; }

    public int? OldStatusId { get; set; }

    public int NewStatusId { get; set; }

    public int ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Note { get; set; }

    public virtual User ChangedByNavigation { get; set; } = null!;

    public virtual ServiceRequestStatus NewStatus { get; set; } = null!;

    public virtual ServiceRequestStatus? OldStatus { get; set; }

    public virtual ServiceRequest ServiceRequest { get; set; } = null!;
}
