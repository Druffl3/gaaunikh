"use client";

import { useCart } from "./cart-provider";

function formatTwoDecimalAmount(amount: number): string {
  return amount.toFixed(2);
}

export function CartPanel() {
  const { lines, summary, increaseQuantity, decreaseQuantity, removeLine } = useCart();

  if (lines.length === 0) {
    return <p className="catalog-state">Your cart is empty. Add products from the catalog.</p>;
  }

  return (
    <section className="cart-panel" aria-label="Cart">
      <div className="cart-lines">
        {lines.map((line) => (
          <article className="cart-line" key={line.id}>
            <div>
              <h2>{line.productName}</h2>
              <p className="cart-meta">{line.weightLabel}</p>
              <p className="cart-meta">INR {line.unitPriceInr}</p>
            </div>

            <div className="cart-actions">
              <div className="cart-quantity-controls">
                <button
                  type="button"
                  aria-label={`Decrease quantity for ${line.productName} ${line.weightLabel}`}
                  onClick={() => decreaseQuantity(line.id)}
                >
                  -
                </button>
                <span>{line.quantity}</span>
                <button
                  type="button"
                  aria-label={`Increase quantity for ${line.productName} ${line.weightLabel}`}
                  onClick={() => increaseQuantity(line.id)}
                >
                  +
                </button>
              </div>

              <button
                type="button"
                aria-label={`Remove ${line.productName} ${line.weightLabel}`}
                onClick={() => removeLine(line.id)}
              >
                Remove
              </button>
            </div>
          </article>
        ))}
      </div>

      <section className="cart-summary" aria-label="Pricing Summary">
        <h2>Pricing Summary</h2>
        <p>Unique items: {summary.uniqueItems}</p>
        <p>Total quantity: {summary.totalQuantity}</p>
        <p>Subtotal: INR {formatTwoDecimalAmount(summary.subtotalInr)}</p>
      </section>
    </section>
  );
}
