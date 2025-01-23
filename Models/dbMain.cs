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

    public virtual DbSet<TblAdminInfo> TblAdminInfos { get; set; }

    public virtual DbSet<TblCompany> TblCompanies { get; set; }

    public virtual DbSet<TblPermission> TblPermissions { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblShop> TblShops { get; set; }

    public virtual DbSet<TblShopCategory> TblShopCategories { get; set; }

    public virtual DbSet<TblTab> TblTabs { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=VIKASH\\MSSQLSERVERV2;Database=dbWMS;User ID=sa;Password=vikash;TrustServerCertificate=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            entity.Property(e => e.VerificationStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VerifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblCompany>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__Companie__2D971C4CD6D1F5DD");

            entity.ToTable("tblCompanies");

            entity.HasIndex(e => e.CompanyName, "UQ__Companie__9BCE05DC3980D1EE").IsUnique();

            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
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
            entity.Property(e => e.Location)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ReputationScore).HasDefaultValue(100);
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
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ShopImagePath).IsUnicode(false);
            entity.Property(e => e.ShopName)
                .HasMaxLength(20)
                .IsUnicode(false);
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

        modelBuilder.Entity<TblTab>(entity =>
        {
            entity.HasKey(e => e.TabId).HasName("PK__tblTabs__80E37C18457ADDD7");

            entity.ToTable("tblTabs");

            entity.Property(e => e.TabName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TabUrl)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ParentId)
                .IsUnicode(false);
            entity.Property(e => e.IconPath)
                .IsUnicode(false);

        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC6F310873");

            entity.ToTable("tblUsers");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4C3F7304A").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105348B239E19").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.AdminRef)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DesignationIdRef).HasColumnName("DesignationID_Ref");
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
            entity.Property(e => e.RoleId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
