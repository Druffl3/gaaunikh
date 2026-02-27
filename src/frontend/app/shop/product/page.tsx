import { Suspense } from "react";
import { routeContent } from "../../../components/route-content";
import { ShopProductPageClient } from "../../../components/shop-product-page-client";
import { SiteShell } from "../../../components/site-shell";

export default function ShopProductPage() {
  return (
    <SiteShell
      heading={routeContent.productDetail.heading}
      description={routeContent.productDetail.description}
      actionText={routeContent.productDetail.actionText}
    >
      <Suspense
        fallback={
          <section className="catalog-detail">
            <p className="catalog-state">Loading product...</p>
          </section>
        }
      >
        <ShopProductPageClient />
      </Suspense>
    </SiteShell>
  );
}
