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
        var totalIncome = context.Payments
            .Where(p => p.PaymentStatus == "Completed")
            .Sum(p => (decimal?)p.Amount) ?? 0;
            
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
