using System.Linq;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public partial class CustomerHistoryView : UserControl
{
    public CustomerHistoryView()
    {
        InitializeComponent();
        LoadHistory();
    }

    private void LoadHistory()
    {
        if (AppState.CurrentUser == null) return;
        int customerId = AppState.CurrentUser.UserId;

        using var context = new BadmintonShopDbContext();
        
        // Load Orders with their OrderItems and Product details
        var orders = context.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();
            
        dgOrders.ItemsSource = orders;

        // Load Service Requests
        var services = context.ServiceRequests
            .Include(s => s.ServiceStatus)
            .Include(s => s.ServiceType)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.RequestedDate)
            .ToList();
            
        dgServices.ItemsSource = services;
    }
}
