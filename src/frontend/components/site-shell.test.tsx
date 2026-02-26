import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "@jest/globals";
import { SiteShell } from "./site-shell";

describe("SiteShell", () => {
  it("renders primary navigation links", () => {
    render(
      <SiteShell
        heading="Pure Spice Craft From Gaaunikh Kitchens"
        description="Freshly ground masalas..."
        actionText="Discover the Brand"
      />
    );

    expect(screen.getByRole("link", { name: "Home" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Shop" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Track Order" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Contact" })).toBeInTheDocument();
  });

  it("keeps the footer tagline", () => {
    render(
      <SiteShell
        heading="Shop Launching Next"
        description="Catalog is preparing for release."
        actionText="Get Notified"
      />
    );

    expect(
      screen.getByText("Flavor built for homes, restaurants, and wholesale partners.")
    ).toBeInTheDocument();
  });
});
