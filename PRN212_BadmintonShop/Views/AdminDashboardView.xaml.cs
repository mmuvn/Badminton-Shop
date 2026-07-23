using System.Linq;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public partial class AdminDashboardView : UserControl
{
    public AdminDashboardView()
    {
        InitializeComponent();
        LoadDashboardData();
    }

    private void LoadDashboardData()
    {
        using var context = new BadmintonShopDbContext();

        // Top Cards
        // Total Income: sum of Paid orders and Paid services that are not already part of an order.
        // Wait, CheckoutView sets OrderId for paid services, so we just sum TotalAmount of Paid/Completed orders.
        var totalIncome = context.Orders
            .Where(o => o.OrderStatus.StatusName == "Paid" || o.OrderStatus.StatusName == "Completed")
            .Sum(o => (decimal?)o.TotalAmount) ?? 0;
            
        txtTotalIncome.Text = totalIncome.ToString("C");

        var completedServices = context.ServiceRequests
            .Count(sr => sr.ServiceStatus.StatusName == "Done");
            
        txtCompletedServices.Text = completedServices.ToString();

        // DataGrids
        dgWeeklyRevenue.ItemsSource = context.VwWeeklyRevenues
            .OrderByDescending(x => x.OrderYear)
            .ThenByDescending(x => x.OrderWeek)
            .ToList();

        dgWeeklyTopProducts.ItemsSource = context.VwWeeklyTopProducts
            .OrderByDescending(x => x.OrderYear)
            .ThenByDescending(x => x.OrderWeek)
            .ThenByDescending(x => x.Revenue)
            .ToList();

        dgWeeklyServiceStats.ItemsSource = context.VwWeeklyServiceStats
            .OrderByDescending(x => x.RequestYear)
            .ThenByDescending(x => x.RequestWeek)
            .ToList();
    }
}
