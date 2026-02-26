import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import App from "./App";

describe("App navigation", () => {
  it("renders required top-level links", () => {
    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>
    );

    expect(screen.getByRole("link", { name: "Home" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Shop" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Track Order" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Contact" })).toBeInTheDocument();
  });
});
