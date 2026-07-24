using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PRN212_BadmintonShop.Models;

namespace PRN212_BadmintonShop.Views;

public partial class ProductMasterDetailView : UserControl
{
    public bool IsCustomer => AppState.CurrentUser?.Role?.RoleName == "Customer";
    public bool IsAdmin => AppState.CurrentUser?.Role?.RoleName == "Admin";

    private Product? _selectedProduct = null;

    public ProductMasterDetailView()
    {
        InitializeComponent();
        DataContext = this;
        LoadCategories();
        LoadProducts();
        
        SetFormReadOnly(IsCustomer); // Customers cannot edit
        UpdateCartCount();
    }
    
    public void UpdateCartCount()
    {
        if (!IsCustomer || AppState.CurrentUser == null) return;
        using var context = new BadmintonShopDbContext();
        var cart = context.Carts.Include(c => c.CartItems).Include(c => c.ServiceRequests).FirstOrDefault(c => c.CustomerId == AppState.CurrentUser.UserId);
        int count = 0;
        if (cart != null)
        {
            count = cart.CartItems.Sum(c => c.Quantity) + cart.ServiceRequests.Count(s => !s.IsPaid);
        }
        if (btnViewCart != null) btnViewCart.Content = $"View Cart ({count})";
    }

    private void LoadCategories()
    {
        using var context = new BadmintonShopDbContext();
        cbCategory.ItemsSource = context.Categories.ToList();
    }

    private void LoadProducts(string search = "")
    {
        using var context = new BadmintonShopDbContext();
        var query = context.Products
            .Include(x => x.Category)
            .Include(x => x.RacketDetail).Include(x => x.ShoeDetail).Include(x => x.ShirtDetail)
            .Include(x => x.StringDetail).Include(x => x.GripDetail)
            .Where(x => x.IsActive);

        if (IsCustomer)
        {
            query = query.Where(x => x.StockQuantity > 0);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(x => x.ProductName.ToLower().Contains(search) 
                                  || (x.Brand != null && x.Brand.ToLower().Contains(search))
                                  || (x.Category != null && x.Category.CategoryName.ToLower().Contains(search)));
        }

        dgProducts.ItemsSource = query.ToList();
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadProducts(txtSearch.Text);
    }

    private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
    {
        txtSearch.Text = string.Empty;
    }

    private void DgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgProducts.SelectedItem is Product p)
        {
            _selectedProduct = p;
            PopulateForm(p);
            if (btnAdd != null) btnAdd.Visibility = Visibility.Collapsed;
            if (btnEdit != null) btnEdit.Visibility = Visibility.Visible;
            if (btnDelete != null) btnDelete.Visibility = Visibility.Visible;
            ValidateForm();
        }
    }

    private void SetFormReadOnly(bool isReadOnly)

    {

        cbCategory.IsEnabled = !isReadOnly;
        txtTitle.IsReadOnly = isReadOnly;
        txtBrand.IsReadOnly = isReadOnly;
        txtPrice.IsReadOnly = isReadOnly;
        txtStock.IsReadOnly = isReadOnly;
        txtDescription.IsReadOnly = isReadOnly;

        foreach (UIElement child in pnlDynamicFields.Children)
        {
            if (child is StackPanel sp)
            {
                foreach (UIElement inner in sp.Children)
                {
                    if (inner is TextBox tb) tb.IsReadOnly = isReadOnly;
                }
            }
        }
    }

    private void PopulateForm(Product p)
    {
        cbCategory.SelectedValue = p.CategoryId;
        cbCategory.IsEnabled = false; // Cannot change category after creation
        
        txtTitle.Text = p.ProductName;
        txtBrand.Text = p.Brand;
        txtPrice.Text = p.Price.ToString("0.##");
        txtStock.Text = p.StockQuantity.ToString();
        txtDescription.Text = p.Description;

        GenerateDynamicFields(p.Category?.CategoryName ?? "", p);
    }

    private void CbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cbCategory.SelectedItem is Category cat)
        {
            GenerateDynamicFields(cat.CategoryName, _selectedProduct);
        }
        ValidateForm();
    }

    private void GenerateDynamicFields(string categoryName, Product? p)
    {
        pnlDynamicFields.Children.Clear();
        if (string.IsNullOrEmpty(categoryName)) return;

        if (categoryName == "Racket")
        {
            AddDynamicField("Color", p?.RacketDetail?.Color);
            AddDynamicField("Max Tension (Lbs)", p?.RacketDetail?.MaxTensionLbs?.ToString("0.##"));
            AddDynamicField("Weight (g)", p?.RacketDetail?.FrameWeightGrams?.ToString("0.##"));
        }
        else if (categoryName == "Shoe")
        {
            AddDynamicField("Color", p?.ShoeDetail?.Color);
            AddDynamicField("Size", p?.ShoeDetail?.Size);
        }
        else if (categoryName == "Shirt")
        {
            AddDynamicField("Color", p?.ShirtDetail?.Color);
            AddDynamicField("Size", p?.ShirtDetail?.Size);
            AddDynamicField("Material", p?.ShirtDetail?.Material);
            AddDynamicField("Sleeve Type", p?.ShirtDetail?.SleeveType);
            AddDynamicField("Gender", p?.ShirtDetail?.Gender);
        }
        else if (categoryName == "String")
        {
            AddDynamicField("Color", p?.StringDetail?.Color);
            AddDynamicField("Durability", p?.StringDetail?.Durability.ToString());
            AddDynamicField("Repulsion", p?.StringDetail?.Repulsion.ToString());
            AddDynamicField("Sound", p?.StringDetail?.Sound.ToString());
            AddDynamicField("Control", p?.StringDetail?.Control.ToString());
            AddDynamicField("Shock Absorb", p?.StringDetail?.ShockAbsorption.ToString());
        }
        else if (categoryName == "Grip")
        {
            AddDynamicField("Thickness (mm)", p?.GripDetail?.ThicknessMm?.ToString("0.##"));
            AddDynamicField("Material", p?.GripDetail?.Material);
            AddDynamicField("Color", p?.GripDetail?.Color);
        }
        
        SetFormReadOnly(IsCustomer);
    }

    private void AddDynamicField(string label, string? value)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
        panel.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
        
        var tb = new TextBox { Name = "dyn_" + label.Replace(" ", "").Replace("(", "").Replace(")", ""), Text = value };
        tb.TextChanged += Input_TextChanged;
        panel.Children.Add(tb);
        
        pnlDynamicFields.Children.Add(panel);
    }

    private string GetDynamicFieldValue(string label)
    {
        string name = "dyn_" + label.Replace(" ", "").Replace("(", "").Replace(")", "");
        foreach (UIElement child in pnlDynamicFields.Children)
        {
            if (child is StackPanel sp)
            {
                var tb = sp.Children.OfType<TextBox>().FirstOrDefault();
                if (tb != null && tb.Name == name) return tb.Text;
            }
        }
        return "";
    }

    private void BtnClearForm_Click(object sender, RoutedEventArgs e)
    {
        _selectedProduct = null;
        dgProducts.SelectedItem = null;
        cbCategory.IsEnabled = IsAdmin;
        cbCategory.SelectedItem = null;
        txtTitle.Text = "";
        txtBrand.Text = "";
        txtPrice.Text = "";
        txtStock.Text = "";
        txtDescription.Text = "";
        pnlDynamicFields.Children.Clear();

        if (btnAdd != null) btnAdd.Visibility = Visibility.Visible;
        if (btnEdit != null) btnEdit.Visibility = Visibility.Collapsed;
        if (btnDelete != null) btnDelete.Visibility = Visibility.Collapsed;
        ValidateForm();
    }

    private void Input_TextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateForm();
    }

    private void ValidateForm()
    {
        if (!IsAdmin) return;
        
        bool isValid = cbCategory.SelectedItem != null &&
                       !string.IsNullOrWhiteSpace(txtTitle.Text) &&
                       !string.IsNullOrWhiteSpace(txtBrand.Text) &&
                       !string.IsNullOrWhiteSpace(txtPrice.Text) &&
                       !string.IsNullOrWhiteSpace(txtStock.Text);
                       
        if (btnAdd != null) btnAdd.IsEnabled = isValid;
        if (btnEdit != null) btnEdit.IsEnabled = isValid;
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        SaveProduct();
    }

    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        SaveProduct();
    }

    private void SaveProduct()
    {
        if (!IsAdmin) return;
        
        try
        {
            if (cbCategory.SelectedItem == null) throw new Exception("Please select a category.");
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) throw new Exception("Title is required.");
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0) throw new Exception("Invalid price.");
            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0) throw new Exception("Invalid stock quantity.");

            using var context = new BadmintonShopDbContext();
            using var transaction = context.Database.BeginTransaction();

            Product p;
            if (_selectedProduct != null)
            {
                p = context.Products
                    .Include(x => x.RacketDetail).Include(x => x.ShoeDetail).Include(x => x.ShirtDetail)
                    .Include(x => x.StringDetail).Include(x => x.GripDetail)
                    .First(x => x.ProductId == _selectedProduct.ProductId);
                p.UpdatedAt = DateTime.Now;
            }
            else
            {
                p = new Product { CategoryId = (int)cbCategory.SelectedValue, CreatedAt = DateTime.Now };
                context.Products.Add(p);
            }

            p.ProductName = txtTitle.Text;
            p.Brand = txtBrand.Text;
            p.Description = txtDescription.Text;
            p.Price = price;
            p.StockQuantity = stock;
            p.IsActive = true;

            if (_selectedProduct == null) context.SaveChanges(); 

            var catName = ((Category)cbCategory.SelectedItem).CategoryName;

            if (catName == "Racket")
            {
                if (p.RacketDetail == null) { p.RacketDetail = new RacketDetail { ProductId = p.ProductId }; context.RacketDetails.Add(p.RacketDetail); }
                p.RacketDetail.Color = GetDynamicFieldValue("Color");
                p.RacketDetail.MaxTensionLbs = decimal.TryParse(GetDynamicFieldValue("Max Tension (Lbs)"), out decimal t) ? t : null;
                p.RacketDetail.FrameWeightGrams = decimal.TryParse(GetDynamicFieldValue("Weight (g)"), out decimal w) ? w : null;
            }
            else if (catName == "Shoe")
            {
                if (p.ShoeDetail == null) { p.ShoeDetail = new ShoeDetail { ProductId = p.ProductId }; context.ShoeDetails.Add(p.ShoeDetail); }
                p.ShoeDetail.Color = GetDynamicFieldValue("Color");
                p.ShoeDetail.Size = GetDynamicFieldValue("Size");
            }
            else if (catName == "String")
            {
                if (p.StringDetail == null) { p.StringDetail = new StringDetail { ProductId = p.ProductId }; context.StringDetails.Add(p.StringDetail); }
                p.StringDetail.Color = GetDynamicFieldValue("Color");
                p.StringDetail.Durability = byte.TryParse(GetDynamicFieldValue("Durability"), out byte d) ? d : (byte)0;
                p.StringDetail.Control = byte.TryParse(GetDynamicFieldValue("Control"), out byte c) ? c : (byte)0;
                p.StringDetail.Repulsion = byte.TryParse(GetDynamicFieldValue("Repulsion"), out byte r) ? r : (byte)0;
                p.StringDetail.ShockAbsorption = byte.TryParse(GetDynamicFieldValue("Shock Absorpsion"), out byte sa) ? sa : (byte)0;
                p.StringDetail.Sound = byte.TryParse(GetDynamicFieldValue("Sound"), out byte s) ? s : (byte)0;
            }
            else if (catName == "Shirt")
            {
                if(p.ShirtDetail == null) { p.ShirtDetail = new ShirtDetail { ProductId = p.ProductId }; context.ShirtDetails.Add(p.ShirtDetail); }
                p.ShirtDetail.Color = GetDynamicFieldValue("Color");
                p.ShirtDetail.Size = GetDynamicFieldValue("Size");
                p.ShirtDetail.SleeveType = GetDynamicFieldValue("Sleeve Type");
                p.ShirtDetail.Material = GetDynamicFieldValue("Material");
                p.ShirtDetail.Gender = GetDynamicFieldValue("Gender");
            }
            else if (catName == "Grip")
            {
                if (p.GripDetail == null) { p.GripDetail= new GripDetail { ProductId = p.ProductId }; context.GripDetails.Add(p.GripDetail); }
                p.GripDetail.ThicknessMm = decimal.TryParse(GetDynamicFieldValue("Thickness(mm)"), out decimal th) ? th : null;
                p.GripDetail.Color = GetDynamicFieldValue("Color");
                p.GripDetail.Material = GetDynamicFieldValue("Material");
            }

            context.SaveChanges();
            transaction.Commit();
            MessageBox.Show("Saved successfully.");
            LoadProducts(txtSearch.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error Saving");
        }
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!IsAdmin || _selectedProduct == null) return;
        
        using var context = new BadmintonShopDbContext();
        var p = context.Products.Find(_selectedProduct.ProductId);
        if (p != null)
        {
            p.IsActive = false; 
            context.SaveChanges();
            LoadProducts(txtSearch.Text);
            BtnClearForm_Click(sender, e);
        }
    }

    private void BtnAddToCartList_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedProduct == null) return;
        if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 1 || qty > _selectedProduct.StockQuantity)
        {
            MessageBox.Show($"Invalid quantity. Max stock is {_selectedProduct.StockQuantity}.");
            return;
        }

        try
        {
            using var context = new BadmintonShopDbContext();
            int customerId = AppState.CurrentUser!.UserId;
            var cart = context.Carts.FirstOrDefault(c => c.CustomerId == customerId);
            if (cart == null)
            {
                cart = new Cart { CustomerId = customerId, CreatedAt = DateTime.Now };
                context.Carts.Add(cart);
                context.SaveChanges();
            }

            var existingItem = context.CartItems.FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == _selectedProduct.ProductId);
            if (existingItem != null)
            {
                if (existingItem.Quantity + qty > _selectedProduct.StockQuantity) throw new Exception("Cannot add more than stock quantity to cart.");
                existingItem.Quantity += qty;
            }
            else
            {
                context.CartItems.Add(new CartItem { CartId = cart.CartId, ProductId = _selectedProduct.ProductId, Quantity = qty });
            }
            context.SaveChanges();
            
            MessageBox.Show($"Added {qty} to cart.");
            UpdateCartCount();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error Adding to Cart");
        }
    }

    private void BtnViewCart_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        if (mainWindow != null)
        {
            mainWindow.MainContent.Content = new CheckoutView();
        }
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        AppState.CurrentUser = null;
        var lw = new LoginWindow();
        lw.Show();
        Window.GetWindow(this)?.Close();
    }
}
