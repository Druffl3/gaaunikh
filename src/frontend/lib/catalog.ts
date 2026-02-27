export type CatalogVariant = {
  weightLabel: string;
  priceInr: number;
};

export type CatalogProduct = {
  slug: string;
  name: string;
  category: string;
  shortDescription: string;
  description: string;
  variants: CatalogVariant[];
};

export type CatalogProductListItem = {
  slug: string;
  name: string;
  category: string;
  shortDescription: string;
  lowestPriceInr: number;
  highestPriceInr: number;
};

type CatalogProductsResponse = {
  products: CatalogProductListItem[];
};

export type CatalogFilters = {
  search: string;
  category: string;
};

const dummyCatalogProducts: CatalogProduct[] = [
  {
    slug: "kashmiri-chili-powder",
    name: "Kashmiri Chili Powder",
    category: "Single Spice",
    shortDescription: "Bright color, balanced warmth, and layered aroma.",
    description:
      "Crafted from low-heat Kashmiri chilies to deliver color-first flavor for curries, tandoori marinades, and finishing oils.",
    variants: [
      { weightLabel: "100g", priceInr: 95 },
      { weightLabel: "250g", priceInr: 210 },
      { weightLabel: "500g", priceInr: 395 }
    ]
  },
  {
    slug: "haldi-gold-turmeric",
    name: "Haldi Gold Turmeric",
    category: "Single Spice",
    shortDescription: "Fresh turmeric brightness for daily cooking.",
    description:
      "Stone-milled turmeric root with deep golden tone and warm earthy finish for dals, sabzis, and wellness recipes.",
    variants: [
      { weightLabel: "100g", priceInr: 80 },
      { weightLabel: "250g", priceInr: 175 },
      { weightLabel: "500g", priceInr: 330 }
    ]
  },
  {
    slug: "roasted-coriander-powder",
    name: "Roasted Coriander Powder",
    category: "Single Spice",
    shortDescription: "Citrus-lifted coriander with roasted depth.",
    description:
      "Slow-roasted coriander seeds milled in small batches for bright, nutty character in gravies, chutneys, and dry rubs.",
    variants: [
      { weightLabel: "100g", priceInr: 72 },
      { weightLabel: "250g", priceInr: 158 },
      { weightLabel: "500g", priceInr: 295 }
    ]
  },
  {
    slug: "signature-garam-masala",
    name: "Signature Garam Masala",
    category: "House Blend",
    shortDescription: "Warm whole-spice blend for finishing dishes.",
    description:
      "A balanced house blend of cardamom, clove, cinnamon, and pepper designed to lift aroma in North and South Indian dishes.",
    variants: [
      { weightLabel: "100g", priceInr: 120 },
      { weightLabel: "250g", priceInr: 265 },
      { weightLabel: "500g", priceInr: 510 }
    ]
  },
  {
    slug: "coastal-kitchen-blend",
    name: "Coastal Kitchen Blend",
    category: "House Blend",
    shortDescription: "Peppery-red blend for seafood and vegetable fries.",
    description:
      "A robust red masala with black pepper, chili, garlic, and curry leaf notes suitable for coastal gravies and pan-seared vegetables.",
    variants: [
      { weightLabel: "100g", priceInr: 130 },
      { weightLabel: "250g", priceInr: 288 },
      { weightLabel: "500g", priceInr: 548 }
    ]
  }
];

function createFilterQuery(filters: CatalogFilters): string {
  const params = new URLSearchParams();

  if (filters.search.trim().length > 0) {
    params.set("search", filters.search.trim());
  }

  if (filters.category.trim().length > 0 && filters.category !== "All") {
    params.set("category", filters.category.trim());
  }

  const query = params.toString();
  return query.length > 0 ? `?${query}` : "";
}

function toListItems(products: CatalogProduct[]): CatalogProductListItem[] {
  return products.map((product) => ({
    slug: product.slug,
    name: product.name,
    category: product.category,
    shortDescription: product.shortDescription,
    lowestPriceInr: Math.min(...product.variants.map((variant) => variant.priceInr)),
    highestPriceInr: Math.max(...product.variants.map((variant) => variant.priceInr))
  }));
}

function filterDummyProducts(filters: CatalogFilters): CatalogProduct[] {
  const search = filters.search.trim().toLowerCase();
  const category = filters.category.trim().toLowerCase();

  return dummyCatalogProducts.filter((product) => {
    const matchesSearch =
      search.length === 0 || product.name.toLowerCase().includes(search);
    const matchesCategory =
      category.length === 0 || category === "all" || product.category.toLowerCase() === category;

    return matchesSearch && matchesCategory;
  });
}

export async function fetchCatalogProducts(filters: CatalogFilters): Promise<CatalogProductListItem[]> {
  try {
    const response = await fetch(`/api/catalog/products${createFilterQuery(filters)}`, {
      cache: "no-store"
    });

    if (!response.ok) {
      return toListItems(filterDummyProducts(filters));
    }

    const payload = (await response.json()) as CatalogProductsResponse;
    return payload.products ?? [];
  } catch {
    return toListItems(filterDummyProducts(filters));
  }
}

export async function fetchCatalogProduct(slug: string): Promise<CatalogProduct | null> {
  try {
    const response = await fetch(`/api/catalog/products/${encodeURIComponent(slug)}`, {
      cache: "no-store"
    });

    if (!response.ok) {
      return (
        dummyCatalogProducts.find((product) => product.slug.toLowerCase() === slug.toLowerCase()) ??
        null
      );
    }

    return (await response.json()) as CatalogProduct;
  } catch {
    return dummyCatalogProducts.find((product) => product.slug.toLowerCase() === slug.toLowerCase()) ?? null;
  }
}
