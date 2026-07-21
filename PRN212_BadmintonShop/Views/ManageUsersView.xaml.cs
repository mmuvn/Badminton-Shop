using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public partial class ManageUsersView : UserControl
{
    public ManageUsersView()
    {
        InitializeComponent();
        LoadUsers();
    }

    private void LoadUsers()
    {
        using var context = new BadmintonShopDbContext();
        dgUsers.ItemsSource = context.Users.Include(u => u.Role).ToList();
    }

    private void BtnToggleActive_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var user = button?.DataContext as User;
        if (user != null)
        {
            using var context = new BadmintonShopDbContext();
            var dbUser = context.Users.Find(user.UserId);
            if (dbUser != null)
            {
                // Prevent admin from deactivating themselves
                if (dbUser.UserId == AppState.CurrentUser?.UserId)
                {
                    MessageBox.Show("You cannot deactivate your own account.", "Action Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dbUser.IsActive = !dbUser.IsActive;
                context.SaveChanges();
                LoadUsers();
            }
        }
    }

    private void BtnChangeRole_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var user = button?.DataContext as User;
        if (user != null)
        {
            MessageBox.Show($"Change Role for {user.Username} clicked. We'll implement a dialog for this later.", "Not Implemented");
        }
    }
}
