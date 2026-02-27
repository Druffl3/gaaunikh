"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import {
  addCartLine,
  createCartLineId,
  getCartSummary,
  removeCartLine,
  updateCartLineQuantity,
  type CartLine,
  type CartSummary
} from "../lib/cart";

const defaultStorageKey = "gaaunikh_cart_v1";

type AddProductVariantInput = {
  productSlug: string;
  productName: string;
  weightLabel: string;
  unitPriceInr: number;
  quantity?: number;
};

type CartContextValue = {
  lines: CartLine[];
  summary: CartSummary;
  addProductVariant: (input: AddProductVariantInput) => void;
  increaseQuantity: (lineId: string) => void;
  decreaseQuantity: (lineId: string) => void;
  removeLine: (lineId: string) => void;
};

const CartContext = createContext<CartContextValue | null>(null);

type CartProviderProps = {
  children: ReactNode;
  storageKey?: string;
};

function isCartLine(value: unknown): value is CartLine {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  const candidate = value as Partial<CartLine>;
  return (
    typeof candidate.id === "string" &&
    typeof candidate.productSlug === "string" &&
    typeof candidate.productName === "string" &&
    typeof candidate.weightLabel === "string" &&
    typeof candidate.unitPriceInr === "number" &&
    typeof candidate.quantity === "number"
  );
}

function parseStoredCart(rawValue: string | null): CartLine[] {
  if (rawValue === null) {
    return [];
  }

  try {
    const parsed = JSON.parse(rawValue) as unknown;

    if (!Array.isArray(parsed)) {
      return [];
    }

    return parsed.filter(isCartLine);
  } catch {
    return [];
  }
}

export function CartProvider({ children, storageKey = defaultStorageKey }: CartProviderProps) {
  const [lines, setLines] = useState<CartLine[]>([]);
  const [hydrated, setHydrated] = useState(false);

  useEffect(() => {
    if (typeof window === "undefined") {
      return;
    }

    const storedLines = parseStoredCart(window.sessionStorage.getItem(storageKey));
    setLines(storedLines);
    setHydrated(true);
  }, [storageKey]);

  useEffect(() => {
    if (!hydrated || typeof window === "undefined") {
      return;
    }

    window.sessionStorage.setItem(storageKey, JSON.stringify(lines));
  }, [hydrated, lines, storageKey]);

  const addProductVariant = useCallback((input: AddProductVariantInput) => {
    setLines((currentLines) =>
      addCartLine(currentLines, {
        id: createCartLineId(input.productSlug, input.weightLabel),
        productSlug: input.productSlug,
        productName: input.productName,
        weightLabel: input.weightLabel,
        unitPriceInr: input.unitPriceInr,
        quantity: input.quantity ?? 1
      })
    );
  }, []);

  const increaseQuantity = useCallback((lineId: string) => {
    setLines((currentLines) => {
      const line = currentLines.find((item) => item.id === lineId);
      if (!line) {
        return currentLines;
      }

      return updateCartLineQuantity(currentLines, lineId, line.quantity + 1);
    });
  }, []);

  const decreaseQuantity = useCallback((lineId: string) => {
    setLines((currentLines) => {
      const line = currentLines.find((item) => item.id === lineId);
      if (!line) {
        return currentLines;
      }

      if (line.quantity <= 1) {
        return removeCartLine(currentLines, lineId);
      }

      return updateCartLineQuantity(currentLines, lineId, line.quantity - 1);
    });
  }, []);

  const removeLine = useCallback((lineId: string) => {
    setLines((currentLines) => removeCartLine(currentLines, lineId));
  }, []);

  const summary = useMemo(() => getCartSummary(lines), [lines]);

  const contextValue = useMemo<CartContextValue>(
    () => ({
      lines,
      summary,
      addProductVariant,
      increaseQuantity,
      decreaseQuantity,
      removeLine
    }),
    [addProductVariant, decreaseQuantity, increaseQuantity, lines, removeLine, summary]
  );

  return <CartContext.Provider value={contextValue}>{children}</CartContext.Provider>;
}

export function useOptionalCart(): CartContextValue | null {
  return useContext(CartContext);
}

export function useCart(): CartContextValue {
  const context = useContext(CartContext);
  if (context === null) {
    throw new Error("useCart must be used inside CartProvider.");
  }

  return context;
}
