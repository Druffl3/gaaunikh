import { InventoryPage } from "../../../components/admin/inventory-page";
import { routeContent } from "../../../components/route-content";
import { SiteShell } from "../../../components/site-shell";

export default function AdminInventoryPage() {
  return (
    <SiteShell
      heading={routeContent.adminInventory.heading}
      description={routeContent.adminInventory.description}
      actionText={routeContent.adminInventory.actionText}
    >
      <InventoryPage />
    </SiteShell>
  );
}
