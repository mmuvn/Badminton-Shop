using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class ServiceRequest
{
    public int ServiceRequestId { get; set; }

    public int CustomerId { get; set; }

    public string? RacketBrand { get; set; }

    public string? RacketModel { get; set; }

    public int ServiceTypeId { get; set; }

    public int? StringProductId { get; set; }

    public decimal? RequestedTension { get; set; }

    public string? Description { get; set; }

    public int ServiceStatusId { get; set; }

    public int? AssignedStaffId { get; set; }

    public string? CancelReason { get; set; }

    public decimal? Price { get; set; }

    public DateTime RequestedDate { get; set; }

    public DateTime? StartedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? CartId { get; set; }

    public bool IsPaid { get; set; }

    public int? OrderId { get; set; }

    public virtual User? AssignedStaff { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual ServiceRequestStatus ServiceStatus { get; set; } = null!;

    public virtual ICollection<ServiceStatusHistory> ServiceStatusHistories { get; set; } = new List<ServiceStatusHistory>();

    public virtual ServiceType ServiceType { get; set; } = null!;

    public virtual Product? StringProduct { get; set; }
}
