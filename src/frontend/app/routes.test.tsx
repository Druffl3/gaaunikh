import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "@jest/globals";
import HomePage from "./page";
import ShopPage from "./shop/page";

describe("Next routes", () => {
  it("renders home and shop headings", () => {
    const homePage = render(<HomePage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent(
      "Pure Spice Craft From Gaaunikh Kitchens"
    );

    homePage.unmount();
    render(<ShopPage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Shop Launching Next");
  });
});
