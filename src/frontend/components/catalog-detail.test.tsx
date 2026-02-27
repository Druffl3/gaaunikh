import { describe, expect, it, jest } from "@jest/globals";
import { render, screen, waitFor } from "@testing-library/react";
import { CatalogDetail } from "./catalog-detail";
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
});
