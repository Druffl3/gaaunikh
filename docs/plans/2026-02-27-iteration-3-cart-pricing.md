# Iteration 3 Cart and Pricing Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Deliver Iteration 3 by adding session-persistent cart behavior (add, update, remove) and accurate pricing summary in the unified frontend/backend app.

**Architecture:** Keep cart state on the frontend for this iteration using a client provider plus `sessionStorage` persistence. Use pure TypeScript cart logic for deterministic pricing and mutation behavior, then compose UI components for product add-to-cart and cart management route.

**Tech Stack:** Next.js App Router (static export), React 18 client context/hooks, TypeScript, Jest + Testing Library.

---

### Task 1: Add Failing Tests for Cart Domain Logic

**Files:**
- Create: `src/frontend/lib/cart.test.ts`

**Step 1: Write the failing test**

Create tests for:
- adding a new line
- increasing quantity when same product/variant is added again
- updating quantity for a line
- removing a line
- computing subtotal, item count, and quantity totals

**Step 2: Run test to verify it fails**

Run: `cd src/frontend; npm test -- lib/cart.test.ts`
Expected: FAIL because `lib/cart.ts` does not exist yet.

**Step 3: Write minimal implementation**

Create `src/frontend/lib/cart.ts` with:
- cart types
- pure functions for `add`, `update`, `remove`
- pure function for pricing summary derivation

**Step 4: Run test to verify it passes**

Run: `cd src/frontend; npm test -- lib/cart.test.ts`
Expected: PASS.

### Task 2: Add Failing Tests for Cart UI and Add-To-Cart Flow

**Files:**
- Create: `src/frontend/components/cart-panel.test.tsx`
- Modify: `src/frontend/components/catalog-detail.test.tsx`

**Step 1: Write the failing test**

Add tests that assert:
- cart page renders line items and summary totals
- quantity increment/decrement updates totals
- remove deletes line and updates summary
- product detail has add-to-cart controls for variants

**Step 2: Run test to verify it fails**

Run: `cd src/frontend; npm test -- components/cart-panel.test.tsx components/catalog-detail.test.tsx`
Expected: FAIL because cart provider/panel and add-to-cart controls are missing.

**Step 3: Write minimal implementation**

Implement:
- `CartProvider` and `useCart` hooks
- `CartPanel` UI component for update/remove and pricing summary
- add-to-cart controls in product detail component

**Step 4: Run test to verify it passes**

Run: `cd src/frontend; npm test -- components/cart-panel.test.tsx components/catalog-detail.test.tsx`
Expected: PASS.

### Task 3: Wire Cart Route and Navigation

**Files:**
- Create: `src/frontend/app/cart/page.tsx`
- Modify: `src/frontend/components/site-shell.tsx`
- Modify: `src/frontend/components/site-shell.test.tsx`
- Modify: `src/frontend/app/routes.test.tsx`
- Modify: `src/frontend/components/route-content.ts`

**Step 1: Write the failing test**

Extend route/shell tests to assert:
- primary navigation includes Cart
- `/cart` route renders cart heading and panel shell

**Step 2: Run test to verify it fails**

Run: `cd src/frontend; npm test -- components/site-shell.test.tsx app/routes.test.tsx`
Expected: FAIL until nav and route are wired.

**Step 3: Write minimal implementation**

Wire:
- cart nav link
- cart route page with `SiteShell` wrapper and `CartPanel`
- route copy entries in `route-content.ts`

**Step 4: Run test to verify it passes**

Run: `cd src/frontend; npm test -- components/site-shell.test.tsx app/routes.test.tsx`
Expected: PASS.

### Task 4: Persist Cart in Browser Session

**Files:**
- Create: `src/frontend/components/cart-provider.tsx`
- Modify: `src/frontend/app/layout.tsx`

**Step 1: Write the failing test**

Add/extend cart provider tests to assert:
- provider hydrates from `sessionStorage`
- provider writes updates back to `sessionStorage`
- invalid stored payload falls back to empty cart

**Step 2: Run test to verify it fails**

Run: `cd src/frontend; npm test -- components/cart-panel.test.tsx`
Expected: FAIL until persistence hooks are added.

**Step 3: Write minimal implementation**

Implement storage hydration/write effects and wrap app layout with provider.

**Step 4: Run test to verify it passes**

Run: `cd src/frontend; npm test -- components/cart-panel.test.tsx`
Expected: PASS.

### Task 5: Full Verification and Iteration Notes Update

**Files:**
- Modify: `docs/ITERATION-1-NOTES.md`

**Step 1: Run full frontend verification**

Run in order:
- `cd src/frontend; npm test`
- `cd src/frontend; npm run build`

Expected:
- all frontend tests pass
- static export build succeeds

**Step 2: Run backend safety verification**

Run:
- `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj`
- `dotnet build src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj`

Expected:
- existing backend tests/build remain green

**Step 3: Update iteration notes**

Append Iteration 3 verification commands and smoke checks:
- add product variant to cart
- update quantity in cart
- remove item
- verify subtotal updates accurately
- verify cart state survives refresh in same browser session

