using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public int OrderStatusId { get; set; }

    public string? ShippingAddress { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Note { get; set; }

    public string? CancelReason { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderStatus OrderStatus { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
