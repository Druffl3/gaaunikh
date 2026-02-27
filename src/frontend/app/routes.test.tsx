import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "@jest/globals";
import { waitFor } from "@testing-library/dom";
import HomePage from "./page";
import ShopPage from "./shop/page";
import ShopProductPage from "./shop/product/page";

describe("Next routes", () => {
  it("renders home and shop experiences", async () => {
    const homePage = render(<HomePage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent(
      "Pure Spice Craft From Gaaunikh Kitchens"
    );

    homePage.unmount();
    render(<ShopPage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Catalog Collection");
    expect(screen.getByText("Search Catalog")).toBeInTheDocument();
    await waitFor(() => {
      expect(screen.getByText("No products found for this filter.")).toBeInTheDocument();
    });
  });

  it("renders product detail route shell", () => {
    render(<ShopProductPage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Product Details");
    expect(screen.getByText("Select a product from the shop to view details.")).toBeInTheDocument();
  });
});
