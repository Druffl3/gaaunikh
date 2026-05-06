"use client";

import { useState } from "react";
import { CheckoutForm } from "../../components/checkout-form";
import type { CheckoutResult, CheckoutSubmission } from "../../components/checkout-form";
import { PaymentStep } from "../../components/payment-step";
import { routeContent } from "../../components/route-content";
import { SiteShell } from "../../components/site-shell";
import { useCart } from "../../components/cart-provider";

export default function CheckoutPage() {
  const { lines } = useCart();
  const [order, setOrder] = useState<CheckoutResult | null>(null);

  async function submitCheckout(request: CheckoutSubmission): Promise<CheckoutResult> {
    const response = await fetch("/api/orders/checkout", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      throw new Error("Unable to create the order.");
    }

    const result = (await response.json()) as CheckoutResult;
    setOrder(result);
    return result;
  }

  return (
    <SiteShell
      heading={routeContent.checkout.heading}
      description={routeContent.checkout.description}
      actionText={routeContent.checkout.actionText}
    >
      <CheckoutForm lines={lines} submitCheckout={submitCheckout} />
      {order ? <PaymentStep order={order} /> : null}
    </SiteShell>
  );
}
