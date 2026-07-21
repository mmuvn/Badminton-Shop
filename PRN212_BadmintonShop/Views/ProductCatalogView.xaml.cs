using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public partial class ProductCatalogView : UserControl
{
    public ProductCatalogView()
    {
        InitializeComponent();
        LoadCatalog();
    }

    private void LoadCatalog()
    {
        using var context = new BadmintonShopDbContext();
        // Only show active products with stock > 0
        icProducts.ItemsSource = context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity > 0)
            .ToList();
    }

    private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var product = button?.DataContext as Product;
        if (product != null)
        {
            MessageBox.Show($"Added {product.ProductName} to Cart! (Logic not implemented yet)", "Cart");
        }
    }
}
