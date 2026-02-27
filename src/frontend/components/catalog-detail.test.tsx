import { describe, expect, it, jest } from "@jest/globals";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { CatalogDetail } from "./catalog-detail";
import { CartProvider, useCart } from "./cart-provider";
import type { CatalogProduct } from "../lib/catalog";

describe("CatalogDetail", () => {
  it("renders product details and variants", async () => {
    const loadProduct = jest.fn(async (): Promise<CatalogProduct | null> => ({
      slug: "kashmiri-chili-powder",
      name: "Kashmiri Chili Powder",
      category: "Single Spice",
      shortDescription: "Bright color and balanced warmth.",
      description: "Crafted for color-first flavor.",
      variants: [
        { weightLabel: "100g", priceInr: 95 },
        { weightLabel: "250g", priceInr: 210 }
      ]
    }));

    render(<CatalogDetail slug="kashmiri-chili-powder" loadProduct={loadProduct} />);

    await waitFor(() => {
      expect(screen.getByRole("heading", { name: "Kashmiri Chili Powder" })).toBeInTheDocument();
      expect(screen.getByText("100g")).toBeInTheDocument();
      expect(screen.getByText("250g")).toBeInTheDocument();
    });
  });

  it("shows not found state", async () => {
    const loadProduct = jest.fn(async (): Promise<CatalogProduct | null> => null);

    render(<CatalogDetail slug="missing" loadProduct={loadProduct} />);

    await waitFor(() => {
      expect(screen.getByText("Product not found.")).toBeInTheDocument();
    });
  });

  it("adds selected variant to cart", async () => {
    const user = userEvent.setup();
    const loadProduct = jest.fn(async (): Promise<CatalogProduct | null> => ({
      slug: "kashmiri-chili-powder",
      name: "Kashmiri Chili Powder",
      category: "Single Spice",
      shortDescription: "Bright color and balanced warmth.",
      description: "Crafted for color-first flavor.",
      variants: [
        { weightLabel: "100g", priceInr: 95 },
        { weightLabel: "250g", priceInr: 210 }
      ]
    }));

    function CartProbe() {
      const { summary } = useCart();
      return <p>Cart quantity: {summary.totalQuantity}</p>;
    }

    render(
      <CartProvider storageKey="test-product-detail-cart">
        <CatalogDetail slug="kashmiri-chili-powder" loadProduct={loadProduct} />
        <CartProbe />
      </CartProvider>
    );

    await waitFor(() => {
      expect(
        screen.getByRole("button", { name: "Add Kashmiri Chili Powder 100g to cart" })
      ).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: "Add Kashmiri Chili Powder 100g to cart" }));

    await waitFor(() => {
      expect(screen.getByText("Cart quantity: 1")).toBeInTheDocument();
    });
  });
});
