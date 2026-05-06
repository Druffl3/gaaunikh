"use client";

import { useState, type FormEvent } from "react";
import type { CartLine } from "../lib/cart";

export type CheckoutSubmission = {
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  shippingAddress: {
    line1: string;
    line2: string;
    city: string;
    state: string;
    postalCode: string;
    countryCode: string;
  };
  lines: Array<{
    productSlug: string;
    weightLabel: string;
    unitPriceInr: number;
    quantity: number;
  }>;
};

export type CheckoutResult = {
  orderId?: string;
  reference: string;
  status: string;
  subtotalInr: number;
  totalInr: number;
};

type CheckoutFormProps = {
  lines: CartLine[];
  submitCheckout?: (request: CheckoutSubmission) => Promise<CheckoutResult>;
};

type FormState = {
  fullName: string;
  email: string;
  phone: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  state: string;
  postalCode: string;
  countryCode: string;
};

const initialFormState: FormState = {
  fullName: "",
  email: "",
  phone: "",
  addressLine1: "",
  addressLine2: "",
  city: "",
  state: "",
  postalCode: "",
  countryCode: ""
};

function formatAmount(amount: number): string {
  return amount.toFixed(2);
}

function hasRequiredFields(form: FormState): boolean {
  return (
    form.fullName.trim().length > 0 &&
    form.email.trim().length > 0 &&
    form.phone.trim().length > 0 &&
    form.addressLine1.trim().length > 0 &&
    form.city.trim().length > 0 &&
    form.state.trim().length > 0 &&
    form.postalCode.trim().length > 0 &&
    form.countryCode.trim().length > 0
  );
}

async function postCheckout(request: CheckoutSubmission): Promise<CheckoutResult> {
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

  return (await response.json()) as CheckoutResult;
}

export function CheckoutForm({ lines, submitCheckout = postCheckout }: CheckoutFormProps) {
  const [form, setForm] = useState<FormState>(initialFormState);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<CheckoutResult | null>(null);

  const subtotalInr = lines.reduce((sum, line) => sum + line.unitPriceInr * line.quantity, 0);
  const canSubmit = lines.length > 0 && hasRequiredFields(form) && !submitting;

  function updateField(field: keyof FormState, value: string) {
    setForm((current) => ({
      ...current,
      [field]: value
    }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!canSubmit) {
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      const checkoutResult = await submitCheckout({
        customerName: form.fullName.trim(),
        customerEmail: form.email.trim(),
        customerPhone: form.phone.trim(),
        shippingAddress: {
          line1: form.addressLine1.trim(),
          line2: form.addressLine2.trim(),
          city: form.city.trim(),
          state: form.state.trim(),
          postalCode: form.postalCode.trim(),
          countryCode: form.countryCode.trim()
        },
        lines: lines.map((line) => ({
          productSlug: line.productSlug,
          weightLabel: line.weightLabel,
          unitPriceInr: line.unitPriceInr,
          quantity: line.quantity
        }))
      });

      setResult(checkoutResult);
    } catch {
      setError("Unable to place your order right now.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="checkout-panel" aria-label="Checkout">
      <div className="checkout-summary">
        <h2>Order Summary</h2>
        {lines.length === 0 ? (
          <p className="catalog-state">Add products to your cart before placing an order.</p>
        ) : (
          <>
            <ul>
              {lines.map((line) => (
                <li key={line.id}>
                  {line.productName} {line.weightLabel} x {line.quantity}
                </li>
              ))}
            </ul>
            <p>Subtotal: INR {formatAmount(subtotalInr)}</p>
          </>
        )}
      </div>

      <form onSubmit={handleSubmit}>
        <label className="field">
          <span>Full Name</span>
          <input
            aria-label="Full Name"
            value={form.fullName}
            onChange={(event) => updateField("fullName", event.target.value)}
          />
        </label>
        <label className="field">
          <span>Email</span>
          <input
            aria-label="Email"
            type="email"
            value={form.email}
            onChange={(event) => updateField("email", event.target.value)}
          />
        </label>
        <label className="field">
          <span>Phone</span>
          <input
            aria-label="Phone"
            value={form.phone}
            onChange={(event) => updateField("phone", event.target.value)}
          />
        </label>
        <label className="field">
          <span>Address Line 1</span>
          <input
            aria-label="Address Line 1"
            value={form.addressLine1}
            onChange={(event) => updateField("addressLine1", event.target.value)}
          />
        </label>
        <label className="field">
          <span>Address Line 2</span>
          <input
            aria-label="Address Line 2"
            value={form.addressLine2}
            onChange={(event) => updateField("addressLine2", event.target.value)}
          />
        </label>
        <label className="field">
          <span>City</span>
          <input
            aria-label="City"
            value={form.city}
            onChange={(event) => updateField("city", event.target.value)}
          />
        </label>
        <label className="field">
          <span>State</span>
          <input
            aria-label="State"
            value={form.state}
            onChange={(event) => updateField("state", event.target.value)}
          />
        </label>
        <label className="field">
          <span>Postal Code</span>
          <input
            aria-label="Postal Code"
            value={form.postalCode}
            onChange={(event) => updateField("postalCode", event.target.value)}
          />
        </label>
        <label className="field">
          <span>Country Code</span>
          <input
            aria-label="Country Code"
            value={form.countryCode}
            onChange={(event) => updateField("countryCode", event.target.value)}
          />
        </label>

        <button type="submit" disabled={!canSubmit}>
          {submitting ? "Placing Order..." : "Place Order"}
        </button>
      </form>

      {error ? <p role="alert">{error}</p> : null}
      {result ? <p>Order reference {result.reference} created successfully.</p> : null}
    </section>
  );
}
