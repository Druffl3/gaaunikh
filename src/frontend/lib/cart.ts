export type CartLine = {
  id: string;
  productSlug: string;
  productName: string;
  weightLabel: string;
  unitPriceInr: number;
  quantity: number;
};

export type CartSummary = {
  uniqueItems: number;
  totalQuantity: number;
  subtotalInr: number;
};

export function createCartLineId(productSlug: string, weightLabel: string): string {
  return `${productSlug}::${weightLabel}`.toLowerCase();
}

export function addCartLine(lines: CartLine[], incomingLine: CartLine): CartLine[] {
  const existingIndex = lines.findIndex((line) => line.id === incomingLine.id);

  if (existingIndex < 0) {
    return [...lines, incomingLine];
  }

  return lines.map((line, index) =>
    index === existingIndex
      ? {
          ...line,
          quantity: line.quantity + incomingLine.quantity
        }
      : line
  );
}

export function updateCartLineQuantity(lines: CartLine[], lineId: string, quantity: number): CartLine[] {
  return lines.map((line) =>
    line.id === lineId
      ? {
          ...line,
          quantity
        }
      : line
  );
}

export function removeCartLine(lines: CartLine[], lineId: string): CartLine[] {
  return lines.filter((line) => line.id !== lineId);
}

export function getCartSummary(lines: CartLine[]): CartSummary {
  const totalQuantity = lines.reduce((sum, line) => sum + line.quantity, 0);
  const subtotalInr = lines.reduce((sum, line) => sum + line.unitPriceInr * line.quantity, 0);

  return {
    uniqueItems: lines.length,
    totalQuantity,
    subtotalInr
  };
}
