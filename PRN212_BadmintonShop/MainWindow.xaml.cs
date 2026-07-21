using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PRN212_BadmintonShop;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public bool IsCustomer => AppState.CurrentUser?.Role?.RoleName == "Customer";
    public bool IsStaffOrAdmin => AppState.CurrentUser?.Role?.RoleName == "Staff" || AppState.CurrentUser?.Role?.RoleName == "Admin";
    public bool IsAdmin => AppState.CurrentUser?.Role?.RoleName == "Admin";

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        AppState.CurrentUser = null;
        LoginWindow loginWindow = new LoginWindow();
        loginWindow.Show();
        this.Close();
    }

    private void LvNavigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainContent != null)
        {
            var item = lvNavigation.SelectedItem as ListViewItem;
            if (item != null)
            {
                MainContent.Content = new TextBlock 
                { 
                    Text = $"Navigate to: {item.Content}", 
                    FontSize = 24, 
                    HorizontalAlignment = HorizontalAlignment.Center, 
                    VerticalAlignment = VerticalAlignment.Center 
                };
            }
        }
    }
}