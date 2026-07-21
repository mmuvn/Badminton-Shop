using System;
using System.Collections.Generic;

namespace PRN212_BadmintonShop.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int CustomerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User Customer { get; set; } = null!;
}
