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

  it("shows action and updated footer signature", () => {
    render(
      <SiteShell
        heading="Catalog Collection"
        description="Browse single-origin spices and house blends."
        actionText="Browse Collection"
      />
    );

    expect(screen.getByRole("button", { name: "Browse Collection" })).toBeInTheDocument();
    expect(screen.getByText("Crafted in small batches for kitchens that demand precision.")).toBeInTheDocument();
  });
});
