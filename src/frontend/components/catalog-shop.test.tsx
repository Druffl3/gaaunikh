import { describe, expect, it, jest } from "@jest/globals";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { CatalogShop } from "./catalog-shop";
import type { CatalogProductListItem } from "../lib/catalog";

const products: CatalogProductListItem[] = [
  {
    slug: "kashmiri-chili-powder",
    name: "Kashmiri Chili Powder",
    category: "Single Spice",
    shortDescription: "Bright color and balanced warmth.",
    lowestPriceInr: 95,
    highestPriceInr: 395
  },
  {
    slug: "haldi-gold-turmeric",
    name: "Haldi Gold Turmeric",
    category: "Single Spice",
    shortDescription: "Fresh turmeric brightness.",
    lowestPriceInr: 80,
    highestPriceInr: 330
  },
  {
    slug: "signature-garam-masala",
    name: "Signature Garam Masala",
    category: "House Blend",
    shortDescription: "Warm whole-spice blend.",
    lowestPriceInr: 120,
    highestPriceInr: 510
  }
];

function filterProducts(search: string, category: string): CatalogProductListItem[] {
  return products.filter((product) => {
    const matchesSearch =
      search.trim().length === 0 ||
      product.name.toLowerCase().includes(search.trim().toLowerCase());
    const matchesCategory =
      category.trim().length === 0 ||
      category === "All" ||
      product.category.toLowerCase() === category.toLowerCase();

    return matchesSearch && matchesCategory;
  });
}

describe("CatalogShop", () => {
  it("renders products and filters by search", async () => {
    const loadProducts = jest.fn(async ({ search, category }) => filterProducts(search, category));
    const user = userEvent.setup();

    render(<CatalogShop loadProducts={loadProducts} />);

    await waitFor(() => {
      expect(screen.getByText("Kashmiri Chili Powder")).toBeInTheDocument();
      expect(screen.getByText("Haldi Gold Turmeric")).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText("Search Catalog"), "turmeric");

    await waitFor(() => {
      expect(screen.getByText("Haldi Gold Turmeric")).toBeInTheDocument();
      expect(screen.queryByText("Kashmiri Chili Powder")).not.toBeInTheDocument();
    });
  });

  it("filters by category", async () => {
    const loadProducts = jest.fn(async ({ search, category }) => filterProducts(search, category));
    const user = userEvent.setup();

    render(<CatalogShop loadProducts={loadProducts} />);

    await waitFor(() => {
      expect(screen.getByText("Signature Garam Masala")).toBeInTheDocument();
    });

    await user.selectOptions(screen.getByLabelText("Category"), "House Blend");

    await waitFor(() => {
      expect(screen.getByText("Signature Garam Masala")).toBeInTheDocument();
      expect(screen.queryByText("Haldi Gold Turmeric")).not.toBeInTheDocument();
    });
  });
});
