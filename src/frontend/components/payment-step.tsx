"use client";

import { useState } from "react";
import type { CheckoutResult } from "./checkout-form";
import { resolveApiUrl } from "../lib/api";

export type CreatePaymentResult = {
  provider: string;
  amountInr: number;
  currency: string;
  razorpayOrderId: string;
  razorpayKeyId: string;
};

type PaymentStepProps = {
  order: CheckoutResult;
  createPayment?: (orderId: string) => Promise<CreatePaymentResult>;
};

async function postCreatePayment(orderId: string): Promise<CreatePaymentResult> {
  const response = await fetch(resolveApiUrl("/api/payments/create-payment"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ orderId })
  });

  if (!response.ok) {
    throw new Error("Unable to initialize payment.");
  }

  return (await response.json()) as CreatePaymentResult;
}

function formatAmount(amount: number): string {
  return amount.toFixed(2);
}

export function PaymentStep({ order, createPayment = postCreatePayment }: PaymentStepProps) {
  const [handoff, setHandoff] = useState<CreatePaymentResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (!order.orderId) {
    return null;
  }

  async function handleCreatePayment() {
    setSubmitting(true);
    setError(null);

    try {
      const result = await createPayment(order.orderId!);
      setHandoff(result);
    } catch {
      setError("Unable to prepare Razorpay payment right now.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="checkout-panel" aria-label="Payment Step">
      <h2>Payment</h2>
      <p>Order reference {order.reference} is ready for payment.</p>
      <p>Total due: INR {formatAmount(order.totalInr)}</p>
      <p>Backend verification remains the source of truth for payment status.</p>

      <button type="button" onClick={handleCreatePayment} disabled={submitting}>
        {submitting ? "Preparing Razorpay..." : "Continue to Payment"}
      </button>

      {handoff ? (
        <div>
          <p>Razorpay order {handoff.razorpayOrderId} created.</p>
          <p>Using checkout key {handoff.razorpayKeyId}.</p>
          <p>Awaiting verified backend confirmation before marking this order paid.</p>
        </div>
      ) : null}

      {error ? <p role="alert">{error}</p> : null}
    </section>
  );
}
