import "@testing-library/jest-dom";
import { jest } from "@jest/globals";

if (typeof globalThis.fetch === "undefined") {
  Object.defineProperty(globalThis, "fetch", {
    configurable: true,
    writable: true,
    value: jest.fn(async (input: string | URL | Request) => {
      const url = input.toString();

      if (url.includes("/api/catalog/products/")) {
        return {
          ok: false,
          status: 404,
          json: async () => ({ error: "product_not_found" })
        };
      }

      return {
        ok: true,
        status: 200,
        json: async () => ({ products: [] })
      };
    })
  });
}

