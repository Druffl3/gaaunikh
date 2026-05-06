using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gaaunikh.Api.Data.Migrations;

public partial class InitialCommerceFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "inventory_items",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Sku = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                OnHandQuantity = table.Column<int>(type: "integer", nullable: false),
                ReservedQuantity = table.Column<int>(type: "integer", nullable: false),
                ReorderThreshold = table.Column<int>(type: "integer", nullable: false),
                UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_inventory_items", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "notification_messages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                Channel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Recipient = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ProviderMessageId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                LastError = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                AttemptCount = table.Column<int>(type: "integer", nullable: false),
                CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_notification_messages", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "orders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Reference = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                CustomerEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                CustomerPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                ShippingAddressLine1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ShippingAddressLine2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                ShippingCity = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                ShippingState = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                ShippingPostalCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                ShippingCountryCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                SubtotalInr = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                TotalInr = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_orders", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "provider_callback_logs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ExternalEventId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                PayloadJson = table.Column<string>(type: "text", nullable: false),
                Processed = table.Column<bool>(type: "boolean", nullable: false),
                ReceivedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_callback_logs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "inventory_movements",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                QuantityDelta = table.Column<int>(type: "integer", nullable: false),
                MovementType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Reason = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Note = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_inventory_movements", x => x.Id);
                table.ForeignKey(
                    name: "FK_inventory_movements_inventory_items_InventoryItemId",
                    column: x => x.InventoryItemId,
                    principalTable: "inventory_items",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "order_lines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductSlug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                WeightLabel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                UnitPriceInr = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                LineTotalInr = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_order_lines", x => x.Id);
                table.ForeignKey(
                    name: "FK_order_lines_orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "payment_attempts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                Provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                AmountInr = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                ProviderOrderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                ProviderPaymentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_payment_attempts", x => x.Id);
                table.ForeignKey(
                    name: "FK_payment_attempts_orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "shipments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                Provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ProviderShipmentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                Awb = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                CourierName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                TrackingNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                TrackingUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_shipments", x => x.Id);
                table.ForeignKey(
                    name: "FK_shipments_orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_inventory_items_Sku",
            table: "inventory_items",
            column: "Sku",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_inventory_movements_InventoryItemId",
            table: "inventory_movements",
            column: "InventoryItemId");

        migrationBuilder.CreateIndex(
            name: "IX_order_lines_OrderId",
            table: "order_lines",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_orders_Reference",
            table: "orders",
            column: "Reference",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_payment_attempts_OrderId",
            table: "payment_attempts",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_shipments_OrderId",
            table: "shipments",
            column: "OrderId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "inventory_movements");
        migrationBuilder.DropTable(name: "notification_messages");
        migrationBuilder.DropTable(name: "order_lines");
        migrationBuilder.DropTable(name: "payment_attempts");
        migrationBuilder.DropTable(name: "provider_callback_logs");
        migrationBuilder.DropTable(name: "shipments");
        migrationBuilder.DropTable(name: "inventory_items");
        migrationBuilder.DropTable(name: "orders");
    }
}
