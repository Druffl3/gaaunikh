# Gaaunikh Masala - Master Plan (Unified App Deployment)

As of February 27, 2026.

## 1. Goal
Launch a production-ready online spice powder store for **Gaaunikh Masala** using a unified app deployment model (currently DigitalOcean App Platform), with reliable ordering, payment capture, and delivery tracking.

## 2. Architecture Baseline
- Frontend: Next.js static export + TypeScript + Tailwind CSS
- Backend: .NET 8 Web API serving frontend static assets (`wwwroot`) and route fallback
- Database: PostgreSQL 16
- Cache: Redis
- Packaging: single multi-stage Docker build (`frontend build -> copy out -> backend publish -> one runtime image`)
- Edge: DigitalOcean App Platform TLS and ingress (Nginx optional only for self-managed VM deployments)
- Deployment: DigitalOcean App Platform for production app runtime, Docker Compose for local development
- Versioning: Git only (no CI/CD pipelines)

## 3. Product Scope
- Hero storefront page matching approved visual direction
- Shopping centre: catalog, filtering, product detail, cart, checkout
- Inventory management: SKU stock in/out and low-stock indicators
- Online payment: gateway checkout and webhook reconciliation
- Delivery and tracking: shipment records, AWB/courier, status timeline

## 4. Delivery Principles
- Every iteration must ship a usable business outcome.
- Keep schema migration-safe from the first release.
- Payment and inventory updates must be idempotent.
- Keep a manual deployment and rollback runbook from day one.

## 5. Environments
- Local: Docker Compose stack where one app container serves both frontend and API
- Production: DigitalOcean App Platform service using the same unified app image + managed secrets + backups

## 6. Database Hosting Strategy (Budget-First)
- Keep iteration sequence, but add a mandatory DB decision gate before checkout/order persistence work.
- Prefer DigitalOcean-managed options first to reduce operational complexity.
- Budget guidance:
  - Lowest temporary cost: App Platform Dev DB (`$7/month`) for short-lived/non-production usage.
  - Best production value: DigitalOcean Managed PostgreSQL (starts around `$15/month`) with backups and managed operations.
  - Cross-provider option: Neon (free/usage-based tiers) only if strict cost constraints outweigh additional operational complexity.
- Production recommendation: use DigitalOcean Managed PostgreSQL by the start of Iteration 4.

---

## Tiny Iteration Plans (Value Per Iteration)

## Iteration 1 - Brand Landing MVP (2-3 days)
### Scope
- Backend-hosted frontend pipeline (Vite build output served by .NET)
- SPA fallback routing and `/api/health` endpoint
- Hero page and branding for Gaaunikh Masala
- Navigation: Home, Shop, Track Order, Contact
### Value
- Public branded presence is live from a single deployable image
### Done When
- One Docker image serves landing page and API health check end-to-end
### Detailed Plan
- See [ITERATION-1-PLAN.md](./ITERATION-1-PLAN.md)

## Iteration 2 - Catalog Read-Only (3-4 days)
### Scope
- Product model and seed catalog
- Catalog APIs and shop grid with basic filters
- Product details page with weight variants
### Value
- Customers can browse real products
### Done When
- Browse and product detail flows work end-to-end via backend-served frontend

## Iteration 3 - Cart and Pricing (3-4 days)
### Scope
- Add/update/remove cart items
- Cart total and pricing summary
### Value
- Customers can prepare an order basket
### Done When
- Cart persists for session and totals are accurate in the unified app deployment

## Iteration 3.5 - Database Foundation and Hosting Decision (2-3 days)
### Scope
- Select production database hosting provider and size tier
- Provision PostgreSQL instance and networking/access controls
- Wire app configuration (`ConnectionStrings`) via App Platform managed secrets
- Add initial migration baseline and run migration against non-local DB
- Validate backup and restore workflow once
### Value
- Removes delivery risk before order persistence and payment work
### Done When
- Staging/production app can connect to managed PostgreSQL
- Initial schema migration runs successfully in hosted environment
- Backup + restore drill is documented with measured RTO/RPO expectations
- Monthly run-cost estimate is documented and approved

## Iteration 4 - Checkout and Order Creation (4-5 days)
### Scope
- Customer details and address capture
- Create order and order lines in the selected managed PostgreSQL database
### Value
- Business can receive structured orders
### Done When
- Order is saved with initial status and visible in admin/order view through same-origin UI/API

## Iteration 5 - Online Payment (4-5 days)
### Scope
- Razorpay (India) or Stripe integration
- Payment webhook verification and status updates
- Duplicate webhook-safe processing
### Value
- Paid orders are accepted online
### Done When
- Successful payment moves order to paid status reliably with idempotent webhook handling

## Iteration 6 - Inventory Core (3-4 days)
### Scope
- SKU stock ledger (in/out/adjust)
- Stock reserve on order creation
- Stock release on payment failure/cancel
- Low-stock flags
### Value
- Prevents overselling and supports planning
### Done When
- Inventory reflects order lifecycle accurately

## Iteration 7 - Delivery and Tracking Basic (3-4 days)
### Scope
- Admin sets courier and AWB
- Tracking page timeline: Placed, Paid, Packed, Shipped, Delivered
### Value
- Customers can self-serve order status
### Done When
- Tracking page works with real order updates in the backend-served frontend

## Iteration 8 - Admin Operations Pack (4-5 days)
### Scope
- Admin authentication and role guard
- Product CRUD
- Inventory adjustment UI
- Order status management
### Value
- Day-to-day operations handled without direct DB edits
### Done When
- Admin can operate catalog, stock, and order state safely through the unified app

## Iteration 9 - Production Hardening (3-4 days)
### Scope
- Production-grade single-image Dockerfile and Compose config
- Cache headers/versioned static assets and SPA 404 fallback behavior
- Nginx TLS, security headers, and basic rate limiting
- Daily backup script and restore test
### Value
- Production reliability and recovery baseline
### Done When
- Unified image deployment and backup restore test are verified and documented

## Iteration 10 - Go-Live Readiness (2-3 days)
### Scope
- Checkout/payment/tracking smoke tests
- Error logging and fallback pages
- Launch and rollback checklist
### Value
- Controlled launch with reduced business risk
### Done When
- Team can launch and rollback the single app image with documented steps

---

## Recommended Execution Pattern
1. Pick one iteration only.
2. Implement and verify acceptance criteria.
3. Deploy manually to DigitalOcean App Platform.
4. For Iteration 3.5, complete provider decision + migration + backup drill before Iteration 4.
5. Capture feedback and start next iteration.
