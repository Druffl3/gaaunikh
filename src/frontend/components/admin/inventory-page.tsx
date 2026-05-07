"use client";

import { useEffect, useState, type FormEvent } from "react";
import { resolveApiUrl } from "../../lib/api";

export type InventoryMovementEntry = {
  quantityDelta: number;
  movementType: string;
  reason: string;
  note?: string | null;
  createdUtc: string;
};

export type InventorySummaryItem = {
  sku: string;
  productSlug: string;
  productName: string;
  category: string;
  weightLabel: string;
  unitPriceInr: number;
  onHand: number;
  reserved: number;
  available: number;
  reorderThreshold: number;
  isLowStock: boolean;
  recentMovements?: InventoryMovementEntry[];
};

export type CreateInventoryItemInput = {
  sku: string;
  productSlug: string;
  productName: string;
  category: string;
  shortDescription: string;
  description: string;
  weightLabel: string;
  unitPriceInr: number;
  reorderThreshold: number;
};

export type StockAdjustmentInput = {
  sku: string;
  quantityDelta: number;
  reason: string;
  note?: string;
};

type InventorySummaryResponse = {
  items?: InventorySummaryItem[];
};

type InventoryPageProps = {
  loadInventory?: () => Promise<InventorySummaryItem[]>;
  createInventoryItem?: (input: CreateInventoryItemInput) => Promise<InventorySummaryItem>;
  adjustInventory?: (input: StockAdjustmentInput) => Promise<InventorySummaryItem>;
};

type CreateFormState = {
  sku: string;
  productSlug: string;
  productName: string;
  category: string;
  shortDescription: string;
  description: string;
  weightLabel: string;
  unitPriceInr: string;
  reorderThreshold: string;
};

type AdjustmentFormState = {
  sku: string;
  quantityDelta: string;
  reason: string;
  note: string;
};

const adjustmentReasons = ["Restock", "Damage", "ManualCorrection", "ReturnReceived"];

const initialCreateForm: CreateFormState = {
  sku: "",
  productSlug: "",
  productName: "",
  category: "",
  shortDescription: "",
  description: "",
  weightLabel: "",
  unitPriceInr: "",
  reorderThreshold: ""
};

const initialAdjustmentForm: AdjustmentFormState = {
  sku: "",
  quantityDelta: "",
  reason: adjustmentReasons[0],
  note: ""
};

async function fetchInventorySummary(): Promise<InventorySummaryItem[]> {
  const response = await fetch(resolveApiUrl("/api/admin/inventory/summary"), {
    cache: "no-store"
  });

  if (!response.ok) {
    throw new Error("Unable to load inventory.");
  }

  const payload = (await response.json()) as InventorySummaryResponse;
  return payload.items ?? [];
}

async function postCreateInventoryItem(input: CreateInventoryItemInput): Promise<InventorySummaryItem> {
  const response = await fetch(resolveApiUrl("/api/admin/inventory/items"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(input)
  });

  if (!response.ok) {
    throw new Error("Unable to create inventory item.");
  }

  return (await response.json()) as InventorySummaryItem;
}

async function postStockAdjustment(input: StockAdjustmentInput): Promise<InventorySummaryItem> {
  const response = await fetch(resolveApiUrl("/api/admin/inventory/adjustments"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(input)
  });

  if (!response.ok) {
    throw new Error("Unable to adjust inventory.");
  }

  return (await response.json()) as InventorySummaryItem;
}

function replaceInventoryItem(items: InventorySummaryItem[], updatedItem: InventorySummaryItem) {
  const existingIndex = items.findIndex((item) => item.sku === updatedItem.sku);
  if (existingIndex < 0) {
    return [...items, updatedItem].sort((left, right) => left.productName.localeCompare(right.productName));
  }

  return items.map((item) => (item.sku === updatedItem.sku ? updatedItem : item));
}

export function InventoryPage({
  loadInventory = fetchInventorySummary,
  createInventoryItem = postCreateInventoryItem,
  adjustInventory = postStockAdjustment
}: InventoryPageProps) {
  const [items, setItems] = useState<InventorySummaryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [createForm, setCreateForm] = useState<CreateFormState>(initialCreateForm);
  const [adjustmentForm, setAdjustmentForm] = useState<AdjustmentFormState>(initialAdjustmentForm);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const result = await loadInventory();
        if (!cancelled) {
          setItems(result);
          setAdjustmentForm((current) => ({
            ...current,
            sku: current.sku || result[0]?.sku || ""
          }));
          setError(null);
        }
      } catch {
        if (!cancelled) {
          setError("Unable to load inventory right now.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void load();

    return () => {
      cancelled = true;
    };
  }, [loadInventory]);

  function updateCreateField(field: keyof CreateFormState, value: string) {
    setCreateForm((current) => ({
      ...current,
      [field]: value
    }));
  }

  function updateAdjustmentField(field: keyof AdjustmentFormState, value: string) {
    setAdjustmentForm((current) => ({
      ...current,
      [field]: value
    }));
  }

  async function handleCreateSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      const createdItem = await createInventoryItem({
        sku: createForm.sku.trim(),
        productSlug: createForm.productSlug.trim(),
        productName: createForm.productName.trim(),
        category: createForm.category.trim(),
        shortDescription: createForm.shortDescription.trim(),
        description: createForm.description.trim(),
        weightLabel: createForm.weightLabel.trim(),
        unitPriceInr: Number(createForm.unitPriceInr),
        reorderThreshold: Number(createForm.reorderThreshold)
      });

      setItems((current) => replaceInventoryItem(current, createdItem));
      setAdjustmentForm((current) => ({
        ...current,
        sku: current.sku || createdItem.sku
      }));
      setCreateForm(initialCreateForm);
      setError(null);
    } catch {
      setError("Unable to create inventory item right now.");
    }
  }

  async function handleAdjustmentSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      const updatedItem = await adjustInventory({
        sku: adjustmentForm.sku,
        quantityDelta: Number(adjustmentForm.quantityDelta),
        reason: adjustmentForm.reason,
        note: adjustmentForm.note.trim()
      });

      setItems((current) => replaceInventoryItem(current, updatedItem));
      setAdjustmentForm((current) => ({
        ...current,
        quantityDelta: "",
        note: ""
      }));
      setError(null);
    } catch {
      setError("Unable to adjust inventory right now.");
    }
  }

  return (
    <section aria-label="Inventory Admin">
      <div className="checkout-panel">
        <h2>Create Inventory Item</h2>
        <p>Inventory-created variants are the only products exposed to the storefront.</p>
        <form onSubmit={handleCreateSubmit}>
          <label className="field">
            <span>SKU</span>
            <input
              aria-label="SKU"
              value={createForm.sku}
              onChange={(event) => updateCreateField("sku", event.target.value)}
            />
          </label>
          <label className="field">
            <span>Product Slug</span>
            <input
              aria-label="Product Slug"
              value={createForm.productSlug}
              onChange={(event) => updateCreateField("productSlug", event.target.value)}
            />
          </label>
          <label className="field">
            <span>Product Name</span>
            <input
              aria-label="Product Name"
              value={createForm.productName}
              onChange={(event) => updateCreateField("productName", event.target.value)}
            />
          </label>
          <label className="field">
            <span>Category</span>
            <input
              aria-label="Category"
              value={createForm.category}
              onChange={(event) => updateCreateField("category", event.target.value)}
            />
          </label>
          <label className="field">
            <span>Weight Label</span>
            <input
              aria-label="Weight Label"
              value={createForm.weightLabel}
              onChange={(event) => updateCreateField("weightLabel", event.target.value)}
            />
          </label>
          <label className="field">
            <span>Unit Price (INR)</span>
            <input
              aria-label="Unit Price (INR)"
              inputMode="decimal"
              value={createForm.unitPriceInr}
              onChange={(event) => updateCreateField("unitPriceInr", event.target.value)}
            />
          </label>
          <label className="field">
            <span>Reorder Threshold</span>
            <input
              aria-label="Reorder Threshold"
              inputMode="numeric"
              value={createForm.reorderThreshold}
              onChange={(event) => updateCreateField("reorderThreshold", event.target.value)}
            />
          </label>
          <label className="field">
            <span>Short Description</span>
            <input
              aria-label="Short Description"
              value={createForm.shortDescription}
              onChange={(event) => updateCreateField("shortDescription", event.target.value)}
            />
          </label>
          <label className="field">
            <span>Description</span>
            <textarea
              aria-label="Description"
              value={createForm.description}
              onChange={(event) => updateCreateField("description", event.target.value)}
            />
          </label>

          <button type="submit">Create Inventory Item</button>
        </form>
      </div>

      <div className="checkout-panel">
        <h2>Adjust Inventory</h2>
        <form onSubmit={handleAdjustmentSubmit}>
          <label className="field">
            <span>Adjustment SKU</span>
            <select
              aria-label="Adjustment SKU"
              value={adjustmentForm.sku}
              onChange={(event) => updateAdjustmentField("sku", event.target.value)}
            >
              <option value="">Select SKU</option>
              {items.map((item) => (
                <option key={item.sku} value={item.sku}>
                  {item.sku}
                </option>
              ))}
            </select>
          </label>
          <label className="field">
            <span>Quantity Delta</span>
            <input
              aria-label="Quantity Delta"
              inputMode="numeric"
              value={adjustmentForm.quantityDelta}
              onChange={(event) => updateAdjustmentField("quantityDelta", event.target.value)}
            />
          </label>
          <label className="field">
            <span>Reason</span>
            <select
              aria-label="Reason"
              value={adjustmentForm.reason}
              onChange={(event) => updateAdjustmentField("reason", event.target.value)}
            >
              {adjustmentReasons.map((reason) => (
                <option key={reason} value={reason}>
                  {reason}
                </option>
              ))}
            </select>
          </label>
          <label className="field">
            <span>Adjustment Note</span>
            <input
              aria-label="Adjustment Note"
              value={adjustmentForm.note}
              onChange={(event) => updateAdjustmentField("note", event.target.value)}
            />
          </label>

          <button type="submit">Apply Adjustment</button>
        </form>
      </div>

      <div className="checkout-panel">
        <h2>Inventory Ledger</h2>
        {loading ? <p className="catalog-state">Loading inventory...</p> : null}
        {!loading && items.length === 0 ? (
          <p className="catalog-state">No inventory items created yet.</p>
        ) : null}

        {items.map((item) => (
          <article className="product-card" key={item.sku}>
            <p className="product-category">
              {item.category} · {item.weightLabel}
            </p>
            <h3>{item.productName}</h3>
            <p>{item.sku}</p>
            <p>On Hand {item.onHand}</p>
            <p>Reserved {item.reserved}</p>
            <p>Available {item.available}</p>
            <p>Reorder Threshold {item.reorderThreshold}</p>
            <p>Unit Price INR {item.unitPriceInr}</p>
            {item.isLowStock ? <p>Low Stock</p> : <p>Stock Healthy</p>}
            {item.recentMovements && item.recentMovements.length > 0 ? (
              <ul>
                {item.recentMovements.map((movement, index) => (
                  <li key={`${item.sku}-${movement.createdUtc}-${index}`}>
                    {movement.movementType} {movement.quantityDelta} ({movement.reason})
                  </li>
                ))}
              </ul>
            ) : (
              <p>No movement history yet.</p>
            )}
          </article>
        ))}
      </div>

      {error ? <p role="alert">{error}</p> : null}
    </section>
  );
}
