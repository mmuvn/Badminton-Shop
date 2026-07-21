using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRN212_BadmintonShop.Models;

public partial class BadmintonShopDbContext : DbContext
{
    public BadmintonShopDbContext()
    {
    }

    public BadmintonShopDbContext(DbContextOptions<BadmintonShopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<GripDetail> GripDetails { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<RacketDetail> RacketDetails { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }

    public virtual DbSet<ServiceRequestStatus> ServiceRequestStatuses { get; set; }

    public virtual DbSet<ServiceStatusHistory> ServiceStatusHistories { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<ShirtDetail> ShirtDetails { get; set; }

    public virtual DbSet<ShoeDetail> ShoeDetails { get; set; }

    public virtual DbSet<StringDetail> StringDetails { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VwWeeklyRevenue> VwWeeklyRevenues { get; set; }

    public virtual DbSet<VwWeeklyServiceStat> VwWeeklyServiceStats { get; set; }

    public virtual DbSet<VwWeeklyTopProduct> VwWeeklyTopProducts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BadmintonShopDB;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Carts__51BCD7B7DE041D3E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Customer).WithMany(p => p.Carts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Carts_Users");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B0A4DE43E1B");

            entity.HasIndex(e => new { e.CartId, e.ProductId }, "UQ_CartItems").IsUnique();

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK_CartItems_Carts");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartItems_Products");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BEF50EA8C");

            entity.HasIndex(e => e.CategoryName, "UQ__Categori__8517B2E0FD861A7B").IsUnique();

            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<GripDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__GripDeta__B40CC6CD2D353896");

            entity.Property(e => e.ProductId).ValueGeneratedNever();
            entity.Property(e => e.Color).HasMaxLength(30);
            entity.Property(e => e.Material).HasMaxLength(50);
            entity.Property(e => e.ThicknessMm).HasColumnType("decimal(3, 2)");

            entity.HasOne(d => d.Product).WithOne(p => p.GripDetail)
                .HasForeignKey<GripDetail>(d => d.ProductId)
                .HasConstraintName("FK_GripDetails_Products");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF5B71CA73");

            entity.ToTable(tb => tb.HasTrigger("TR_Orders_RequireCancelReason"));

            entity.HasIndex(e => e.CustomerId, "IX_Orders_CustomerId");

            entity.HasIndex(e => e.OrderDate, "IX_Orders_OrderDate");

            entity.Property(e => e.CancelReason).HasMaxLength(255);
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ShippingAddress).HasMaxLength(255);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Users");

            entity.HasOne(d => d.OrderStatus).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_OrderStatuses");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED0681CC6042A6");

            entity.Property(e => e.UnitPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderItems_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Products");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.OrderStatusId).HasName("PK__OrderSta__BC674CA1693C5C61");

            entity.HasIndex(e => e.StatusName, "UQ__OrderSta__05E7698A57035977").IsUnique();

            entity.Property(e => e.StatusName).HasMaxLength(30);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A38EB88EE84");

            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PaymentMethod).HasMaxLength(30);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Payments_Orders");

            entity.HasOne(d => d.ServiceRequest).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ServiceRequestId)
                .HasConstraintName("FK_Payments_ServiceRequests");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CDA0FFC2A9");

            entity.HasIndex(e => e.CategoryId, "IX_Products_CategoryId");

            entity.HasIndex(e => e.ProductName, "IX_Products_Name");

            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(150);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");
        });

        modelBuilder.Entity<RacketDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__RacketDe__B40CC6CD59BB27C4");

            entity.Property(e => e.ProductId).ValueGeneratedNever();
            entity.Property(e => e.Color).HasMaxLength(30);
            entity.Property(e => e.FrameWeightGrams).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MaxTensionLbs).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Product).WithOne(p => p.RacketDetail)
                .HasForeignKey<RacketDetail>(d => d.ProductId)
                .HasConstraintName("FK_RacketDetails_Products");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A104FB86C");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61600CE7E194").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.ServiceRequestId).HasName("PK__ServiceR__790F6C8B8CB7D18A");

            entity.ToTable(tb => tb.HasTrigger("TR_ServiceRequests_RequireCancelReason"));

            entity.HasIndex(e => e.AssignedStaffId, "IX_ServiceRequests_Staff");

            entity.HasIndex(e => e.ServiceStatusId, "IX_ServiceRequests_Status");

            entity.Property(e => e.CancelReason).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.RacketBrand).HasMaxLength(100);
            entity.Property(e => e.RacketModel).HasMaxLength(100);
            entity.Property(e => e.RequestedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.RequestedTension).HasMaxLength(20);

            entity.HasOne(d => d.AssignedStaff).WithMany(p => p.ServiceRequestAssignedStaffs)
                .HasForeignKey(d => d.AssignedStaffId)
                .HasConstraintName("FK_ServiceRequests_Staff");

            entity.HasOne(d => d.Customer).WithMany(p => p.ServiceRequestCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_Customer");

            entity.HasOne(d => d.ServiceStatus).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.ServiceStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_Status");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_Type");

            entity.HasOne(d => d.StringProduct).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.StringProductId)
                .HasConstraintName("FK_ServiceRequests_StringProduct");
        });

        modelBuilder.Entity<ServiceRequestStatus>(entity =>
        {
            entity.HasKey(e => e.ServiceStatusId).HasName("PK__ServiceR__009D5ED9B9C33C9B");

            entity.HasIndex(e => e.StatusName, "UQ__ServiceR__05E7698A32B0ABCA").IsUnique();

            entity.Property(e => e.StatusName).HasMaxLength(20);
        });

        modelBuilder.Entity<ServiceStatusHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__ServiceS__4D7B4ABD513E2BB0");

            entity.ToTable("ServiceStatusHistory");

            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Note).HasMaxLength(255);

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.ServiceStatusHistories)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_History_ChangedBy");

            entity.HasOne(d => d.NewStatus).WithMany(p => p.ServiceStatusHistoryNewStatuses)
                .HasForeignKey(d => d.NewStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_History_NewStatus");

            entity.HasOne(d => d.OldStatus).WithMany(p => p.ServiceStatusHistoryOldStatuses)
                .HasForeignKey(d => d.OldStatusId)
                .HasConstraintName("FK_History_OldStatus");

            entity.HasOne(d => d.ServiceRequest).WithMany(p => p.ServiceStatusHistories)
                .HasForeignKey(d => d.ServiceRequestId)
                .HasConstraintName("FK_History_ServiceRequests");
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.HasKey(e => e.ServiceTypeId).HasName("PK__ServiceT__8ADFAA6C2511591A");

            entity.HasIndex(e => e.TypeName, "UQ__ServiceT__D4E7DFA8A792E510").IsUnique();

            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<ShirtDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__ShirtDet__B40CC6CD68C841E3");

            entity.Property(e => e.ProductId).ValueGeneratedNever();
            entity.Property(e => e.Color).HasMaxLength(30);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.Material).HasMaxLength(50);
            entity.Property(e => e.Size).HasMaxLength(10);
            entity.Property(e => e.SleeveType).HasMaxLength(20);

            entity.HasOne(d => d.Product).WithOne(p => p.ShirtDetail)
                .HasForeignKey<ShirtDetail>(d => d.ProductId)
                .HasConstraintName("FK_ShirtDetails_Products");
        });

        modelBuilder.Entity<ShoeDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__ShoeDeta__B40CC6CD360538BF");

            entity.Property(e => e.ProductId).ValueGeneratedNever();
            entity.Property(e => e.Color).HasMaxLength(30);
            entity.Property(e => e.Size).HasMaxLength(10);

            entity.HasOne(d => d.Product).WithOne(p => p.ShoeDetail)
                .HasForeignKey<ShoeDetail>(d => d.ProductId)
                .HasConstraintName("FK_ShoeDetails_Products");
        });

        modelBuilder.Entity<StringDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__StringDe__B40CC6CD0DEB7314");

            entity.Property(e => e.ProductId).ValueGeneratedNever();
            entity.Property(e => e.Color).HasMaxLength(30);

            entity.HasOne(d => d.Product).WithOne(p => p.StringDetail)
                .HasForeignKey<StringDetail>(d => d.ProductId)
                .HasConstraintName("FK_StringDetails_Products");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C75E4FFDA");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4870235E6").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534DF259467").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<VwWeeklyRevenue>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_WeeklyRevenue");

            entity.Property(e => e.TotalRevenue).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<VwWeeklyServiceStat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_WeeklyServiceStats");

            entity.Property(e => e.StatusName).HasMaxLength(20);
        });

        modelBuilder.Entity<VwWeeklyTopProduct>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_WeeklyTopProducts");

            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.Revenue).HasColumnType("decimal(38, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
