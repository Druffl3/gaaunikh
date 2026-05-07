import { describe, expect, it, jest } from "@jest/globals";
import { fetchCatalogProduct, fetchCatalogProducts } from "./catalog";

describe("catalog api fallback", () => {
  it("returns an empty catalog list when api endpoint is unavailable", async () => {
    const fetchMock = jest.fn(async () => ({
      ok: false,
      status: 404,
      json: async () => ({})
    }));

    Object.defineProperty(globalThis, "fetch", {
      configurable: true,
      writable: true,
      value: fetchMock
    });

    const result = await fetchCatalogProducts({ search: "", category: "All" });
    expect(result).toEqual([]);
  });

  it("returns null product detail when api endpoint is unavailable", async () => {
    const fetchMock = jest.fn(async () => ({
      ok: false,
      status: 404,
      json: async () => ({})
    }));

    Object.defineProperty(globalThis, "fetch", {
      configurable: true,
      writable: true,
      value: fetchMock
    });

    const result = await fetchCatalogProduct("kashmiri-chili-powder");
    expect(result).toBeNull();
  });
});
