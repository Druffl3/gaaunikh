using Gaaunikh.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gaaunikh.Api.Data;

public sealed class CommerceDbContext(DbContextOptions<CommerceDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<ProviderCallbackLog> ProviderCallbackLogs => Set<ProviderCallbackLog>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(order => order.Id);
            entity.HasIndex(order => order.Reference).IsUnique();
            entity.Property(order => order.Reference).HasMaxLength(32);
            entity.Property(order => order.Status).HasMaxLength(64);
            entity.Property(order => order.CustomerName).HasMaxLength(200);
            entity.Property(order => order.CustomerEmail).HasMaxLength(320);
            entity.Property(order => order.CustomerPhone).HasMaxLength(32);
            entity.Property(order => order.ShippingAddressLine1).HasMaxLength(200);
            entity.Property(order => order.ShippingAddressLine2).HasMaxLength(200);
            entity.Property(order => order.ShippingCity).HasMaxLength(120);
            entity.Property(order => order.ShippingState).HasMaxLength(120);
            entity.Property(order => order.ShippingPostalCode).HasMaxLength(32);
            entity.Property(order => order.ShippingCountryCode).HasMaxLength(8);
            entity.Property(order => order.SubtotalInr).HasPrecision(18, 2);
            entity.Property(order => order.TotalInr).HasPrecision(18, 2);
            entity.HasMany(order => order.Lines).WithOne(line => line.Order).HasForeignKey(line => line.OrderId);
            entity.HasMany(order => order.PaymentAttempts).WithOne(payment => payment.Order).HasForeignKey(payment => payment.OrderId);
            entity.HasMany(order => order.Shipments).WithOne(shipment => shipment.Order).HasForeignKey(shipment => shipment.OrderId);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.ToTable("order_lines");
            entity.HasKey(line => line.Id);
            entity.Property(line => line.ProductSlug).HasMaxLength(200);
            entity.Property(line => line.ProductName).HasMaxLength(200);
            entity.Property(line => line.WeightLabel).HasMaxLength(64);
            entity.Property(line => line.UnitPriceInr).HasPrecision(18, 2);
            entity.Property(line => line.LineTotalInr).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PaymentAttempt>(entity =>
        {
            entity.ToTable("payment_attempts");
            entity.HasKey(payment => payment.Id);
            entity.Property(payment => payment.Provider).HasMaxLength(64);
            entity.Property(payment => payment.Status).HasMaxLength(64);
            entity.Property(payment => payment.Currency).HasMaxLength(16);
            entity.Property(payment => payment.ProviderOrderId).HasMaxLength(128);
            entity.Property(payment => payment.ProviderPaymentId).HasMaxLength(128);
            entity.Property(payment => payment.AmountInr).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("shipments");
            entity.HasKey(shipment => shipment.Id);
            entity.Property(shipment => shipment.Provider).HasMaxLength(64);
            entity.Property(shipment => shipment.Status).HasMaxLength(64);
            entity.Property(shipment => shipment.ProviderShipmentId).HasMaxLength(128);
            entity.Property(shipment => shipment.Awb).HasMaxLength(128);
            entity.Property(shipment => shipment.CourierName).HasMaxLength(128);
            entity.Property(shipment => shipment.TrackingNumber).HasMaxLength(128);
            entity.Property(shipment => shipment.TrackingUrl).HasMaxLength(512);
        });

        modelBuilder.Entity<NotificationMessage>(entity =>
        {
            entity.ToTable("notification_messages");
            entity.HasKey(notification => notification.Id);
            entity.Property(notification => notification.Channel).HasMaxLength(64);
            entity.Property(notification => notification.EventType).HasMaxLength(128);
            entity.Property(notification => notification.Recipient).HasMaxLength(320);
            entity.Property(notification => notification.Status).HasMaxLength(64);
            entity.Property(notification => notification.ProviderMessageId).HasMaxLength(128);
            entity.Property(notification => notification.LastError).HasMaxLength(2048);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("inventory_items");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.Sku).IsUnique();
            entity.Property(item => item.Sku).HasMaxLength(128);
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.ToTable("inventory_movements");
            entity.HasKey(movement => movement.Id);
            entity.Property(movement => movement.MovementType).HasMaxLength(64);
            entity.Property(movement => movement.Reason).HasMaxLength(128);
            entity.Property(movement => movement.Note).HasMaxLength(1024);
            entity.HasOne(movement => movement.InventoryItem).WithMany(item => item.Movements).HasForeignKey(movement => movement.InventoryItemId);
        });

        modelBuilder.Entity<ProviderCallbackLog>(entity =>
        {
            entity.ToTable("provider_callback_logs");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.Provider).HasMaxLength(64);
            entity.Property(log => log.EventType).HasMaxLength(128);
            entity.Property(log => log.ExternalEventId).HasMaxLength(256);
            entity.Property(log => log.PayloadJson).HasColumnType("text");
        });
    }
}
