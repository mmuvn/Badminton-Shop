using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public partial class ManageProductsView : UserControl
{
    public ManageProductsView()
    {
        InitializeComponent();
        LoadProducts();
    }

    private void LoadProducts()
    {
        using var context = new BadmintonShopDbContext();
        dgProducts.ItemsSource = context.Products.Include(p => p.Category).ToList();
    }

    private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProductDialogWindow();
        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            LoadProducts();
        }
    }

    private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var product = button?.DataContext as Product;
        if (product != null)
        {
            var dialog = new ProductDialogWindow(product.ProductId);
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                LoadProducts();
            }
        }
    }

    private void BtnToggleActive_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var product = button?.DataContext as Product;
        if (product != null)
        {
            using var context = new BadmintonShopDbContext();
            var dbProduct = context.Products.Find(product.ProductId);
            if (dbProduct != null)
            {
                dbProduct.IsActive = !dbProduct.IsActive;
                context.SaveChanges();
                LoadProducts();
            }
        }
    }
}
