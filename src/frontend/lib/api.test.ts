import { describe, expect, it } from "@jest/globals";
import { resolveApiUrl } from "./api";

describe("resolveApiUrl", () => {
  it("targets the backend app when running from local Next dev", () => {
    expect(resolveApiUrl("/api/orders/checkout", "http://localhost:3000")).toBe(
      "http://localhost:5108/api/orders/checkout"
    );
  });

  it("keeps same-origin requests when the app is not running from local Next dev", () => {
    expect(resolveApiUrl("/api/orders/checkout", "http://localhost:8080")).toBe(
      "/api/orders/checkout"
    );
  });

  it("prefers an explicit public API base URL", () => {
    process.env.NEXT_PUBLIC_API_BASE_URL = "http://localhost:8080/";

    expect(resolveApiUrl("/api/orders/checkout", "http://localhost:3000")).toBe(
      "http://localhost:8080/api/orders/checkout"
    );

    delete process.env.NEXT_PUBLIC_API_BASE_URL;
  });
});
