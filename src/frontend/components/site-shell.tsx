"use client";

import Link from "next/link";
import type { ReactNode } from "react";
import { useOptionalCart } from "./cart-provider";

type SiteShellProps = {
  heading: string;
  description: string;
  actionText: string;
  children?: ReactNode;
  showHeroImage?: boolean;
};

export function SiteShell({ heading, description, actionText, children, showHeroImage = false }: SiteShellProps) {
  const cart = useOptionalCart();
  const cartQuantity = cart?.summary.totalQuantity ?? 0;

  return (
    <div className="app-shell">
      <header className="topbar">
        <Link aria-label="Gaaunikh Masala" className="brand" href="/">
          <img className="brand-logo" src="/brand/logo_w.png" alt="Gaaunikh Groups logo" />
          <span>Gaaunikh Masala</span>
        </Link>
        <nav aria-label="Primary">
          <Link className="nav-link" href="/">
            Home
          </Link>
          <Link className="nav-link" href="/shop/">
            Shop
          </Link>
          <Link className="nav-link nav-link-cart" href="/cart/">
            <span>Cart</span>
            {cartQuantity > 0 ? (
              <span className="nav-badge" aria-label={`${cartQuantity} item${cartQuantity === 1 ? "" : "s"} in cart`}>
                {cartQuantity}
              </span>
            ) : null}
          </Link>
          <Link className="nav-link" href="/track-order/">
            Track Order
          </Link>
          <Link className="nav-link" href="/contact/">
            Contact
          </Link>
        </nav>
      </header>

      <main className="page">
        <section className={`intro-card${showHeroImage ? " intro-card-hero" : ""}`}>
          <p className="eyebrow">Gaaunikh Masala</p>
          <h1>{heading}</h1>
          <p>{description}</p>
          <button type="button">{actionText}</button>
        </section>
        {children ? (
          <section className="content-panel">{children}</section>
        ) : (
          <section className="content-panel">
            <h2>Batch Promise</h2>
            <p>Single-origin sourcing. Small-batch grinding. Precision-sealed packing.</p>
          </section>
        )}
      </main>

      <footer>
        <p>Crafted in small batches for kitchens that demand precision.</p>
      </footer>
    </div>
  );
}
