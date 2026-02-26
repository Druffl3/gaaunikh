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
        <Link className="brand" href="/">
          Gaaunikh
          <span>Masala</span>
        </Link>
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

      <main className="hero-wrap">
        <section className="hero-card">
          <p className="eyebrow">Gaaunikh Masala</p>
          <h1>{heading}</h1>
          <p>{description}</p>
          <button type="button">{actionText}</button>
        </section>
        <aside className="highlight-card">
          <h2>Batch Promise</h2>
          <p>
            Every pack is sourced, roasted, and milled for flavor retention and everyday consistency.
          </p>
          <ul>
            <li>Single-origin spice selection</li>
            <li>Fresh grind cycles every week</li>
            <li>Sealed for aroma protection</li>
          </ul>
        </aside>
      </main>

      {children}
    </div>
  );
}

