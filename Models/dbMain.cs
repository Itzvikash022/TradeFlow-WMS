using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WMS_Application.Models;

public partial class dbMain : DbContext
{
    public dbMain()
    {
    }

    public dbMain(DbContextOptions<dbMain> options)
        : base(options)
    {
    }

    public virtual DbSet<TblActivityLog> TblActivityLogs { get; set; }

    public virtual DbSet<TblAdminInfo> TblAdminInfos { get; set; }

    public virtual DbSet<TblCompany> TblCompanies { get; set; }

    public virtual DbSet<TblCustomer> TblCustomers { get; set; }

    public virtual DbSet<TblOrder> TblOrders { get; set; }

    public virtual DbSet<TblOrderDetail> TblOrderDetails { get; set; }

    public virtual DbSet<TblPermission> TblPermissions { get; set; }

    public virtual DbSet<TblProduct> TblProducts { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblShop> TblShops { get; set; }

    public virtual DbSet<TblShopCategory> TblShopCategories { get; set; }

    public virtual DbSet<TblStock> TblStocks { get; set; }

    public virtual DbSet<TblTab> TblTabs { get; set; }

    public virtual DbSet<TblTransaction> TblTransactions { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=VIKASH\\MSSQLSERVERV2;Database=dbWMS;User ID=sa;Password=vikash;TrustServerCertificate=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__tblActiv__5E5499A8A7383945");

            entity.ToTable("tblActivityLog");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.ActionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ActionType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EntityId).HasColumnName("EntityID");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<TblAdminInfo>(entity =>
        {
            entity.HasKey(e => e.InfoId).HasName("PK__AdminInf__4DEC9D9AB00D761C");

            entity.ToTable("tblAdminInfo");

            entity.Property(e => e.InfoId).HasColumnName("InfoID");
            entity.Property(e => e.AddressProofPath).IsUnicode(false);
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IdentityDocNo)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.IdentityDocPath).IsUnicode(false);
            entity.Property(e => e.IdentityDocType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ShopLicenseNo)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ShopLicensePath).IsUnicode(false);
        });

        modelBuilder.Entity<TblCompany>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__Companie__2D971C4CD6D1F5DD");

            entity.ToTable("tblCompanies");

            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
            entity.Property(e => e.AddedBy).HasDefaultValue(100);
            entity.Property(e => e.Address).IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CompanyLogo).IsUnicode(false);
            entity.Property(e => e.CompanyName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Gst)
                .IsUnicode(false)
                .HasColumnName("GST");
            entity.Property(e => e.LastOrderDate).HasColumnType("datetime");
            entity.Property(e => e.State)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblCustomer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__tblCusto__A4AE64B843E78226");

            entity.ToTable("tblCustomers");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Contact)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.CustomerName)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__tblOrder__C3905BCFF87972BB");

            entity.ToTable("tblOrders");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.OrderType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalQty);
        });

        modelBuilder.Entity<TblOrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__tblOrder__D3B9D30C0BAE6D27");

            entity.ToTable("tblOrderDetails");

            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PricePerUnit).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Order).WithMany(p => p.TblOrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__tblOrderD__Order__093F5D4E");

            entity.HasOne(d => d.Product).WithMany(p => p.TblOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__tblOrderD__Produ__0A338187");
        });

        modelBuilder.Entity<TblPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__tblPermi__EFA6FB2F766BECD4");

            entity.ToTable("tblPermissions");

            entity.HasOne(d => d.Tab).WithMany(p => p.TblPermissions)
                .HasForeignKey(d => d.TabId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblPermis__TabId__59904A2C");
        });

        modelBuilder.Entity<TblProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__tblProdu__B40CC6EDD77ECDDD");

            entity.ToTable("tblProducts");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Category)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastUpdateDate).HasColumnType("datetime");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.PricePerUnit).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductQty);
            entity.Property(e => e.ProductImagePath).IsUnicode(false);
            entity.Property(e => e.ProductName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__tblRoles__8AFACE1ACBEB9A77");

            entity.ToTable("tblRoles");

            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RoleName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblShop>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("PK__Shop__67C55629F6A6F3FB");

            entity.ToTable("tblShop");

            entity.Property(e => e.ShopId).HasColumnName("ShopID");
            entity.Property(e => e.Address).IsUnicode(false);
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.City)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ClosingTime).HasPrecision(0);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ShopImagePath).IsUnicode(false);
            entity.Property(e => e.ShopName)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StartTime).HasPrecision(0);
            entity.Property(e => e.State)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblShopCategory>(entity =>
        {
            entity.HasKey(e => e.ShopCatId).HasName("PK__ShopCate__9673CEEFCBB8B0C9");

            entity.ToTable("tblShopCategories");

            entity.Property(e => e.ShopCatId).HasColumnName("ShopCatID");
            entity.Property(e => e.ShopCategory)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblStock>(entity =>
        {
            entity.HasKey(e => e.StockId).HasName("PK__tblStock__2C83A9E2A20396A9");

            entity.ToTable("tblStock");

            entity.Property(e => e.StockId).HasColumnName("StockID");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ShopPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.ShopId).HasColumnName("ShopID");

            entity.HasOne(d => d.Product).WithMany(p => p.TblStocks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__tblStock__Produc__7EC1CEDB");

            entity.HasOne(d => d.Shop).WithMany(p => p.TblStocks)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__tblStock__ShopID__7FB5F314");
        });

        modelBuilder.Entity<TblTab>(entity =>
        {
            entity.HasKey(e => e.TabId).HasName("PK__tblTabs__80E37C18457ADDD7");

            entity.ToTable("tblTabs");

            entity.Property(e => e.IconPath).IsUnicode(false);
            entity.Property(e => e.TabName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TabUrl)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__tblTrans__55433A4BB862A767");

            entity.ToTable("tblTransactions");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Remarks)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ReceiptPath)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__tblUsers__1788CCAC46059246");

            entity.ToTable("tblUsers");

            entity.HasIndex(e => e.Username, "UQ__tblUsers__536C85E49B6EBFE9").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__tblUsers__A9D1053455EE6088").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.AdminRef)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Otp)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.OtpExpiry).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ProfileImgPath).IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VerificationStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VerifiedBy)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
