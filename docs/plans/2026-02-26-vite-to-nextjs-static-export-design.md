# Vite to Next.js Static Export Design

Date: 2026-02-26
Status: Approved

## Goal
Replace the frontend Vite toolchain with a more stable React framework while preserving the current deployment model where the .NET backend serves frontend static assets from `wwwroot` in a single container.

## Approved Direction
Use Next.js (App Router, TypeScript) with static export (`output: "export"`), and keep ASP.NET Core as the only runtime process in production.

## Architecture
- Frontend framework changes from Vite to Next.js.
- Frontend build output remains static files.
- Backend keeps `UseDefaultFiles`, `UseStaticFiles`, and SPA fallback behavior.
- Docker remains a unified multi-stage build that copies frontend output into backend `wwwroot`.

## Routing and UI Behavior
- Preserve existing routes:
  - `/`
  - `/shop`
  - `/track-order`
  - `/contact`
- Preserve existing visual design and responsive behavior.
- Use Next.js navigation (`Link`) for internal route transitions.
- Continue relying on backend fallback for non-API routes on hard refresh/deep links.

## Build and Container Flow
- Remove Vite build/test configuration and dependencies.
- Add Next.js config for static export.
- Build frontend using Next.js in Docker build stage.
- Copy static export output into backend `wwwroot` in final image.
- Keep the current single app container pattern and same-origin API model.

## Testing Strategy
- Keep React Testing Library coverage for navigation and basic rendering.
- Replace Vitest with Jest + jsdom (aligned with Next.js defaults/tooling).
- Update test setup and scripts accordingly.

## Error Handling and Runtime Expectations
- `/api/*` unknown endpoints continue returning 404 from backend.
- Non-API unknown routes continue serving frontend fallback document.
- Missing frontend build artifacts continue returning backend `503` with `frontend_not_built`.

## Trade-Offs
- Pros:
  - Removes Vite-specific build issues.
  - Keeps deployment model stable and simple.
  - Avoids introducing Node runtime into production container.
- Cons:
  - No Next.js server-side features (SSR, API routes, middleware at runtime).
  - Static export requires client-side patterns for dynamic behavior.

## Out of Scope
- Splitting frontend/backend into separate runtime services.
- Introducing SSR or server components that require Node runtime in production.
- Functional changes to business flows beyond framework migration.

