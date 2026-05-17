import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "@jest/globals";
import { SiteShell } from "./site-shell";
import { CartProvider, useCart } from "./cart-provider";

function CartSeedAction() {
  const { addProductVariant } = useCart();

  return (
    <button
      type="button"
      onClick={() =>
        addProductVariant({
          productSlug: "kashmiri-chili-powder",
          productName: "Kashmiri Chili Powder",
          weightLabel: "100g",
          unitPriceInr: 95
        })
      }
    >
      Seed Cart
    </button>
  );
}

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
    expect(screen.getByRole("link", { name: "Cart" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Track Order" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Contact" })).toBeInTheDocument();
  });

  it("renders the brand logo and home hero treatment when enabled", () => {
    render(
      <SiteShell
        heading="Pure Spice Craft From Gaaunikh Kitchens"
        description="Freshly ground masalas..."
        actionText="Discover the Brand"
        showHeroImage
      />
    );

    expect(screen.getByAltText("Gaaunikh Groups logo")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Gaaunikh Masala" })).toBeInTheDocument();
    expect(screen.getByRole("heading", { level: 1 }).closest("section")).toHaveClass("intro-card-hero");
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

  it("shows a cart quantity badge when items are in the cart", async () => {
    const user = userEvent.setup();

    render(
      <CartProvider storageKey="test-site-shell-cart-badge">
        <CartSeedAction />
        <SiteShell
          heading="Catalog Collection"
          description="Browse single-origin spices and house blends."
          actionText="Browse Collection"
        />
      </CartProvider>
    );

    await user.click(screen.getByRole("button", { name: "Seed Cart" }));

    await waitFor(() => {
      expect(screen.getByLabelText("1 item in cart")).toBeInTheDocument();
    });
  });
});
