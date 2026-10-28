using AssetManagementBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementBackend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductImage> ProductImages { get; set; } = null!;
    public DbSet<Vendor> Vendors { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Stock> Stocks { get; set; } = null!;
    public DbSet<StockMovement> StockMovements { get; set; } = null!;
    public DbSet<Issuance> Issuances { get; set; } = null!;
    public DbSet<IssuanceItem> IssuanceItems { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Brand> Brands { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(user => user.StaffId)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(user => user.StaffId)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.HasSequence<int>("OrderNumberSequence")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<Order>()
            .HasIndex(order => order.OrderNumber)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .Property(order => order.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .HasOne(order => order.Vendor)
            .WithMany()
            .HasForeignKey(order => order.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(order => order.CreatedByUser)
            .WithMany()
            .HasForeignKey(order => order.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .Property(item => item.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(item => item.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .HasOne(item => item.Order)
            .WithMany(order => order.OrderItems)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Stock>()
            .HasIndex(stock => stock.ProductId)
            .IsUnique();

        modelBuilder.Entity<Stock>()
            .HasOne(stock => stock.Product)
            .WithOne()
            .HasForeignKey<Stock>(stock => stock.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockMovement>()
            .HasOne(movement => movement.Product)
            .WithMany()
            .HasForeignKey(movement => movement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockMovement>()
            .HasOne(movement => movement.Order)
            .WithMany()
            .HasForeignKey(movement => movement.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockMovement>()
            .HasOne(movement => movement.PerformedByUser)
            .WithMany()
            .HasForeignKey(movement => movement.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Issuance>()
            .Property(i => i.RecipientName)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<Issuance>()
            .HasOne(i => i.IssuedByUser)
            .WithMany()
            .HasForeignKey(i => i.IssuedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<IssuanceItem>()
            .HasOne(i => i.Issuance)
            .WithMany(i => i.Items)
            .HasForeignKey(i => i.IssuanceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<IssuanceItem>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<IssuanceItem>()
            .Property(i => i.Barcode)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<IssuanceItem>()
            .Property(i => i.SerialNumber)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<IssuanceItem>()
            .HasIndex(i => i.Barcode)
            .IsUnique();

        modelBuilder.Entity<IssuanceItem>()
            .HasIndex(i => i.SerialNumber)
            .IsUnique();

        modelBuilder.Entity<StockMovement>()
            .HasOne(m => m.Issuance)
            .WithMany()
            .HasForeignKey(m => m.IssuanceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasIndex(category => category.Name)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .Property(category => category.Name)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Brand>()
            .HasIndex(brand => brand.Name)
            .IsUnique();

        modelBuilder.Entity<Brand>()
            .Property(brand => brand.Name)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Product>()
            .HasOne(product => product.Category)
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(product => product.Brand)
            .WithMany()
            .HasForeignKey(product => product.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}