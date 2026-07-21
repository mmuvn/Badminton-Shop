using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public partial class ProductDialogWindow : Window
{
    private readonly int? _productId;

    public ProductDialogWindow(int? productId = null)
    {
        InitializeComponent();
        _productId = productId;
        LoadCategories();

        if (_productId.HasValue)
        {
            LoadProductData(_productId.Value);
            Title = "Edit Product";
        }
        else
        {
            Title = "Add New Product";
        }
    }

    private void LoadCategories()
    {
        using var context = new BadmintonShopDbContext();
        cbCategory.ItemsSource = context.Categories.ToList();
    }

    private void LoadProductData(int id)
    {
        using var context = new BadmintonShopDbContext();
        var p = context.Products
            .Include(x => x.RacketDetail)
            .Include(x => x.ShoeDetail)
            .Include(x => x.ShirtDetail)
            .Include(x => x.StringDetail)
            .Include(x => x.GripDetail)
            .FirstOrDefault(x => x.ProductId == id);

        if (p == null) return;

        cbCategory.SelectedValue = p.CategoryId;
        cbCategory.IsEnabled = false; // Prevent changing category after creation

        txtProductName.Text = p.ProductName;
        txtBrand.Text = p.Brand;
        txtDescription.Text = p.Description;
        txtPrice.Text = p.Price.ToString("0.##");
        txtStock.Text = p.StockQuantity.ToString();
        chkIsActive.IsChecked = p.IsActive;

        if (p.RacketDetail != null)
        {
            txtRacketColor.Text = p.RacketDetail.Color;
            txtRacketTension.Text = p.RacketDetail.MaxTensionLbs?.ToString("0.##");
            txtRacketWeight.Text = p.RacketDetail.FrameWeightGrams?.ToString("0.##");
        }
        else if (p.ShoeDetail != null)
        {
            txtShoeColor.Text = p.ShoeDetail.Color;
            txtShoeSize.Text = p.ShoeDetail.Size;
        }
        else if (p.ShirtDetail != null)
        {
            txtShirtColor.Text = p.ShirtDetail.Color;
            txtShirtSize.Text = p.ShirtDetail.Size;
            txtShirtMaterial.Text = p.ShirtDetail.Material;
            txtShirtSleeve.Text = p.ShirtDetail.SleeveType;
            txtShirtGender.Text = p.ShirtDetail.Gender;
        }
        else if (p.StringDetail != null)
        {
            txtStringColor.Text = p.StringDetail.Color;
            txtStringDurability.Text = p.StringDetail.Durability.ToString();
            txtStringRepulsion.Text = p.StringDetail.Repulsion.ToString();
            txtStringSound.Text = p.StringDetail.Sound.ToString();
            txtStringControl.Text = p.StringDetail.Control.ToString();
            txtStringShock.Text = p.StringDetail.ShockAbsorption.ToString();
        }
        else if (p.GripDetail != null)
        {
            txtGripThickness.Text = p.GripDetail.ThicknessMm?.ToString("0.##");
            txtGripMaterial.Text = p.GripDetail.Material;
            txtGripColor.Text = p.GripDetail.Color;
        }
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
            if (cbCategory.SelectedItem == null) throw new Exception("Please select a category.");
            if (string.IsNullOrWhiteSpace(txtProductName.Text)) throw new Exception("Product name is required.");
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0) throw new Exception("Invalid price.");
            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0) throw new Exception("Invalid stock quantity.");

            using var context = new BadmintonShopDbContext();
            using var transaction = context.Database.BeginTransaction();

            Product product;
            if (_productId.HasValue)
            {
                product = context.Products
                    .Include(x => x.RacketDetail).Include(x => x.ShoeDetail).Include(x => x.ShirtDetail)
                    .Include(x => x.StringDetail).Include(x => x.GripDetail)
                    .First(x => x.ProductId == _productId.Value);
                product.UpdatedAt = DateTime.Now;
            }
            else
            {
                product = new Product
                {
                    CategoryId = (int)cbCategory.SelectedValue,
                    CreatedAt = DateTime.Now
                };
                context.Products.Add(product);
            }

            product.ProductName = txtProductName.Text;
            product.Brand = txtBrand.Text;
            product.Description = txtDescription.Text;
            product.Price = price;
            product.StockQuantity = stock;
            product.IsActive = chkIsActive.IsChecked ?? false;

            // We must save the product first if adding so we get the ProductId for the detail table
            if (!_productId.HasValue)
            {
                context.SaveChanges(); 
            }

            var cat = (Category)cbCategory.SelectedItem;
            string categoryName = cat.CategoryName;

            if (categoryName == "Racket")
            {
                if (product.RacketDetail == null) { product.RacketDetail = new RacketDetail { ProductId = product.ProductId }; context.RacketDetails.Add(product.RacketDetail); }
                product.RacketDetail.Color = txtRacketColor.Text;
                product.RacketDetail.MaxTensionLbs = decimal.TryParse(txtRacketTension.Text, out decimal tension) ? tension : null;
                product.RacketDetail.FrameWeightGrams = decimal.TryParse(txtRacketWeight.Text, out decimal weight) ? weight : null;
            }
            else if (categoryName == "Shoe")
            {
                if (product.ShoeDetail == null) { product.ShoeDetail = new ShoeDetail { ProductId = product.ProductId }; context.ShoeDetails.Add(product.ShoeDetail); }
                product.ShoeDetail.Color = txtShoeColor.Text;
                product.ShoeDetail.Size = txtShoeSize.Text;
            }
            else if (categoryName == "Shirt")
            {
                if (product.ShirtDetail == null) { product.ShirtDetail = new ShirtDetail { ProductId = product.ProductId }; context.ShirtDetails.Add(product.ShirtDetail); }
                product.ShirtDetail.Color = txtShirtColor.Text;
                product.ShirtDetail.Size = txtShirtSize.Text;
                product.ShirtDetail.Material = txtShirtMaterial.Text;
                product.ShirtDetail.SleeveType = txtShirtSleeve.Text;
                product.ShirtDetail.Gender = txtShirtGender.Text;
            }
            else if (categoryName == "String")
            {
                if (product.StringDetail == null) { product.StringDetail = new StringDetail { ProductId = product.ProductId }; context.StringDetails.Add(product.StringDetail); }
                product.StringDetail.Color = txtStringColor.Text;
                product.StringDetail.Durability = byte.TryParse(txtStringDurability.Text, out byte dur) ? dur : (byte)5;
                product.StringDetail.Repulsion = byte.TryParse(txtStringRepulsion.Text, out byte rep) ? rep : (byte)5;
                product.StringDetail.Sound = byte.TryParse(txtStringSound.Text, out byte snd) ? snd : (byte)5;
                product.StringDetail.Control = byte.TryParse(txtStringControl.Text, out byte ctrl) ? ctrl : (byte)5;
                product.StringDetail.ShockAbsorption = byte.TryParse(txtStringShock.Text, out byte shk) ? shk : (byte)5;
            }
            else if (categoryName == "Grip")
            {
                if (product.GripDetail == null) { product.GripDetail = new GripDetail { ProductId = product.ProductId }; context.GripDetails.Add(product.GripDetail); }
                product.GripDetail.ThicknessMm = decimal.TryParse(txtGripThickness.Text, out decimal thickness) ? thickness : null;
                product.GripDetail.Material = txtGripMaterial.Text;
                product.GripDetail.Color = txtGripColor.Text;
            }

            context.SaveChanges();
            transaction.Commit();

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error Saving", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
