"use client";

import { useSearchParams } from "next/navigation";
import { CatalogDetail } from "./catalog-detail";

export function ShopProductPageClient() {
  const searchParams = useSearchParams();
  const slug = searchParams?.get("slug") ?? "";

  if (slug.length === 0) {
    return (
      <section className="catalog-detail">
        <p className="catalog-state">Select a product from the shop to view details.</p>
      </section>
    );
  }

  return <CatalogDetail slug={slug} />;
}
