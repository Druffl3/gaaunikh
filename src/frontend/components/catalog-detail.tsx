"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useOptionalCart } from "./cart-provider";
import type { CatalogProduct } from "../lib/catalog";
import { fetchCatalogProduct } from "../lib/catalog";

type CatalogDetailProps = {
  slug: string;
  loadProduct?: (slug: string) => Promise<CatalogProduct | null>;
};

export function CatalogDetail({ slug, loadProduct = fetchCatalogProduct }: CatalogDetailProps) {
  const cart = useOptionalCart();
  const [product, setProduct] = useState<CatalogProduct | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoading(true);
      const result = await loadProduct(slug);

      if (!cancelled) {
        setProduct(result);
        setNotFound(result === null);
        setLoading(false);
      }
    }

    void load();

    return () => {
      cancelled = true;
    };
  }, [slug, loadProduct]);

  if (loading) {
    return <p className="catalog-state">Loading product...</p>;
  }

  if (notFound || product === null) {
    return (
      <section className="catalog-detail">
        <p className="catalog-state">Product not found.</p>
        <Link className="product-link" href="/shop">
          Back to Shop
        </Link>
      </section>
    );
  }

  return (
    <section className="catalog-detail">
      <p className="product-category">{product.category}</p>
      <h2>{product.name}</h2>
      <p>{product.description}</p>
      <div className="variant-list">
        {product.variants.map((variant) => (
          <div className="variant-row" key={variant.weightLabel}>
            <div>
              <span>{variant.weightLabel}</span>
              <strong>INR {variant.priceInr}</strong>
            </div>
            <button
              type="button"
              onClick={() =>
                cart?.addProductVariant({
                  productSlug: product.slug,
                  productName: product.name,
                  weightLabel: variant.weightLabel,
                  unitPriceInr: variant.priceInr
                })
              }
              aria-label={`Add ${product.name} ${variant.weightLabel} to cart`}
            >
              Add to Cart
            </button>
          </div>
        ))}
      </div>
      <Link className="product-link" href="/shop">
        Back to Shop
      </Link>
    </section>
  );
}
