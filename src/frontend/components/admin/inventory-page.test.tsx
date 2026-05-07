import { describe, expect, it, jest } from "@jest/globals";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { InventoryPage } from "./inventory-page";
import type { CreateInventoryItemInput, InventorySummaryItem, StockAdjustmentInput } from "./inventory-page";

const inventoryItems: InventorySummaryItem[] = [
  {
    sku: "SPICE-SMOKED-PAPRIKA-200G",
    productSlug: "smoked-paprika",
    productName: "Smoked Paprika",
    category: "Single Spice",
    weightLabel: "200g",
    unitPriceInr: 120,
    onHand: 10,
    reserved: 3,
    available: 7,
    reorderThreshold: 5,
    isLowStock: false
  },
  {
    sku: "BLEND-FIRE-MASALA-100G",
    productSlug: "fire-roast-masala",
    productName: "Fire Roast Masala",
    category: "House Blend",
    weightLabel: "100g",
    unitPriceInr: 140,
    onHand: 2,
    reserved: 1,
    available: 1,
    reorderThreshold: 2,
    isLowStock: true
  }
];

describe("InventoryPage", () => {
  it("renders inventory quantities and low-stock visibility", async () => {
    render(
      <InventoryPage
        loadInventory={jest.fn(async () => inventoryItems)}
        adjustInventory={jest.fn<() => Promise<InventorySummaryItem>>()}
        createInventoryItem={jest.fn<() => Promise<InventorySummaryItem>>()}
      />
    );

    await waitFor(() => {
      expect(screen.getByText("Smoked Paprika")).toBeInTheDocument();
      expect(screen.getByText("Fire Roast Masala")).toBeInTheDocument();
    });

    expect(screen.getByText("On Hand 10")).toBeInTheDocument();
    expect(screen.getByText("Reserved 3")).toBeInTheDocument();
    expect(screen.getByText("Available 7")).toBeInTheDocument();
    expect(screen.getByText("Low Stock")).toBeInTheDocument();
  });

  it("submits an adjustment for the selected sku", async () => {
    const user = userEvent.setup();
    const adjustInventory = jest.fn(async (input: StockAdjustmentInput) => ({
      ...inventoryItems[0],
      onHand: inventoryItems[0].onHand + input.quantityDelta,
      available: inventoryItems[0].available + input.quantityDelta
    }));

    render(
      <InventoryPage
        loadInventory={jest.fn(async () => inventoryItems)}
        adjustInventory={adjustInventory}
        createInventoryItem={jest.fn<() => Promise<InventorySummaryItem>>()}
      />
    );

    await waitFor(() => {
      expect(screen.getByLabelText("Adjustment SKU")).toBeInTheDocument();
    });

    await user.selectOptions(screen.getByLabelText("Adjustment SKU"), "SPICE-SMOKED-PAPRIKA-200G");
    await user.type(screen.getByLabelText("Quantity Delta"), "4");
    await user.selectOptions(screen.getByLabelText("Reason"), "Restock");
    await user.type(screen.getByLabelText("Adjustment Note"), "Cycle count correction");
    await user.click(screen.getByRole("button", { name: "Apply Adjustment" }));

    await waitFor(() => {
      expect(adjustInventory).toHaveBeenCalledWith({
        sku: "SPICE-SMOKED-PAPRIKA-200G",
        quantityDelta: 4,
        reason: "Restock",
        note: "Cycle count correction"
      });
    });
  });

  it("creates a new inventory item for catalog use", async () => {
    const user = userEvent.setup();
    const createInventoryItem = jest.fn(async (input: CreateInventoryItemInput) => ({
      sku: input.sku,
      productSlug: input.productSlug,
      productName: input.productName,
      category: input.category,
      weightLabel: input.weightLabel,
      unitPriceInr: input.unitPriceInr,
      onHand: 0,
      reserved: 0,
      available: 0,
      reorderThreshold: input.reorderThreshold,
      isLowStock: true
    }));

    render(
      <InventoryPage
        loadInventory={jest.fn(async () => inventoryItems)}
        adjustInventory={jest.fn<() => Promise<InventorySummaryItem>>()}
        createInventoryItem={createInventoryItem}
      />
    );

    await waitFor(() => {
      expect(screen.getByLabelText("SKU")).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText("SKU"), "SPICE-KASHMIRI-GARLIC-150G");
    await user.type(screen.getByLabelText("Product Slug"), "kashmiri-garlic-blend");
    await user.type(screen.getByLabelText("Product Name"), "Kashmiri Garlic Blend");
    await user.type(screen.getByLabelText("Category"), "House Blend");
    await user.type(screen.getByLabelText("Weight Label"), "150g");
    await user.type(screen.getByLabelText("Unit Price (INR)"), "165");
    await user.type(screen.getByLabelText("Reorder Threshold"), "4");
    await user.type(screen.getByLabelText("Short Description"), "Garlic-forward finishing masala.");
    await user.type(
      screen.getByLabelText("Description"),
      "A savory garlic and chili blend for fries, gravies, and finishing oils."
    );
    await user.click(screen.getByRole("button", { name: "Create Inventory Item" }));

    await waitFor(() => {
      expect(createInventoryItem).toHaveBeenCalledWith({
        sku: "SPICE-KASHMIRI-GARLIC-150G",
        productSlug: "kashmiri-garlic-blend",
        productName: "Kashmiri Garlic Blend",
        category: "House Blend",
        weightLabel: "150g",
        unitPriceInr: 165,
        reorderThreshold: 4,
        shortDescription: "Garlic-forward finishing masala.",
        description: "A savory garlic and chili blend for fries, gravies, and finishing oils."
      });
    });
  });
});
