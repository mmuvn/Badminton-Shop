using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class ServiceRequestStatus
{
    public int ServiceStatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    public virtual ICollection<ServiceStatusHistory> ServiceStatusHistoryNewStatuses { get; set; } = new List<ServiceStatusHistory>();

    public virtual ICollection<ServiceStatusHistory> ServiceStatusHistoryOldStatuses { get; set; } = new List<ServiceStatusHistory>();
}
