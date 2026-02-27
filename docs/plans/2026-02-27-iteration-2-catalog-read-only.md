# Iteration 2 Catalog Read-Only + Luxury Minimal UI Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement Iteration 2 end-to-end with a seeded product catalog, backend catalog APIs, a real shop browsing experience with filters, and product detail pages with weight variants, while restyling the UI to a dark red/black/white luxury-minimal design language.

**Architecture:** Keep the unified deployment model: Next.js static export served by ASP.NET Core from `wwwroot` and same-origin API calls to `/api/*`. Add an in-memory seeded catalog in backend for this iteration and consume it from client-side frontend components for listing/filter/detail flows.

**Tech Stack:** ASP.NET Core 8 minimal APIs, C# records, xUnit + ASP.NET Core integration testing, Next.js 14 (App Router, static export), React 18, TypeScript, Jest + Testing Library, Tailwind + custom CSS variables.

---

### Task 1: Add Failing Backend API Tests for Catalog List and Detail

**Files:**
- Create: `src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj`
- Create: `src/backend/Gaaunikh.Api.Tests/CatalogApiTests.cs`
- Create: `src/backend/Gaaunikh.Api.Tests/Usings.cs`

**Step 1: Write the failing test**

Create tests that validate:
- `GET /api/catalog/products` returns non-empty seeded products.
- `GET /api/catalog/products?search=chili` returns filtered subset.
- `GET /api/catalog/products/{slug}` returns product with variants.
- `GET /api/catalog/products/{slug}` returns 404 for unknown slug.

**Step 2: Run test to verify it fails**

Run: `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj`
Expected: FAIL because `/api/catalog/*` endpoints do not exist.

**Step 3: Write minimal implementation**

Implement seeded product model and minimal API endpoints in backend.

**Step 4: Run test to verify it passes**

Run: `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj`
Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api.Tests src/backend/Gaaunikh.Api/Program.cs
git commit -m "feat(api): add seeded catalog list and detail endpoints"
```

### Task 2: Add Failing Frontend Tests for Shop Catalog and Detail Flows

**Files:**
- Create: `src/frontend/components/catalog-shop.test.tsx`
- Create: `src/frontend/components/catalog-detail.test.tsx`

**Step 1: Write the failing test**

Add tests that assert:
- Shop view renders fetched products and client-side search/category filtering.
- Product detail view renders product name and all weight variants.
- Not-found detail state appears for missing products.

**Step 2: Run test to verify it fails**

Run: `cd src/frontend; npm test -- components/catalog-shop.test.tsx components/catalog-detail.test.tsx`
Expected: FAIL because catalog components do not exist.

**Step 3: Write minimal implementation**

Add the catalog UI components and API helpers required by tests.

**Step 4: Run test to verify it passes**

Run: `cd src/frontend; npm test -- components/catalog-shop.test.tsx components/catalog-detail.test.tsx`
Expected: PASS.

**Step 5: Commit**

```bash
git add src/frontend/components/catalog-*.tsx src/frontend/components/catalog-*.test.tsx src/frontend/lib
git commit -m "feat(frontend): add catalog shop and product detail client flows"
```

### Task 3: Wire Next.js Routes for Iteration 2 Catalog Behavior

**Files:**
- Modify: `src/frontend/app/shop/page.tsx`
- Create: `src/frontend/app/shop/[slug]/page.tsx`
- Modify: `src/frontend/app/routes.test.tsx`

**Step 1: Write the failing test**

Extend route tests to assert:
- `/shop` route renders catalog heading and list shell.
- `/shop/[slug]` route renders detail experience.

**Step 2: Run test to verify it fails**

Run: `cd src/frontend; npm test -- app/routes.test.tsx`
Expected: FAIL due missing detail route wiring and catalog route output.

**Step 3: Write minimal implementation**

Wire route pages to render catalog components and maintain existing nav flows.

**Step 4: Run test to verify it passes**

Run: `cd src/frontend; npm test -- app/routes.test.tsx`
Expected: PASS.

**Step 5: Commit**

```bash
git add src/frontend/app/shop/page.tsx src/frontend/app/shop/[slug]/page.tsx src/frontend/app/routes.test.tsx
git commit -m "feat(frontend): wire shop and product detail routes"
```

### Task 4: Restyle UI to Dark Red, Black, White Luxury-Minimal System

**Files:**
- Modify: `src/frontend/app/globals.css`
- Modify: `src/frontend/components/site-shell.tsx`
- Modify: `src/frontend/components/route-content.ts`
- Modify: `src/frontend/components/site-shell.test.tsx`

**Step 1: Write the failing test**

Add assertions for updated shell copy/structure that represents the new minimal design tokens and retained primary nav.

**Step 2: Run test to verify it fails**

Run: `cd src/frontend; npm test -- components/site-shell.test.tsx`
Expected: FAIL until shell text/structure updates are applied.

**Step 3: Write minimal implementation**

Apply a restrained theme:
- CSS variables: black, dark red, white only.
- Simplified spacing, borders, typography, and card styling.
- Keep responsive behavior for desktop/mobile.

**Step 4: Run test to verify it passes**

Run: `cd src/frontend; npm test -- components/site-shell.test.tsx`
Expected: PASS.

**Step 5: Commit**

```bash
git add src/frontend/app/globals.css src/frontend/components/site-shell.tsx src/frontend/components/route-content.ts src/frontend/components/site-shell.test.tsx
git commit -m "feat(frontend): apply luxury minimal dark theme"
```

### Task 5: Full Verification and Iteration Notes Update

**Files:**
- Modify: `docs/ITERATION-1-NOTES.md` (append Iteration 2 verification section)

**Step 1: Run full verification**

Run in order:
- `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj`
- `dotnet build src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj`
- `cd src/frontend; npm test`
- `cd src/frontend; npm run build`

Expected:
- backend tests pass
- backend build passes
- frontend tests pass
- frontend static export build passes

**Step 2: Update docs**

Add Iteration 2 verification commands and smoke checks:
- `/shop` lists products
- `/shop/<slug>` shows variants
- `/api/catalog/products` and `/api/catalog/products/{slug}` return expected payloads

**Step 3: Final commit**

```bash
git add docs/ITERATION-1-NOTES.md
git commit -m "docs: add iteration 2 verification commands and smoke tests"
```
