using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public class OrderQueueRow : INotifyPropertyChanged
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }

    private string _selectedStatus = "";
    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            _selectedStatus = value;
            OnPropertyChanged(nameof(SelectedStatus));
            OnPropertyChanged(nameof(ShowCancelReason));
            OnPropertyChanged(nameof(IsSaveEnabled));
        }
    }

    private string _cancelReason = "";
    public string CancelReason
    {
        get => _cancelReason;
        set
        {
            _cancelReason = value;
            OnPropertyChanged(nameof(CancelReason));
            OnPropertyChanged(nameof(IsSaveEnabled));
        }
    }

    public bool ShowCancelReason => SelectedStatus == "Cancelled";

    public bool IsSaveEnabled =>
        SelectedStatus == "Completed" || SelectedStatus == "Paid" ||
        (SelectedStatus == "Cancelled" && !string.IsNullOrWhiteSpace(CancelReason));

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class StaffOrderQueueView : UserControl
{
    public StaffOrderQueueView()
    {
        InitializeComponent();
        LoadOrders();
    }

    private void LoadOrders()
    {
        using var context = new BadmintonShopDbContext();

        var orders = context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .Where(o => (o.OrderStatus.StatusName == "Pending" || o.OrderStatus.StatusName == "Paid")
                        && o.OrderItems.Any())
            .OrderBy(o => o.OrderDate)
            .ToList();

        var rows = orders.Select(o => new OrderQueueRow
        {
            OrderId = o.OrderId,
            CustomerName = o.Customer.FullName,
            OrderDate = o.OrderDate,
            TotalAmount = o.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice),
            SelectedStatus = o.OrderStatus.StatusName
        }).ToList();

        dgOrders.ItemsSource = rows;
    }

    private void BtnSaveOrder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not OrderQueueRow row) return;

        if (row.SelectedStatus == "Cancelled" && string.IsNullOrWhiteSpace(row.CancelReason))
        {
            MessageBox.Show("A cancel reason is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var context = new BadmintonShopDbContext();
            var order = context.Orders.First(o => o.OrderId == row.OrderId);
            var status = context.OrderStatuses.First(s => s.StatusName == row.SelectedStatus);

            order.OrderStatusId = status.OrderStatusId;
            order.CancelReason = row.SelectedStatus == "Cancelled" ? row.CancelReason : null;

            context.SaveChanges();

            LoadOrders();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error Updating Order", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}