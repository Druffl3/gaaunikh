import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "@jest/globals";
import { waitFor } from "@testing-library/dom";
import type { ReactElement } from "react";
import HomePage from "./page";
import ShopPage from "./shop/page";
import ShopProductPage from "./shop/product/page";
import CartPage from "./cart/page";
import { CartProvider } from "../components/cart-provider";

function renderWithProviders(content: ReactElement) {
  return render(<CartProvider storageKey="test-routes-cart">{content}</CartProvider>);
}

describe("Next routes", () => {
  it("renders home and shop experiences", async () => {
    const homePage = renderWithProviders(<HomePage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent(
      "Pure Spice Craft From Gaaunikh Kitchens"
    );
    expect(screen.getByRole("link", { name: "Cart" })).toBeInTheDocument();

    homePage.unmount();
    renderWithProviders(<ShopPage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Catalog Collection");
    expect(screen.getByText("Search Catalog")).toBeInTheDocument();
    await waitFor(() => {
      expect(screen.getByText("No products found for this filter.")).toBeInTheDocument();
    });
  });

  it("renders product detail route shell", () => {
    renderWithProviders(<ShopProductPage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Product Details");
    expect(screen.getByText("Select a product from the shop to view details.")).toBeInTheDocument();
  });

  it("renders cart route shell", () => {
    renderWithProviders(<CartPage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Your Cart");
    expect(screen.getByText("Your cart is empty. Add products from the catalog.")).toBeInTheDocument();
  });
});
