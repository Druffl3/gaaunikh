import { describe, expect, it } from "@jest/globals";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { CartPanel } from "./cart-panel";
import { CartProvider, useCart } from "./cart-provider";
import { createCartLineId } from "../lib/cart";

function CartSeedAction() {
  const { addProductVariant } = useCart();

  return (
    <button
      type="button"
      onClick={() =>
        addProductVariant({
          productSlug: "kashmiri-chili-powder",
          productName: "Kashmiri Chili Powder",
          weightLabel: "100g",
          unitPriceInr: 95
        })
      }
    >
      Seed Cart
    </button>
  );
}

describe("CartPanel", () => {
  it("renders empty state when there are no cart lines", () => {
    render(
      <CartProvider storageKey="test-cart-empty">
        <CartPanel />
      </CartProvider>
    );

    expect(screen.getByText("Your cart is empty. Add products from the catalog.")).toBeInTheDocument();
  });

  it("updates totals when quantity is changed and line removed", async () => {
    const user = userEvent.setup();

    render(
      <CartProvider storageKey="test-cart-flow">
        <CartSeedAction />
        <CartPanel />
      </CartProvider>
    );

    await user.click(screen.getByRole("button", { name: "Seed Cart" }));

    await waitFor(() => {
      expect(screen.getByText("Kashmiri Chili Powder")).toBeInTheDocument();
      expect(screen.getByText("INR 95")).toBeInTheDocument();
      expect(screen.getByText("Subtotal: INR 95.00")).toBeInTheDocument();
    });

    await user.click(
      screen.getByRole("button", { name: "Increase quantity for Kashmiri Chili Powder 100g" })
    );

    await waitFor(() => {
      expect(screen.getByText("Subtotal: INR 190.00")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: "Remove Kashmiri Chili Powder 100g" }));

    await waitFor(() => {
      expect(screen.getByText("Your cart is empty. Add products from the catalog.")).toBeInTheDocument();
    });
  });

  it("hydrates cart lines from session storage", async () => {
    const storageKey = "test-cart-hydrate";
    window.sessionStorage.setItem(
      storageKey,
      JSON.stringify([
        {
          id: createCartLineId("haldi-gold-turmeric", "250g"),
          productSlug: "haldi-gold-turmeric",
          productName: "Haldi Gold Turmeric",
          weightLabel: "250g",
          unitPriceInr: 175,
          quantity: 2
        }
      ])
    );

    render(
      <CartProvider storageKey={storageKey}>
        <CartPanel />
      </CartProvider>
    );

    await waitFor(() => {
      expect(screen.getByText("Haldi Gold Turmeric")).toBeInTheDocument();
      expect(screen.getByText("Subtotal: INR 350.00")).toBeInTheDocument();
    });
  });

  it("falls back to empty cart when stored payload is invalid", async () => {
    const storageKey = "test-cart-invalid";
    window.sessionStorage.setItem(storageKey, "{not-json");

    render(
      <CartProvider storageKey={storageKey}>
        <CartPanel />
      </CartProvider>
    );

    await waitFor(() => {
      expect(screen.getByText("Your cart is empty. Add products from the catalog.")).toBeInTheDocument();
    });
  });

  it("writes updates back to session storage", async () => {
    const user = userEvent.setup();
    const storageKey = "test-cart-persist-write";

    render(
      <CartProvider storageKey={storageKey}>
        <CartSeedAction />
        <CartPanel />
      </CartProvider>
    );

    await user.click(screen.getByRole("button", { name: "Seed Cart" }));

    await waitFor(() => {
      const raw = window.sessionStorage.getItem(storageKey);
      expect(raw).not.toBeNull();
      const parsed = JSON.parse(raw ?? "[]") as Array<{ quantity: number }>;
      expect(parsed[0]?.quantity).toBe(1);
    });
  });
});
