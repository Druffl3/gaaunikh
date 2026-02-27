import { CatalogShop } from "../../components/catalog-shop";
import { SiteShell } from "../../components/site-shell";
import { routeContent } from "../../components/route-content";

export default function ShopPage() {
  return (
    <SiteShell
      heading={routeContent.shop.heading}
      description={routeContent.shop.description}
      actionText={routeContent.shop.actionText}
    >
      <CatalogShop />
    </SiteShell>
  );
}

