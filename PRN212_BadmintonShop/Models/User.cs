using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<ServiceRequest> ServiceRequestAssignedStaffs { get; set; } = new List<ServiceRequest>();

    public virtual ICollection<ServiceRequest> ServiceRequestCustomers { get; set; } = new List<ServiceRequest>();

    public virtual ICollection<ServiceStatusHistory> ServiceStatusHistories { get; set; } = new List<ServiceStatusHistory>();
}
