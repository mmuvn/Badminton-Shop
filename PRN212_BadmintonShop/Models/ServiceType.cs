using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class ServiceType
{
    public int ServiceTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
