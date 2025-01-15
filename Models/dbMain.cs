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

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }

    public virtual DbSet<ShopCategory> ShopCategories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=VIKASH\\MSSQLSERVERV2;Database=dbWMS;User ID=sa;Password=vikash;TrustServerCertificate=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__Companie__2D971C4CD6D1F5DD");

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

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("PK__Shop__67C55629F6A6F3FB");

            entity.ToTable("Shop");

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

        modelBuilder.Entity<ShopCategory>(entity =>
        {
            entity.HasKey(e => e.ShopCatId).HasName("PK__ShopCate__9673CEEFCBB8B0C9");

            entity.Property(e => e.ShopCatId).HasColumnName("ShopCatID");
            entity.Property(e => e.ShopCategory1)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ShopCategory");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACE00A1D10");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4A4BC76A1").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105342B90EC57").IsUnique();

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
            entity.Property(e => e.OtpExpiry).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ProfileImgPath).IsUnicode(false);
            entity.Property(e => e.Role)
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
