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

    private void BtnCreateUser_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new UserDialogWindow();
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() == true)
        {
            LoadUsers();
        }
    }

    private void BtnToggleActive_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is User user)
        {
            if (user.Role?.RoleName == "Admin")
            {
                MessageBox.Show("Cannot deactivate an Admin account.");
                return;
            }

            using var context = new BadmintonShopDbContext();
            var dbUser = context.Users.Find(user.UserId);
            if (dbUser != null)
            {
                dbUser.IsActive = !dbUser.IsActive;
                context.SaveChanges();
                LoadUsers();
            }
        }
    }
}
