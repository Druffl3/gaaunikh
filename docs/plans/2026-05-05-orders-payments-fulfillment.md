# Orders, Payments, Fulfillment, and Tracking Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Turn the current catalog-and-cart application into a single-image commerce app with persisted checkout, Razorpay payments, inventory management, Shiprocket shipping/tracking, and customer notifications through Resend and Twilio WhatsApp.

**Architecture:** Keep the current unified deployment model: Next.js static export served by ASP.NET Core, same-origin APIs, PostgreSQL as the source of truth, and in-process hosted services for durable background work. Provider callbacks are stored raw, processed idempotently, and fan out through database-backed jobs so the app remains deployable as one Docker image on any platform.

**Tech Stack:** ASP.NET Core 8, C#, EF Core, PostgreSQL, xUnit integration tests, Next.js 14 static export, React 18, TypeScript, Jest + Testing Library, Docker Compose, Razorpay, Shiprocket, Resend, Twilio WhatsApp.

---

## Session Map

The work is intentionally split into session-sized deliverables. Execute sessions in order:

1. Persistence and checkout foundation
2. Razorpay payment flow and webhook verification
3. Inventory domain and admin inventory page
4. Shiprocket shipment creation and tracking timeline
5. Resend and Twilio notification pipeline
6. Admin operations for orders and fulfillment
7. Production hardening and full smoke verification

### Task 1: Add Database, Domain Models, and Configuration Skeleton

**Files:**
- Modify: `src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj`
- Modify: `src/backend/Gaaunikh.Api/appsettings.json`
- Modify: `docker-compose.yml`
- Create: `src/backend/Gaaunikh.Api/Configuration/CommerceOptions.cs`
- Create: `src/backend/Gaaunikh.Api/Data/CommerceDbContext.cs`
- Create: `src/backend/Gaaunikh.Api/Data/Entities/*.cs`
- Create: `src/backend/Gaaunikh.Api/Data/Migrations/*`
- Create: `src/backend/Gaaunikh.Api.Tests/DatabaseConfigurationTests.cs`

**Step 1: Write the failing test**

Create integration tests that assert:
- the app boots with PostgreSQL configuration present
- the EF Core context can resolve from DI
- required option sections for payments, shipping, and notifications bind successfully

**Step 2: Run test to verify it fails**

Run: `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter DatabaseConfigurationTests`
Expected: FAIL because EF Core, option types, and domain entities do not exist.

**Step 3: Write minimal implementation**

Implement:
- EF Core package references
- `CommerceDbContext`
- initial entities for orders, payments, shipments, notifications, inventory, and callback logs
- option classes for Razorpay, Shiprocket, Resend, and Twilio placeholders
- Docker Compose PostgreSQL service and backend connection string wiring

**Step 4: Run test to verify it passes**

Run: `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter DatabaseConfigurationTests`
Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj src/backend/Gaaunikh.Api/appsettings.json docker-compose.yml src/backend/Gaaunikh.Api/Configuration src/backend/Gaaunikh.Api/Data src/backend/Gaaunikh.Api.Tests/DatabaseConfigurationTests.cs
git commit -m "feat(data): add commerce database foundation and provider config skeleton"
```

### Task 2: Implement Checkout Persistence API and Customer Checkout UI

**Files:**
- Create: `src/backend/Gaaunikh.Api/Features/Orders/CheckoutRequest.cs`
- Create: `src/backend/Gaaunikh.Api/Features/Orders/CheckoutResponse.cs`
- Create: `src/backend/Gaaunikh.Api/Features/Orders/OrderService.cs`
- Modify: `src/backend/Gaaunikh.Api/Program.cs`
- Create: `src/backend/Gaaunikh.Api.Tests/CheckoutApiTests.cs`
- Create: `src/frontend/components/checkout-form.tsx`
- Create: `src/frontend/components/checkout-form.test.tsx`
- Create: `src/frontend/app/checkout/page.tsx`
- Modify: `src/frontend/components/cart-panel.tsx`
- Modify: `src/frontend/components/route-content.ts`
- Modify: `src/frontend/app/routes.test.tsx`

**Step 1: Write the failing test**

Add backend and frontend tests that assert:
- checkout API accepts customer details and cart lines
- backend persists order and immutable order lines
- frontend renders checkout form fields and submits successfully
- cart page exposes a path into checkout

**Step 2: Run test to verify it fails**

Run:
- `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter CheckoutApiTests`
- `cd src/frontend; npm test -- components/checkout-form.test.tsx app/routes.test.tsx`

Expected: FAIL because checkout endpoint and page do not exist.

**Step 3: Write minimal implementation**

Implement:
- checkout API
- authoritative server-side cart total calculation
- order persistence
- frontend checkout page and form
- navigation path from cart to checkout

**Step 4: Run test to verify it passes**

Run the same commands.
Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api/Features/Orders src/backend/Gaaunikh.Api/Program.cs src/backend/Gaaunikh.Api.Tests/CheckoutApiTests.cs src/frontend/components/checkout-form.tsx src/frontend/components/checkout-form.test.tsx src/frontend/app/checkout/page.tsx src/frontend/components/cart-panel.tsx src/frontend/components/route-content.ts src/frontend/app/routes.test.tsx
git commit -m "feat(checkout): persist customer orders from checkout flow"
```

### Task 3: Add Razorpay Order Creation and Verified Payment Webhook Handling

**Files:**
- Create: `src/backend/Gaaunikh.Api/Features/Payments/*.cs`
- Create: `src/backend/Gaaunikh.Api/Infrastructure/Payments/RazorpayGateway.cs`
- Create: `src/backend/Gaaunikh.Api/Infrastructure/Payments/IRazorpayGateway.cs`
- Create: `src/backend/Gaaunikh.Api.Tests/RazorpayPaymentTests.cs`
- Modify: `src/backend/Gaaunikh.Api/Program.cs`
- Create: `src/frontend/components/payment-step.tsx`
- Create: `src/frontend/components/payment-step.test.tsx`
- Modify: `src/frontend/app/checkout/page.tsx`

**Step 1: Write the failing test**

Add tests that assert:
- backend creates a Razorpay order for a pending order
- webhook signature verification is required
- duplicate webhook payloads do not double-mark payment or duplicate downstream work
- checkout page renders a payment handoff state

**Step 2: Run test to verify it fails**

Run:
- `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter RazorpayPaymentTests`
- `cd src/frontend; npm test -- components/payment-step.test.tsx`

Expected: FAIL because payment flow is not implemented.

**Step 3: Write minimal implementation**

Implement:
- Razorpay gateway abstraction
- payment attempt persistence
- create-payment endpoint
- payment callback endpoint
- webhook endpoint with signature validation and callback logging
- frontend payment step with placeholder config hooks

**Step 4: Run test to verify it passes**

Run the same commands.
Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api/Features/Payments src/backend/Gaaunikh.Api/Infrastructure/Payments src/backend/Gaaunikh.Api/Program.cs src/backend/Gaaunikh.Api.Tests/RazorpayPaymentTests.cs src/frontend/components/payment-step.tsx src/frontend/components/payment-step.test.tsx src/frontend/app/checkout/page.tsx
git commit -m "feat(payments): add Razorpay order creation and verified payment handling"
```

### Task 4: Add Inventory Ledger, Reservation Policy, and Admin Inventory Page

**Files:**
- Create: `src/backend/Gaaunikh.Api/Features/Inventory/*.cs`
- Create: `src/backend/Gaaunikh.Api.Tests/InventoryTests.cs`
- Modify: `src/backend/Gaaunikh.Api/Program.cs`
- Create: `src/frontend/components/admin/inventory-page.tsx`
- Create: `src/frontend/components/admin/inventory-page.test.tsx`
- Create: `src/frontend/app/admin/inventory/page.tsx`
- Modify: `src/frontend/app/routes.test.tsx`

**Step 1: Write the failing test**

Add tests that assert:
- inventory movements compute on-hand/reserved/available correctly
- payment-confirmed orders trigger stock reservation policy consistently
- inventory items can persist and return an assigned image reference for admin and catalog views
- whole and mix spice inventory supports only `250g`, `500g`, and `1kg` unit labels
- admin inventory page renders quantities, low-stock state, image assignment, supported unit options, and adjustment controls

**Step 2: Run test to verify it fails**

Run:
- `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter InventoryTests`
- `cd src/frontend; npm test -- components/admin/inventory-page.test.tsx app/routes.test.tsx`

Expected: FAIL because inventory domain and admin page do not exist.

**Step 3: Write minimal implementation**

Implement:
- inventory entities and movement rules, including assigned image metadata
- stock adjustment endpoint
- stock summary endpoint
- fixed unit options for whole and mix spices: `250g`, `500g`, and `1kg`
- admin inventory page with manual adjustment flow and image assignment

**Step 4: Run test to verify it passes**

Run the same commands.
Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api/Features/Inventory src/backend/Gaaunikh.Api/Program.cs src/backend/Gaaunikh.Api.Tests/InventoryTests.cs src/frontend/components/admin/inventory-page.tsx src/frontend/components/admin/inventory-page.test.tsx src/frontend/app/admin/inventory/page.tsx src/frontend/app/routes.test.tsx
git commit -m "feat(inventory): add stock ledger and admin inventory management"
```

### Task 5: Add Shiprocket Shipment Creation, Tracking Sync, and Customer Tracking Page

**Files:**
- Create: `src/backend/Gaaunikh.Api/Features/Shipping/*.cs`
- Create: `src/backend/Gaaunikh.Api/Infrastructure/Shipping/IShiprocketGateway.cs`
- Create: `src/backend/Gaaunikh.Api/Infrastructure/Shipping/ShiprocketGateway.cs`
- Create: `src/backend/Gaaunikh.Api/Infrastructure/BackgroundJobs/*.cs`
- Create: `src/backend/Gaaunikh.Api.Tests/ShippingTests.cs`
- Modify: `src/backend/Gaaunikh.Api/Program.cs`
- Create: `src/frontend/components/tracking-page.tsx`
- Create: `src/frontend/components/tracking-page.test.tsx`
- Modify: `src/frontend/app/track-order/page.tsx`

**Step 1: Write the failing test**

Add tests that assert:
- paid orders enqueue shipment creation work
- shipment creation stores AWB/courier/tracking data
- tracking webhooks are idempotent
- polling fallback can update stale shipments
- customer tracking page renders a timeline from persisted tracking events

**Step 2: Run test to verify it fails**

Run:
- `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter ShippingTests`
- `cd src/frontend; npm test -- components/tracking-page.test.tsx`

Expected: FAIL because shipment orchestration and tracking UI do not exist.

**Step 3: Write minimal implementation**

Implement:
- Shiprocket adapter
- shipment creation background job
- tracking event persistence
- webhook and polling handlers
- customer tracking page using secure lookup token flow

**Step 4: Run test to verify it passes**

Run the same commands.
Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api/Features/Shipping src/backend/Gaaunikh.Api/Infrastructure/Shipping src/backend/Gaaunikh.Api/Infrastructure/BackgroundJobs src/backend/Gaaunikh.Api/Program.cs src/backend/Gaaunikh.Api.Tests/ShippingTests.cs src/frontend/components/tracking-page.tsx src/frontend/components/tracking-page.test.tsx src/frontend/app/track-order/page.tsx
git commit -m "feat(shipping): add Shiprocket fulfillment and customer tracking timeline"
```

### Task 6: Add Resend Email and Twilio WhatsApp Notification Pipeline

**Files:**
- Create: `src/backend/Gaaunikh.Api/Features/Notifications/*.cs`
- Create: `src/backend/Gaaunikh.Api/Infrastructure/Notifications/IEmailSender.cs`
- Create: `src/backend/Gaaunikh.Api/Infrastructure/Notifications/IWhatsAppSender.cs`
- Create: `src/backend/Gaaunikh.Api/Infrastructure/Notifications/ResendEmailSender.cs`
- Create: `src/backend/Gaaunikh.Api/Infrastructure/Notifications/TwilioWhatsAppSender.cs`
- Create: `src/backend/Gaaunikh.Api.Tests/NotificationTests.cs`
- Create: `src/frontend/components/order-confirmation.tsx`
- Create: `src/frontend/components/order-confirmation.test.tsx`
- Create: `src/frontend/app/order/[reference]/page.tsx`

**Step 1: Write the failing test**

Add tests that assert:
- order/payment/shipment events enqueue notifications
- failed notification delivery is recorded and retryable
- confirmation page renders order reference and tracking entry points

**Step 2: Run test to verify it fails**

Run:
- `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter NotificationTests`
- `cd src/frontend; npm test -- components/order-confirmation.test.tsx`

Expected: FAIL because notification pipeline and confirmation page do not exist.

**Step 3: Write minimal implementation**

Implement:
- notification job creation and persistence
- provider adapters for Resend and Twilio WhatsApp
- retryable delivery attempts
- order confirmation page

**Step 4: Run test to verify it passes**

Run the same commands.
Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api/Features/Notifications src/backend/Gaaunikh.Api/Infrastructure/Notifications src/backend/Gaaunikh.Api.Tests/NotificationTests.cs src/frontend/components/order-confirmation.tsx src/frontend/components/order-confirmation.test.tsx src/frontend/app/order/[reference]/page.tsx
git commit -m "feat(notifications): add Resend email and Twilio WhatsApp delivery updates"
```

### Task 7: Add Admin Order and Fulfillment Operations

**Files:**
- Create: `src/backend/Gaaunikh.Api/Features/Admin/*.cs`
- Create: `src/backend/Gaaunikh.Api.Tests/AdminOperationsTests.cs`
- Modify: `src/backend/Gaaunikh.Api/Program.cs`
- Create: `src/frontend/components/admin/order-list.tsx`
- Create: `src/frontend/components/admin/order-detail.tsx`
- Create: `src/frontend/components/admin/order-ops.test.tsx`
- Create: `src/frontend/app/admin/orders/page.tsx`
- Create: `src/frontend/app/admin/orders/[id]/page.tsx`

**Step 1: Write the failing test**

Add tests that assert:
- admin can list orders and inspect payment/shipment state
- admin can retry shipment syncs and notification sends
- admin can set fulfillment state transitions that are allowed by business rules

**Step 2: Run test to verify it fails**

Run:
- `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter AdminOperationsTests`
- `cd src/frontend; npm test -- components/admin/order-ops.test.tsx`

Expected: FAIL because admin operations do not exist.

**Step 3: Write minimal implementation**

Implement:
- admin order APIs
- retry endpoints for shipment and notification jobs
- admin pages for order list and order detail operations
- placeholder auth guard structure if full auth is deferred by one slice

**Step 4: Run test to verify it passes**

Run the same commands.
Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api/Features/Admin src/backend/Gaaunikh.Api/Program.cs src/backend/Gaaunikh.Api.Tests/AdminOperationsTests.cs src/frontend/components/admin/order-list.tsx src/frontend/components/admin/order-detail.tsx src/frontend/components/admin/order-ops.test.tsx src/frontend/app/admin/orders/page.tsx src/frontend/app/admin/orders/[id]/page.tsx
git commit -m "feat(admin): add order and fulfillment operations"
```

### Task 8: Wire Single-Image Runtime, Provider Stub Config, and Operational Docs

**Files:**
- Modify: `Dockerfile`
- Modify: `docker-compose.yml`
- Modify: `src/backend/Gaaunikh.Api/appsettings.json`
- Modify: `docs/PLAN.md`
- Modify: `docs/ITERATION-1-NOTES.md`
- Create: `docs/plans/2026-05-05-runtime-smoke-checklist.md`

**Step 1: Write the failing test**

Define smoke expectations for:
- backend serves frontend routes
- checkout API is reachable
- payment webhook endpoint is reachable
- tracking page is reachable
- admin pages load
- container starts with placeholder provider settings without performing real external calls

**Step 2: Run verification to show current gap**

Run:
- `cd src/frontend; npm run build`
- `dotnet build src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj`
- `docker compose build`

Expected: one or more commands fail or remain incomplete because the runtime wiring and docs are stale.

**Step 3: Write minimal implementation**

Implement:
- final Docker image wiring for frontend export + backend runtime + database dependency expectations
- placeholder configuration patterns and example environment variable names
- updated docs for local bring-up and provider secret injection

**Step 4: Run verification to confirm pass**

Run:
- `cd src/frontend; npm test`
- `cd src/frontend; npm run build`
- `dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj`
- `dotnet build src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj`
- `docker compose up --build -d`
- smoke test `/`, `/shop`, `/cart`, `/checkout`, `/track-order`, `/api/health`

Expected:
- frontend tests/build pass
- backend tests/build pass
- unified container runs
- smoke routes respond successfully

**Step 5: Commit**

```bash
git add Dockerfile docker-compose.yml src/backend/Gaaunikh.Api/appsettings.json docs/PLAN.md docs/ITERATION-1-NOTES.md docs/plans/2026-05-05-runtime-smoke-checklist.md
git commit -m "docs(runtime): finalize single-image commerce runtime and smoke checklist"
```

## Notes for Execution

- Keep provider credentials empty in source control and inject through environment variables only.
- Use mocked provider adapters until real credentials are supplied.
- Prefer adding auth scaffolding for admin paths early, even if final sign-in UX is completed in a later slice.
- Do not skip idempotency tests for payment and shipping callbacks; those are core correctness checks, not optional hardening.
