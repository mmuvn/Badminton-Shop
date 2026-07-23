using System.ComponentModel;
using System.Windows;

namespace PRN212_BadmintonShop;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public bool IsCustomer => AppState.CurrentUser?.Role?.RoleName == "Customer";
    public bool IsStaffOrAdmin => AppState.CurrentUser?.Role?.RoleName is "Staff" or "Admin";
    public bool IsAdmin => AppState.CurrentUser?.Role?.RoleName == "Admin";

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        
        // Default View is the Product Master-Detail view
        MainContent.Content = new Views.ProductMasterDetailView();
    }

    private void BtnProducts_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new Views.ProductMasterDetailView();
    }

    private void BtnRequestService_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Views.ServiceRequestDialog();
        dialog.Owner = this;
        if (dialog.ShowDialog() == true)
        {
            // If submitted successfully, refresh the cart count if the Master-Detail view is active
            if (MainContent.Content is Views.ProductMasterDetailView pdView)
            {
                pdView.UpdateCartCount();
            }
        }
    }

    private void BtnOrderQueue_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new Views.StaffOrderQueueView();
    }

    private void BtnServiceQueue_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new Views.StaffServiceQueueView();
    }

    private void BtnManageUsers_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new Views.ManageUsersView();
    }

    private void BtnReports_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new System.Windows.Controls.TextBlock { Text = "Reports View here...", FontSize = 24, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    }

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new System.Windows.Controls.TextBlock { Text = "Settings View here...", FontSize = 24, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    }
}