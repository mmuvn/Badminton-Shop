using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            txtError.Text = "Please enter both username and password.";
            return;
        }

        using var context = new BadmintonShopDbContext();
        var user = context.Users.Include(u => u.Role).FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

        if (user != null)
        {
            if (!user.IsActive)
            {
                txtError.Text = "Your account is deactivated. Please contact an administrator.";
                return;
            }

            AppState.CurrentUser = user;
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        else
        {
            txtError.Text = "Invalid username or password.";
        }
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        RegisterWindow registerWindow = new RegisterWindow();
        registerWindow.Show();
        this.Close();
    }
}
