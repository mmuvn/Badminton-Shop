using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public class CheckoutLineItem
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
    
    public CartItem? CartItemRef { get; set; }
    public ServiceRequest? ServiceRequestRef { get; set; }
}

public partial class CheckoutView : UserControl
{
    private List<CheckoutLineItem> _items = new();

    public CheckoutView()
    {
        InitializeComponent();
        LoadCart();
    }

    private void LoadCart()
    {
        if (AppState.CurrentUser == null) return;
        
        using var context = new BadmintonShopDbContext();
        var cart = context.Carts
            .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
            .Include(c => c.ServiceRequests).ThenInclude(sr => sr.StringProduct)
            .Include(c => c.ServiceRequests).ThenInclude(sr => sr.ServiceType)
            .FirstOrDefault(c => c.CustomerId == AppState.CurrentUser.UserId);

        _items.Clear();

        if (cart != null)
        {
            // Products
            foreach (var ci in cart.CartItems)
            {
                _items.Add(new CheckoutLineItem
                {
                    Id = ci.CartItemId,
                    Type = "Product",
                    Description = ci.Product.ProductName,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price,
                    CartItemRef = ci
                });
            }

            // Services
            foreach (var sr in cart.ServiceRequests.Where(s => !s.IsPaid))
            {
                _items.Add(new CheckoutLineItem
                {
                    Id = sr.ServiceRequestId,
                    Type = "Service",
                    Description = sr.Description ?? sr.ServiceType.TypeName,
                    Quantity = 1,
                    UnitPrice = sr.Price ?? 0,
                    ServiceRequestRef = sr
                });
            }
        }

        dgCheckoutItems.ItemsSource = null;
        dgCheckoutItems.ItemsSource = _items;

        txtTotal.Text = _items.Sum(i => i.LineTotal).ToString("C");
    }

    private void BtnRemove_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CheckoutLineItem item)
        {
            using var context = new BadmintonShopDbContext();
            if (item.Type == "Product")
            {
                var ci = context.CartItems.Find(item.Id);
                if (ci != null) context.CartItems.Remove(ci);
            }
            else if (item.Type == "Service")
            {
                var sr = context.ServiceRequests.Find(item.Id);
                if (sr != null) context.ServiceRequests.Remove(sr); // or cancel
            }
            context.SaveChanges();
            LoadCart();
        }
    }

    private void BtnPay_Click(object sender, RoutedEventArgs e)
    {
        if (!_items.Any())
        {
            MessageBox.Show("Your cart is empty.");
            return;
        }

        try
        {
            using var context = new BadmintonShopDbContext();
            using var transaction = context.Database.BeginTransaction();

            int customerId = AppState.CurrentUser!.UserId;
            var cart = context.Carts
                .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                .Include(c => c.ServiceRequests)
                .FirstOrDefault(c => c.CustomerId == customerId);

            if (cart == null) throw new Exception("Cart not found.");

            // 1. Create Order
            var orderStatus = context.OrderStatuses.FirstOrDefault(s => s.StatusName == "Pending")?.OrderStatusId ?? 1;
            
            decimal totalAmount = _items.Sum(i => i.LineTotal);
            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                OrderStatusId = orderStatus,
                TotalAmount = totalAmount,
                ShippingAddress = AppState.CurrentUser.Address ?? "Store Pickup"
            };
            context.Orders.Add(order);
            context.SaveChanges(); // get OrderId

            // 2. Add OrderItems & Product Payment
            if (cart.CartItems.Any())
            {
                decimal productTotal = 0;
                foreach (var ci in cart.CartItems)
                {
                    context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.Product.Price
                    });
                    
                    // Deduct stock
                    var prod = context.Products.Find(ci.ProductId);
                    if (prod != null) prod.StockQuantity -= ci.Quantity;
                    
                    productTotal += (ci.Quantity * ci.Product.Price);
                }

                if (productTotal > 0)
                {
                    // No Payment table needed
                }
            }

            // 3. Handle Service Requests & Service Payments
            var unpaidServices = cart.ServiceRequests.Where(s => !s.IsPaid).ToList();
            foreach (var sr in unpaidServices)
            {
                sr.IsPaid = true;
                sr.OrderId = order.OrderId;
            }

            // 4. Clear CartItems
            context.CartItems.RemoveRange(cart.CartItems);

            context.SaveChanges();
            transaction.Commit();
            MessageBox.Show("Payment successful! Order has been created.", "Success");
            LoadCart();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Checkout failed: {ex.Message}", "Error");
        }
    }
}
