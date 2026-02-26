# Vite to Next.js Static Export Migration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace the frontend Vite stack with Next.js static export while keeping the current single-container deployment where ASP.NET Core serves frontend static assets.

**Architecture:** Migrate `src/frontend` to a Next.js App Router project configured with `output: "export"`. Keep frontend routes static (`/`, `/shop`, `/track-order`, `/contact`) and copy exported output into backend `wwwroot` in Docker. Backend request handling (`/api/*` + SPA/static fallback behavior) stays intact.

**Tech Stack:** Next.js (App Router, TypeScript), React 18, React Testing Library, Jest + jsdom, ASP.NET Core 8, Docker Compose.

---

### Task 1: Set Up Next.js + Jest Baseline

**Files:**
- Modify: `src/frontend/package.json`
- Modify: `src/frontend/tsconfig.json`
- Create: `src/frontend/next.config.ts`
- Create: `src/frontend/jest.config.ts`
- Create: `src/frontend/jest.setup.ts`
- Delete: `src/frontend/vite.config.ts`
- Delete: `src/frontend/src/test/setup.ts`

**Step 1: Write the failing test**

Create `src/frontend/components/site-shell.test.tsx`:

```tsx
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "@jest/globals";
import { SiteShell } from "./site-shell";

describe("SiteShell", () => {
  it("renders primary navigation links", () => {
    render(
      <SiteShell
        heading="Pure Spice Craft From Gaaunikh Kitchens"
        description="Freshly ground masalas..."
        actionText="Discover the Brand"
      />
    );

    expect(screen.getByRole("link", { name: "Home" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Shop" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Track Order" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Contact" })).toBeInTheDocument();
  });
});
```

**Step 2: Run test to verify it fails**

Run: `cd src/frontend; npm test -- components/site-shell.test.tsx`
Expected: FAIL with module-not-found error for `./site-shell`.

**Step 3: Write minimal implementation**

Create `src/frontend/components/site-shell.tsx` with only the markup needed to satisfy the test and use `next/link` for internal navigation.

**Step 4: Run test to verify it passes**

Run: `cd src/frontend; npm test -- components/site-shell.test.tsx`
Expected: PASS.

**Step 5: Commit**

```bash
git add src/frontend/package.json src/frontend/tsconfig.json src/frontend/next.config.ts src/frontend/jest.config.ts src/frontend/jest.setup.ts src/frontend/components/site-shell.test.tsx src/frontend/components/site-shell.tsx
git rm src/frontend/vite.config.ts src/frontend/src/test/setup.ts
git commit -m "chore(frontend): switch tooling baseline from vite to next and jest"
```

### Task 2: Implement Next App Router Pages with Shared Route Content

**Files:**
- Create: `src/frontend/app/layout.tsx`
- Create: `src/frontend/app/globals.css`
- Create: `src/frontend/app/page.tsx`
- Create: `src/frontend/app/shop/page.tsx`
- Create: `src/frontend/app/track-order/page.tsx`
- Create: `src/frontend/app/contact/page.tsx`
- Create: `src/frontend/components/route-content.ts`
- Modify: `src/frontend/components/site-shell.tsx`

**Step 1: Write the failing test**

Create `src/frontend/app/routes.test.tsx`:

```tsx
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "@jest/globals";
import HomePage from "./page";
import ShopPage from "./shop/page";

describe("Next routes", () => {
  it("renders home and shop headings", () => {
    render(<HomePage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent(
      "Pure Spice Craft From Gaaunikh Kitchens"
    );

    render(<ShopPage />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Shop Launching Next");
  });
});
```

**Step 2: Run test to verify it fails**

Run: `cd src/frontend; npm test -- app/routes.test.tsx`
Expected: FAIL because page modules do not exist yet.

**Step 3: Write minimal implementation**

- Create `route-content.ts` as a single source of truth for route metadata.
- Build each page (`app/page.tsx`, `app/shop/page.tsx`, `app/track-order/page.tsx`, `app/contact/page.tsx`) using `SiteShell`.
- Keep copy and route labels identical to current Vite app.

**Step 4: Run test to verify it passes**

Run: `cd src/frontend; npm test -- app/routes.test.tsx`
Expected: PASS.

**Step 5: Commit**

```bash
git add src/frontend/app src/frontend/components/route-content.ts src/frontend/components/site-shell.tsx src/frontend/app/routes.test.tsx
git commit -m "feat(frontend): port landing routes to next app router"
```

### Task 3: Port Styling and Remove Legacy Vite Entry Files

**Files:**
- Modify: `src/frontend/app/globals.css`
- Delete: `src/frontend/src/App.tsx`
- Delete: `src/frontend/src/main.tsx`
- Delete: `src/frontend/src/styles.css`
- Delete: `src/frontend/src/vite-env.d.ts`
- Delete: `src/frontend/index.html`
- Delete: `src/frontend/tsconfig.app.json`
- Delete: `src/frontend/tsconfig.node.json`

**Step 1: Write the failing test**

Add to `src/frontend/components/site-shell.test.tsx`:

```tsx
it("renders the brand text and route action", () => {
  render(
    <SiteShell
      heading="Shop Launching Next"
      description="Catalog is preparing for release."
      actionText="Get Notified"
    />
  );

  expect(screen.getByText("Gaaunikh")).toBeInTheDocument();
  expect(screen.getByRole("button", { name: "Get Notified" })).toBeInTheDocument();
});
```

**Step 2: Run test to verify it fails**

Run: `cd src/frontend; npm test -- components/site-shell.test.tsx`
Expected: FAIL if the existing shell markup does not include required brand/action structure.

**Step 3: Write minimal implementation**

- Move existing CSS from `src/styles.css` into `app/globals.css`.
- Ensure layout imports `globals.css`.
- Ensure `SiteShell` keeps the same semantic structure and text used by tests.
- Remove legacy Vite-only entry files.

**Step 4: Run test to verify it passes**

Run: `cd src/frontend; npm test -- components/site-shell.test.tsx`
Expected: PASS.

**Step 5: Commit**

```bash
git add src/frontend/app/globals.css src/frontend/components/site-shell.test.tsx src/frontend/components/site-shell.tsx src/frontend/app/layout.tsx
git rm src/frontend/src/App.tsx src/frontend/src/main.tsx src/frontend/src/styles.css src/frontend/src/vite-env.d.ts src/frontend/index.html src/frontend/tsconfig.app.json src/frontend/tsconfig.node.json
git commit -m "refactor(frontend): migrate landing styling and remove vite entrypoints"
```

### Task 4: Finalize Frontend Build Pipeline and Lock Static Export

**Files:**
- Modify: `src/frontend/package.json`
- Modify: `src/frontend/next.config.ts`
- Modify: `src/frontend/package-lock.json`

**Step 1: Write the failing test**

Run before final script/config alignment:

`cd src/frontend; npm run build`

Expected: FAIL if static export output or script chain is incomplete.

**Step 2: Run check to verify failure reason is correct**

Expected failure should be build/export configuration related (not syntax/type errors in page components).

**Step 3: Write minimal implementation**

- Ensure scripts are:
  - `dev`: `next dev`
  - `build`: `next build`
  - `start`: `next start`
  - `test`: `jest --runInBand`
- Ensure `next.config.ts` includes static export settings required for this app.
- Regenerate lockfile via `npm install`.

**Step 4: Run build to verify it passes**

Run: `cd src/frontend; npm run build`
Expected: PASS and produce `src/frontend/out` folder.

**Step 5: Commit**

```bash
git add src/frontend/package.json src/frontend/package-lock.json src/frontend/next.config.ts
git commit -m "build(frontend): produce static export output with next build"
```

### Task 5: Update Unified Docker Build to Use Next Export Output

**Files:**
- Modify: `Dockerfile`

**Step 1: Write the failing test**

Run with old copy path:

`docker compose build app`

Expected: FAIL because runtime copy references Vite `dist` output instead of Next `out`.

**Step 2: Run check to verify failure reason is correct**

Expected failure should specifically reference missing `dist` artifacts.

**Step 3: Write minimal implementation**

Update runtime copy line from:

```dockerfile
COPY --from=frontend-build /src/frontend/dist ./wwwroot
```

to:

```dockerfile
COPY --from=frontend-build /src/frontend/out ./wwwroot
```

**Step 4: Run verification to confirm pass**

Run:
- `docker compose build app`
- `docker compose up -d app`
- `curl -I http://localhost:8080/`
- `curl -I http://localhost:8080/shop`
- `curl http://localhost:8080/api/health`

Expected:
- Build succeeds
- Frontend routes return 200
- Health endpoint returns 200 with JSON payload

**Step 5: Commit**

```bash
git add Dockerfile
git commit -m "chore(docker): serve next static export from backend wwwroot"
```

### Task 6: Update Docs and Run Full Verification Before Completion

**Files:**
- Modify: `docs/ITERATION-1-NOTES.md`

**Step 1: Write the failing test**

Add a verification checklist item that requires `next build` output location (`out`) and re-run commands from notes.

**Step 2: Run check to verify docs are stale**

Run: review `docs/ITERATION-1-NOTES.md`
Expected: stale references to Vite commands and `dist`.

**Step 3: Write minimal implementation**

Update notes with Next.js commands:
- `npm install`
- `npm test`
- `npm run build`

Ensure smoke tests still include `/`, `/shop`, `/track-order`, `/contact`, `/api/health`.

**Step 4: Run full verification**

Run in order:
- `cd src/frontend; npm test`
- `cd src/frontend; npm run build`
- `cd src/backend/Gaaunikh.Api; dotnet build`
- `cd C:/dev/tinker/gaaunikh; docker compose up --build -d`
- Smoke test routes and `/api/health`

Expected: all commands pass with no new warnings/errors that indicate functional regressions.

**Step 5: Commit**

```bash
git add docs/ITERATION-1-NOTES.md
git commit -m "docs: refresh iteration notes for next static export workflow"
```

