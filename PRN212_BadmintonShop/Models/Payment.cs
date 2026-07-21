using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }

    public int? ServiceRequestId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public DateTime? PaidAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ServiceRequest? ServiceRequest { get; set; }
}
