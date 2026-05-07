const localFrontendOrigins = new Set(["http://localhost:3000", "http://127.0.0.1:3000"]);
const defaultLocalApiBaseUrl = "http://localhost:5108";

function trimTrailingSlash(value: string): string {
  return value.endsWith("/") ? value.slice(0, -1) : value;
}

export function resolveApiUrl(path: string, currentOrigin?: string): string {
  const configuredBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL?.trim();

  if (configuredBaseUrl) {
    return `${trimTrailingSlash(configuredBaseUrl)}${path}`;
  }

  if (currentOrigin && localFrontendOrigins.has(currentOrigin)) {
    return `${defaultLocalApiBaseUrl}${path}`;
  }

  if (typeof window !== "undefined" && localFrontendOrigins.has(window.location.origin)) {
    return `${defaultLocalApiBaseUrl}${path}`;
  }

  return path;
}
