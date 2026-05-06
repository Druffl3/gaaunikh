using System;
using Gaaunikh.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Gaaunikh.Api.Data.Migrations;

[DbContext(typeof(CommerceDbContext))]
public partial class CommerceDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.4")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("uuid");
            entity.Property(e => e.OnHandQuantity).HasColumnType("integer");
            entity.Property(e => e.ReorderThreshold).HasColumnType("integer");
            entity.Property(e => e.ReservedQuantity).HasColumnType("integer");
            entity.Property(e => e.Sku).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property(e => e.UpdatedUtc).HasColumnType("timestamp with time zone");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.ToTable("inventory_items");
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("uuid");
            entity.Property(e => e.CreatedUtc).HasColumnType("timestamp with time zone");
            entity.Property(e => e.InventoryItemId).HasColumnType("uuid");
            entity.Property(e => e.MovementType).HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property(e => e.Note).HasMaxLength(1024).HasColumnType("character varying(1024)");
            entity.Property(e => e.QuantityDelta).HasColumnType("integer");
            entity.Property(e => e.Reason).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InventoryItemId);
            entity.ToTable("inventory_movements");
        });

        modelBuilder.Entity<NotificationMessage>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("uuid");
            entity.Property(e => e.AttemptCount).HasColumnType("integer");
            entity.Property(e => e.Channel).HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property(e => e.CreatedUtc).HasColumnType("timestamp with time zone");
            entity.Property(e => e.EventType).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property(e => e.LastError).HasMaxLength(2048).HasColumnType("character varying(2048)");
            entity.Property(e => e.OrderId).HasColumnType("uuid");
            entity.Property(e => e.ProviderMessageId).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property(e => e.Recipient).HasMaxLength(320).HasColumnType("character varying(320)");
            entity.Property(e => e.Status).HasMaxLength(64).HasColumnType("character varying(64)");
            entity.HasKey(e => e.Id);
            entity.ToTable("notification_messages");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("uuid");
            entity.Property(e => e.CreatedUtc).HasColumnType("timestamp with time zone");
            entity.Property(e => e.CustomerEmail).HasMaxLength(320).HasColumnType("character varying(320)");
            entity.Property(e => e.CustomerName).HasMaxLength(200).HasColumnType("character varying(200)");
            entity.Property(e => e.CustomerPhone).HasMaxLength(32).HasColumnType("character varying(32)");
            entity.Property(e => e.Reference).HasMaxLength(32).HasColumnType("character varying(32)");
            entity.Property(e => e.ShippingAddressLine1).HasMaxLength(200).HasColumnType("character varying(200)");
            entity.Property(e => e.ShippingAddressLine2).HasMaxLength(200).HasColumnType("character varying(200)");
            entity.Property(e => e.ShippingCity).HasMaxLength(120).HasColumnType("character varying(120)");
            entity.Property(e => e.ShippingCountryCode).HasMaxLength(8).HasColumnType("character varying(8)");
            entity.Property(e => e.ShippingPostalCode).HasMaxLength(32).HasColumnType("character varying(32)");
            entity.Property(e => e.ShippingState).HasMaxLength(120).HasColumnType("character varying(120)");
            entity.Property(e => e.Status).HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property(e => e.SubtotalInr).HasPrecision(18, 2).HasColumnType("numeric(18,2)");
            entity.Property(e => e.TotalInr).HasPrecision(18, 2).HasColumnType("numeric(18,2)");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Reference).IsUnique();
            entity.ToTable("orders");
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("uuid");
            entity.Property(e => e.LineTotalInr).HasPrecision(18, 2).HasColumnType("numeric(18,2)");
            entity.Property(e => e.OrderId).HasColumnType("uuid");
            entity.Property(e => e.ProductName).HasMaxLength(200).HasColumnType("character varying(200)");
            entity.Property(e => e.ProductSlug).HasMaxLength(200).HasColumnType("character varying(200)");
            entity.Property(e => e.Quantity).HasColumnType("integer");
            entity.Property(e => e.UnitPriceInr).HasPrecision(18, 2).HasColumnType("numeric(18,2)");
            entity.Property(e => e.WeightLabel).HasMaxLength(64).HasColumnType("character varying(64)");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderId);
            entity.ToTable("order_lines");
        });

        modelBuilder.Entity<PaymentAttempt>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("uuid");
            entity.Property(e => e.AmountInr).HasPrecision(18, 2).HasColumnType("numeric(18,2)");
            entity.Property(e => e.CreatedUtc).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Currency).HasMaxLength(16).HasColumnType("character varying(16)");
            entity.Property(e => e.OrderId).HasColumnType("uuid");
            entity.Property(e => e.Provider).HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property(e => e.ProviderOrderId).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property(e => e.ProviderPaymentId).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property(e => e.Status).HasMaxLength(64).HasColumnType("character varying(64)");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderId);
            entity.ToTable("payment_attempts");
        });

        modelBuilder.Entity<ProviderCallbackLog>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("uuid");
            entity.Property(e => e.EventType).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property(e => e.ExternalEventId).HasMaxLength(256).HasColumnType("character varying(256)");
            entity.Property(e => e.PayloadJson).HasColumnType("text");
            entity.Property(e => e.Processed).HasColumnType("boolean");
            entity.Property(e => e.Provider).HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property(e => e.ReceivedUtc).HasColumnType("timestamp with time zone");
            entity.HasKey(e => e.Id);
            entity.ToTable("provider_callback_logs");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("uuid");
            entity.Property(e => e.Awb).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property(e => e.CourierName).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property(e => e.CreatedUtc).HasColumnType("timestamp with time zone");
            entity.Property(e => e.OrderId).HasColumnType("uuid");
            entity.Property(e => e.Provider).HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property(e => e.ProviderShipmentId).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property(e => e.Status).HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property(e => e.TrackingNumber).HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property(e => e.TrackingUrl).HasMaxLength(512).HasColumnType("character varying(512)");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderId);
            entity.ToTable("shipments");
        });

        modelBuilder.Entity<InventoryMovement>()
            .HasOne(d => d.InventoryItem)
            .WithMany(p => p.Movements)
            .HasForeignKey(d => d.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderLine>()
            .HasOne(d => d.Order)
            .WithMany(p => p.Lines)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PaymentAttempt>()
            .HasOne(d => d.Order)
            .WithMany(p => p.PaymentAttempts)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Shipment>()
            .HasOne(d => d.Order)
            .WithMany(p => p.Shipments)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
