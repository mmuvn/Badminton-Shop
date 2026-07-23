using System;
using System.Linq;
using System.Windows;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public partial class UserDialogWindow : Window
{
    public UserDialogWindow()
    {
        InitializeComponent();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text)) throw new Exception("Username is required.");
            if (string.IsNullOrWhiteSpace(txtEmail.Text)) throw new Exception("Email is required.");
            if (string.IsNullOrWhiteSpace(txtPassword.Text)) throw new Exception("Password is required.");

            using var context = new BadmintonShopDbContext();
            
            if (context.Users.Any(u => u.Username == txtUsername.Text)) throw new Exception("Username already exists.");
            if (context.Users.Any(u => u.Email == txtEmail.Text)) throw new Exception("Email already exists.");

            string roleName = rbCustomer.IsChecked == true ? "Customer" : "Staff";
            var role = context.Roles.FirstOrDefault(r => r.RoleName == roleName);
            if (role == null) throw new Exception("Role not found in database.");

            var user = new User
            {
                Username = txtUsername.Text,
                Email = txtEmail.Text,
                PasswordHash = txtPassword.Text, // Plain text for now as approved
                RoleId = role.RoleId,
                IsActive = true
            };

            context.Users.Add(user);
            context.SaveChanges();

            MessageBox.Show("User created successfully!");
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
