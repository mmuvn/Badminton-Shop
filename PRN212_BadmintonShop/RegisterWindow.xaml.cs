using System;
using System.Linq;
using System.Windows;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        string fullName = txtFullName.Text.Trim();
        string username = txtUsername.Text.Trim();
        string email = txtEmail.Text.Trim();
        string password = txtPassword.Password;
        string confirmPassword = txtConfirmPassword.Password;
        string phone = txtPhone.Text.Trim();
        string address = txtAddress.Text.Trim();

        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || 
            string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            txtError.Text = "Please fill in all required fields (Name, Username, Email, Password).";
            return;
        }

        if (password != confirmPassword)
        {
            txtError.Text = "Passwords do not match.";
            return;
        }

        using var context = new BadmintonShopDbContext();
        
        // Check if username or email already exists
        if (context.Users.Any(u => u.Username == username))
        {
            txtError.Text = "Username is already taken.";
            return;
        }
        if (context.Users.Any(u => u.Email == email))
        {
            txtError.Text = "Email is already registered.";
            return;
        }

        var customerRole = context.Roles.FirstOrDefault(r => r.RoleName == "Customer");
        if (customerRole == null)
        {
            txtError.Text = "System error: Customer role not found.";
            return;
        }

        var newUser = new User
        {
            FullName = fullName,
            Username = username,
            Email = email,
            PasswordHash = password, // Note: In a real app, hash this!
            Phone = phone,
            Address = address,
            RoleId = customerRole.RoleId,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        context.Users.Add(newUser);
        context.SaveChanges();

        MessageBox.Show("Registration successful! Please login.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        
        LoginWindow loginWindow = new LoginWindow();
        loginWindow.Show();
        this.Close();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        LoginWindow loginWindow = new LoginWindow();
        loginWindow.Show();
        this.Close();
    }
}
