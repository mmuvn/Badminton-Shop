using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public class ServiceQueueRow : INotifyPropertyChanged
{
    public int ServiceRequestId { get; set; }
    public string CustomerName { get; set; } = "";
    public string ServiceTypeName { get; set; } = "";

    public string DetailText { get; set; } = "";

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

    public bool ShowCancelReason => SelectedStatus == "Cancel";

    public bool IsSaveEnabled =>
        SelectedStatus == "Done" ||
        (SelectedStatus == "Cancel" && !string.IsNullOrWhiteSpace(CancelReason));

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class StaffServiceQueueView : UserControl
{
    public StaffServiceQueueView()
    {
        InitializeComponent();
        LoadRequests();
        Loaded += (s, e) => LoadRequests();
    }

    private void LoadRequests()
    {
        using var context = new BadmintonShopDbContext();

        var requests = context.ServiceRequests
            .Include(s => s.Customer)
            .Include(s => s.ServiceType)
            .Include(s => s.ServiceStatus)
            .Include(s => s.StringProduct).ThenInclude(p => p!.StringDetail)
            .Where(s => s.ServiceStatus.StatusName == "Todo" || s.ServiceStatus.StatusName == "Doing")
            .OrderBy(s => s.RequestedDate)
            .ToList();

        var rows = requests.Select(s => new ServiceQueueRow
        {
            ServiceRequestId = s.ServiceRequestId,
            CustomerName = s.Customer.FullName,
            ServiceTypeName = s.ServiceType.TypeName,
            DetailText = s.ServiceType.TypeName == "New String"
                ? $"String: {s.StringProduct?.ProductName}\nColor: {s.StringProduct?.StringDetail?.Color}\nTension: {s.RequestedTension} lbs"
                : $"Frame Condition:\n{s.Description}",
            SelectedStatus = s.ServiceStatus.StatusName
        }).ToList();

        dgServices.ItemsSource = rows;
    }

    private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not ServiceQueueRow row) return;

        MessageBox.Show(row.DetailText, $"{row.ServiceTypeName} - Request #{row.ServiceRequestId}",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnSaveService_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not ServiceQueueRow row) return;

        if (row.SelectedStatus == "Cancel" && string.IsNullOrWhiteSpace(row.CancelReason))
        {
            MessageBox.Show("A cancel reason is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var context = new BadmintonShopDbContext();
            var request = context.ServiceRequests.First(s => s.ServiceRequestId == row.ServiceRequestId);
            int oldStatusId = request.ServiceStatusId;
            var newStatus = context.ServiceRequestStatuses.First(s => s.StatusName == row.SelectedStatus);

            request.ServiceStatusId = newStatus.ServiceStatusId;
            request.CancelReason = row.SelectedStatus == "Cancel" ? row.CancelReason : null;

            if (row.SelectedStatus == "Done") request.CompletedDate = DateTime.Now;
            if (AppState.CurrentUser != null) request.AssignedStaffId = AppState.CurrentUser.UserId;

            context.ServiceStatusHistories.Add(new ServiceStatusHistory
            {
                ServiceRequestId = row.ServiceRequestId,
                OldStatusId = oldStatusId,
                NewStatusId = newStatus.ServiceStatusId,
                ChangedBy = AppState.CurrentUser!.UserId,
                ChangedAt = DateTime.Now,
                Note = row.CancelReason
            });

            context.SaveChanges();

            LoadRequests();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error Updating Service Request", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}