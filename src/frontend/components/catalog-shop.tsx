"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import type { CatalogFilters, CatalogProductListItem } from "../lib/catalog";
import { fetchCatalogProducts } from "../lib/catalog";

type CatalogShopProps = {
  loadProducts?: (filters: CatalogFilters) => Promise<CatalogProductListItem[]>;
};

const categories = ["All", "Single Spice", "House Blend"] as const;

export function CatalogShop({ loadProducts = fetchCatalogProducts }: CatalogShopProps) {
  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("All");
  const [products, setProducts] = useState<CatalogProductListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function loadCatalog() {
      setLoading(true);
      const result = await loadProducts({ search, category });
      if (!cancelled) {
        setProducts(result);
        setLoading(false);
      }
    }

    void loadCatalog();

    return () => {
      cancelled = true;
    };
  }, [search, category, loadProducts]);

  return (
    <section className="catalog-section" aria-label="Catalog">
      <div className="catalog-controls">
        <label className="field">
          <span>Search Catalog</span>
          <input
            aria-label="Search Catalog"
            type="search"
            placeholder="Search spices"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </label>

        <label className="field">
          <span>Category</span>
          <select
            aria-label="Category"
            value={category}
            onChange={(event) => setCategory(event.target.value)}
          >
            {categories.map((item) => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </select>
        </label>
      </div>

      {loading ? (
        <p className="catalog-state">Loading catalog...</p>
      ) : products.length === 0 ? (
        <p className="catalog-state">No products found for this filter.</p>
      ) : (
        <div className="catalog-grid">
          {products.map((product) => (
            <article className="product-card" key={product.slug}>
              <p className="product-category">{product.category}</p>
              <h2>{product.name}</h2>
              <p>{product.shortDescription}</p>
              <p className="product-price">
                INR {product.lowestPriceInr} - INR {product.highestPriceInr}
              </p>
              <Link className="product-link" href={`/shop/product/?slug=${encodeURIComponent(product.slug)}`}>
                View Details
              </Link>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
