import Link from "next/link";
import type { ReactNode } from "react";

type SiteShellProps = {
  heading: string;
  description: string;
  actionText: string;
  children?: ReactNode;
};

export function SiteShell({ heading, description, actionText, children }: SiteShellProps) {
  return (
    <div className="app-shell">
      <header className="topbar">
        <Link className="brand" href="/">Gaaunikh Masala</Link>
        <nav aria-label="Primary">
          <Link className="nav-link" href="/">
            Home
          </Link>
          <Link className="nav-link" href="/shop">
            Shop
          </Link>
          <Link className="nav-link" href="/track-order">
            Track Order
          </Link>
          <Link className="nav-link" href="/contact">
            Contact
          </Link>
        </nav>
      </header>

      <main className="page">
        <section className="intro-card">
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
