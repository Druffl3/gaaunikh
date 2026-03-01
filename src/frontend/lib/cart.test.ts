import { describe, expect, it } from "@jest/globals";
import {
  addCartLine,
  createCartLineId,
  getCartSummary,
  removeCartLine,
  updateCartLineQuantity,
  type CartLine
} from "./cart";

function line(overrides: Partial<CartLine> = {}): CartLine {
  return {
    id: createCartLineId("kashmiri-chili-powder", "100g"),
    productSlug: "kashmiri-chili-powder",
    productName: "Kashmiri Chili Powder",
    weightLabel: "100g",
    unitPriceInr: 95,
    quantity: 1,
    ...overrides
  };
}

describe("cart domain", () => {
  it("adds a new cart line", () => {
    const result = addCartLine([], line());
    expect(result).toHaveLength(1);
    expect(result[0]?.productSlug).toBe("kashmiri-chili-powder");
    expect(result[0]?.quantity).toBe(1);
  });

  it("increases quantity when same line is added again", () => {
    const existing = line({ quantity: 2 });
    const result = addCartLine([existing], line({ quantity: 1 }));

    expect(result).toHaveLength(1);
    expect(result[0]?.quantity).toBe(3);
  });

  it("updates quantity for an existing line", () => {
    const id = createCartLineId("kashmiri-chili-powder", "100g");
    const result = updateCartLineQuantity([line({ id, quantity: 1 })], id, 4);
    expect(result[0]?.quantity).toBe(4);
  });

  it("removes a line by id", () => {
    const keep = line({
      id: createCartLineId("haldi-gold-turmeric", "100g"),
      productSlug: "haldi-gold-turmeric"
    });
    const remove = line();

    const result = removeCartLine([keep, remove], remove.id);
    expect(result).toEqual([keep]);
  });

  it("calculates pricing summary totals", () => {
    const lines: CartLine[] = [
      line({ unitPriceInr: 95, quantity: 2 }),
      line({
        id: createCartLineId("haldi-gold-turmeric", "250g"),
        productSlug: "haldi-gold-turmeric",
        productName: "Haldi Gold Turmeric",
        weightLabel: "250g",
        unitPriceInr: 175,
        quantity: 3
      })
    ];

    const summary = getCartSummary(lines);

    expect(summary.uniqueItems).toBe(2);
    expect(summary.totalQuantity).toBe(5);
    expect(summary.subtotalInr).toBe(715);
  });
});
