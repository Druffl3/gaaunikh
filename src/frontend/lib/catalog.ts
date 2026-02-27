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

export async function fetchCatalogProducts(filters: CatalogFilters): Promise<CatalogProductListItem[]> {
  const response = await fetch(`/api/catalog/products${createFilterQuery(filters)}`, {
    cache: "no-store"
  });

  if (!response.ok) {
    return [];
  }

  const payload = (await response.json()) as CatalogProductsResponse;
  return payload.products ?? [];
}

export async function fetchCatalogProduct(slug: string): Promise<CatalogProduct | null> {
  const response = await fetch(`/api/catalog/products/${encodeURIComponent(slug)}`, {
    cache: "no-store"
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    return null;
  }

  return (await response.json()) as CatalogProduct;
}
