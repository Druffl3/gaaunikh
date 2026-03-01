import { CartPanel } from "../../components/cart-panel";
import { routeContent } from "../../components/route-content";
import { SiteShell } from "../../components/site-shell";

export default function CartPage() {
  return (
    <SiteShell
      heading={routeContent.cart.heading}
      description={routeContent.cart.description}
      actionText={routeContent.cart.actionText}
    >
      <CartPanel />
    </SiteShell>
  );
}
