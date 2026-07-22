using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public partial class ServiceRequestDialog : Window
{
    private Product? _selectedString = null;

    public ServiceRequestDialog()
    {
        InitializeComponent();
        LoadStrings();
    }

    private void LoadStrings()
    {
        using var context = new BadmintonShopDbContext();
        var strings = context.Products
            .Include(p => p.StringDetail)
            .Where(p => p.Category.CategoryName == "String" && p.IsActive && p.StockQuantity > 0)
            .ToList();
        
        cbStringProduct.ItemsSource = strings;
        if (strings.Any()) cbStringProduct.SelectedIndex = 0;
    }

    private void RbService_Checked(object sender, RoutedEventArgs e)
    {
        if (pnlNewString == null || pnlWeldFrame == null) return;

        if (rbNewString.IsChecked == true)
        {
            pnlNewString.Visibility = Visibility.Visible;
            pnlWeldFrame.Visibility = Visibility.Collapsed;
        }
        else
        {
            pnlNewString.Visibility = Visibility.Collapsed;
            pnlWeldFrame.Visibility = Visibility.Visible;
        }
    }

    private void CbStringProduct_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (cbStringProduct.SelectedItem is Product p)
        {
            _selectedString = p;
            txtStringColor.Text = p.StringDetail?.Color ?? "N/A";
            txtStringPrice.Text = p.Price.ToString("C");
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void BtnSubmit_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var context = new BadmintonShopDbContext();
            
            // Get or create cart
            int customerId = AppState.CurrentUser!.UserId;
            var cart = context.Carts.FirstOrDefault(c => c.CustomerId == customerId);
            if (cart == null)
            {
                cart = new Cart { CustomerId = customerId, CreatedAt = DateTime.Now };
                context.Carts.Add(cart);
                context.SaveChanges();
            }

            var serviceRequest = new ServiceRequest
            {
                CustomerId = customerId,
                CartId = cart.CartId,
                IsPaid = false,
                RequestedDate = DateTime.Now
            };

            // Hardcoded lookups per user approval
            var todoStatus = context.ServiceRequestStatuses.FirstOrDefault(s => s.StatusName == "Todo");
            if (todoStatus != null) serviceRequest.ServiceStatusId = todoStatus.ServiceStatusId;

            if (rbNewString.IsChecked == true)
            {
                if (_selectedString == null) throw new Exception("Please select a string.");
                if (!decimal.TryParse(txtTension.Text, out decimal tension) || tension <= 5) throw new Exception("Tension must be greater than 5.");

                var serviceType = context.ServiceTypes.FirstOrDefault(s => s.TypeName == "New String");
                if (serviceType != null) serviceRequest.ServiceTypeId = serviceType.ServiceTypeId;

                serviceRequest.StringProductId = _selectedString.ProductId;
                serviceRequest.RequestedTension = tension;
                serviceRequest.Price = _selectedString.Price;
                serviceRequest.Description = $"Stringing with {_selectedString.ProductName}";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtCondition.Text)) throw new Exception("Please describe the frame condition.");

                var serviceType = context.ServiceTypes.FirstOrDefault(s => s.TypeName == "Weld Frame");
                if (serviceType != null) serviceRequest.ServiceTypeId = serviceType.ServiceTypeId;

                serviceRequest.Description = "Weld Frame: " + txtCondition.Text;
                serviceRequest.Price = 100000;
            }

            context.ServiceRequests.Add(serviceRequest);
            context.SaveChanges();

            MessageBox.Show("Service request added to cart!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
