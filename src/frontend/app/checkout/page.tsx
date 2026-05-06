"use client";

import { CheckoutForm } from "../../components/checkout-form";
import { routeContent } from "../../components/route-content";
import { SiteShell } from "../../components/site-shell";
import { useCart } from "../../components/cart-provider";

export default function CheckoutPage() {
  const { lines } = useCart();

  return (
    <SiteShell
      heading={routeContent.checkout.heading}
      description={routeContent.checkout.description}
      actionText={routeContent.checkout.actionText}
    >
      <CheckoutForm lines={lines} />
    </SiteShell>
  );
}
