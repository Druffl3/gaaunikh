# Orders, Payments, Fulfillment, and Tracking Design

As of May 5, 2026.

## Goal

Extend the current Gaaunikh single-image application from catalog plus session cart into a production-shaped commerce system that supports:
- persistent orders
- online payment with Razorpay
- inventory management
- shipment creation and tracking via Shiprocket
- customer communication through Resend email and Twilio WhatsApp
- backend-served frontend with a single Docker image deployable on any platform

## Existing Baseline

- Frontend: Next.js static export served by ASP.NET Core from `wwwroot`
- Backend: ASP.NET Core 8 minimal API
- Current product data: in-memory seeded catalog
- Current cart: browser session state only
- Deployment target: one Docker image containing frontend assets and backend runtime

## Approaches Considered

### Approach 1: Production-Shaped Monolith (Recommended)

Keep one ASP.NET Core application and one database. Serve the frontend from the same container, expose same-origin APIs, process provider webhooks internally, and run background jobs inside the same process using hosted services and database-backed work queues.

Pros:
- matches the single-image deployment requirement
- keeps operational complexity low
- supports reliable order, payment, shipment, and notification workflows
- allows strong idempotency and auditability without extra infrastructure

Cons:
- requires more up-front domain design than a demo build
- background work must be designed carefully to avoid duplicate processing

### Approach 2: Fast Demo Build

Implement checkout, payment, shipping, and notifications with minimal persistence and mostly synchronous flows.

Pros:
- fastest path to a clickable end-to-end flow

Cons:
- too fragile for real payment and fulfillment handling
- poor retry characteristics
- high risk of inconsistent state across payment, shipment, and notifications

### Approach 3: Distributed Services From Day One

Split APIs, workers, and admin into separate deployables with queue infrastructure immediately.

Pros:
- clean scaling boundary

Cons:
- directly conflicts with the current hosting and packaging goal
- too much operational overhead for this stage

## Recommended Design

Pick Approach 1 and treat the application as a modular monolith:
- one ASP.NET Core runtime
- one PostgreSQL database
- one exported Next.js frontend served by the backend
- one Docker image
- no mandatory external queue or worker service

## Architecture

The backend remains the application entry point and serves:
- static frontend assets
- customer APIs
- admin APIs
- payment webhook endpoints
- shipping webhook endpoints
- tracking lookup endpoints

Internally, the backend is organized into modules:
- `Catalog`
- `Orders`
- `Payments`
- `Inventory`
- `Shipping`
- `Notifications`
- `Admin`
- `Infrastructure`

Provider integrations are hidden behind internal interfaces:
- `IRazorpayGateway`
- `IShiprocketGateway`
- `IEmailSender`
- `IWhatsAppSender`

Concrete implementations:
- `RazorpayGateway`
- `ShiprocketGateway`
- `ResendEmailSender`
- `TwilioWhatsAppSender`

Background work stays in-process through hosted services and durable database tables. Provider webhooks update durable state first, then enqueue follow-up work for retryable processing.

## Data Model

Core persisted entities:
- `Product`
- `ProductVariant`
- `InventoryItem`
- `InventoryMovement`
- `Customer`
- `CustomerAddress`
- `Order`
- `OrderLine`
- `PaymentAttempt`
- `PaymentEvent`
- `Shipment`
- `ShipmentTrackingEvent`
- `NotificationMessage`
- `ProviderCallbackLog`
- `BackgroundJob`

Important modeling rules:
- order lines are immutable snapshots of product name, SKU, unit price, and quantity at purchase time
- inventory is tracked per variant/SKU
- provider callback payloads are stored raw for audit/debugging
- notification attempts are durable records, not fire-and-forget calls
- public tracking links use opaque lookup tokens rather than exposing internal IDs

## Order and Checkout Flow

1. Customer browses products and adds variants to the session cart.
2. Customer enters checkout details:
   - full name
   - email
   - phone
   - shipping address
3. Frontend submits cart and customer details to the backend.
4. Backend validates product/variant existence and computes authoritative totals.
5. Backend creates:
   - customer record or reuse strategy
   - order
   - order lines
   - payment attempt
6. Backend creates a Razorpay order and returns the checkout payload required by the frontend.
7. Frontend completes Razorpay checkout.
8. Backend verifies payment through callback and webhook handling before treating the order as paid.

## Payment Design

Razorpay is the only payment provider in scope for the first release.

Rules:
- frontend success is never the source of truth
- verified backend signature checks decide payment state
- duplicate webhooks must be harmless
- order and payment status transitions must be idempotent

Config placeholders to add:
- `Payments__Provider`
- `Payments__Currency`
- `Payments__Razorpay__KeyId`
- `Payments__Razorpay__KeySecret`
- `Payments__Razorpay__WebhookSecret`
- `Payments__Razorpay__AutoCapture`

## Inventory Design

Inventory is required and must be admin-manageable.

Inventory rules:
- track stock per variant SKU
- preserve an audit trail through movement rows rather than overwriting counts blindly
- expose `OnHand`, `Reserved`, `Available`, and `ReorderThreshold`
- allow each inventory item to carry an assigned image reference for admin and catalog display
- for whole and mix spices, support only `250g`, `500g`, and `1kg` unit labels
- support manual adjustments with reason codes such as:
  - `Restock`
  - `Damage`
  - `ManualCorrection`
  - `ReturnReceived`
- reserve stock on paid order or at a later transition depending on implementation choice, but make the policy explicit and testable
- release or consume stock according to order and shipment lifecycle

## Shipping and Tracking Design

Shiprocket is the first shipping integration.

Flow:
1. Paid order becomes eligible for fulfillment.
2. Backend enqueues shipment creation work.
3. Shipping adapter creates a shipment in Shiprocket.
4. Local `Shipment` record stores courier details, shipment IDs, AWB, labels, and tracking metadata.
5. Tracking updates arrive from Shiprocket webhooks and a scheduled polling fallback covers stale shipments.
6. Customer-facing tracking page renders a timeline from local persisted tracking events.

Config placeholders to add:
- `Shipping__Provider`
- `Shipping__Shiprocket__ApiBaseUrl`
- `Shipping__Shiprocket__Email`
- `Shipping__Shiprocket__Password`
- `Shipping__Shiprocket__Token`
- `Shipping__Shiprocket__WebhookSecret`
- `Shipping__Shiprocket__DefaultPickupLocation`
- `Shipping__Shiprocket__DefaultPackageLengthCm`
- `Shipping__Shiprocket__DefaultPackageWidthCm`
- `Shipping__Shiprocket__DefaultPackageHeightCm`
- `Shipping__Shiprocket__DefaultPackageWeightKg`
- `Shipping__Shiprocket__TrackingPollIntervalMinutes`

## Customer Communication Design

Notification channels:
- email through Resend
- WhatsApp through Twilio

Message events:
- order created
- payment confirmed
- shipment created with courier and tracking details
- shipped / in transit
- delivered
- shipment exception where customer communication is appropriate

Rules:
- notifications do not block order progression
- failures are logged and retried
- admin can manually retry a failed notification
- customer tracking links are sent through both email and WhatsApp where possible

Config placeholders to add:
- `Notifications__Email__Provider`
- `Notifications__Email__Resend__ApiKey`
- `Notifications__Email__FromAddress`
- `Notifications__Email__ReplyToAddress`
- `Notifications__Email__AdminAlertAddress`
- `Notifications__WhatsApp__Provider`
- `Notifications__WhatsApp__Twilio__AccountSid`
- `Notifications__WhatsApp__Twilio__AuthToken`
- `Notifications__WhatsApp__Twilio__FromNumber`
- `Notifications__WhatsApp__Twilio__StatusCallbackSecret`
- `Notifications__WhatsApp__Templates__OrderConfirmed`
- `Notifications__WhatsApp__Templates__ShipmentCreated`
- `Notifications__WhatsApp__Templates__ShipmentDelivered`

## Admin Design

Admin capabilities required:
- order list and order detail
- payment state visibility
- shipment creation status and retry controls
- tracking history visibility
- inventory list and inventory adjustments
- product and variant maintenance if kept within the current project scope

Inventory management page requirements:
- filterable list by product/category/SKU
- columns for `OnHand`, `Reserved`, `Available`, `ReorderThreshold`
- image thumbnail or assigned image field per inventory item
- inventory creation/edit flow limited to `250g`, `500g`, and `1kg` units for whole and mix spices
- adjustment form with reason and note
- recent stock movement history
- low-stock highlighting

## Security and Reliability

- provider secrets live only in configuration and environment variables
- webhook endpoints require signature validation or shared secret verification
- raw provider callbacks are stored before mutation handling
- all externally triggered handlers are idempotent
- public tracking links use opaque tokens
- admin routes require authentication and authorization before release
- all important state transitions are auditable

## Testing Strategy

Backend:
- integration tests for checkout, order creation, payment verification, shipment orchestration, tracking lookup, and inventory mutation flows
- provider adapter tests with mocked upstream responses
- webhook idempotency tests

Frontend:
- checkout form tests
- payment result and confirmation state tests
- tracking page tests
- admin inventory page tests
- admin shipment/order operation tests

System:
- Docker-based smoke path for:
  - browse
  - add to cart
  - checkout
  - simulated payment success
  - shipment creation
  - tracking lookup

## Out of Scope for the First Execution Slice

- multi-warehouse inventory
- return merchandise authorization workflows
- automatic refunds beyond provider state recording
- advanced coupon/promotions engine
- customer login/accounts
- multi-provider shipping selection in the UI

## Recommended Delivery Shape

This work is too large for a single coding session. It should be implemented in sequential deliverable slices:
1. persistence and checkout foundation
2. Razorpay integration
3. inventory core
4. Shiprocket shipping and tracking
5. Resend and Twilio notifications
6. admin operations
7. hardening, smoke tests, and deployment readiness
