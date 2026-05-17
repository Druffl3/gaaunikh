# Admin Authentication and Inventory Authorization Design

As of May 7, 2026.

## Goal

Protect admin inventory management from unauthorized access by adding real authentication, role-based authorization, and auditable stock-change attribution without changing the public storefront checkout flow.

## Existing Baseline

- Backend: ASP.NET Core 8 minimal API in `src/backend/Gaaunikh.Api/Program.cs`
- Frontend: Next.js static export with client-side admin inventory UI in `src/frontend/app/admin/inventory/page.tsx`
- Current risk: `/api/admin/inventory/*` endpoints are public and the admin inventory page renders without any session check
- Current inventory audit gap: `InventoryMovement` stores reason and note, but not which admin performed the adjustment

## Approaches Considered

### Approach 1: Shared Admin Secret or Basic Auth

Protect admin endpoints with a static secret or browser basic auth.

Pros:
- fastest to ship
- small code change surface

Cons:
- poor credential hygiene
- no per-user audit trail
- weak fit for future admin order operations
- difficult to rotate safely without operational friction

### Approach 2: First-Party Cookie Session Auth with Roles (Recommended)

Use ASP.NET Core cookie authentication, store admin users in PostgreSQL, add a same-origin admin login flow, and enforce policy-based authorization on admin APIs.

Pros:
- fits the existing single-app, same-origin deployment model
- supports per-user audit attribution for inventory changes
- extends cleanly to future admin order and fulfillment pages
- avoids exposing tokens to browser storage

Cons:
- requires a small auth domain and database migration
- frontend admin pages need session-aware client behavior

### Approach 3: External Identity Provider

Use an external admin identity provider such as Auth0, Clerk, or Entra ID.

Pros:
- strong security primitives out of the box
- less password-handling code in this repo

Cons:
- adds external dependency and setup burden
- over-scoped for the current monolith and local Docker workflow
- slows down delivery of the immediate inventory protection need

## Recommended Design

Pick Approach 2.

Implement first-party admin authentication on the backend and keep the customer storefront public. The admin surface becomes a protected slice that uses a secure server-issued cookie, a small admin user table, and ASP.NET authorization policies.

## Authentication Design

Add a dedicated admin auth module:

- `AdminUser` table with:
  - `Id`
  - `Email`
  - `DisplayName`
  - `PasswordHash`
  - `Role`
  - `IsActive`
  - `LastLoginUtc`
  - `CreatedUtc`
  - `UpdatedUtc`
- password hashing through ASP.NET Core `PasswordHasher<TUser>`
- secure cookie auth scheme for admin sessions
- bootstrap seed support through environment-backed config so the first admin can be created without hardcoding credentials in source control

Recommended config shape:

- `AdminAuth__CookieName`
- `AdminAuth__SeedEmail`
- `AdminAuth__SeedPassword`
- `AdminAuth__SeedDisplayName`
- `AdminAuth__SessionMinutes`

Bootstrap rules:

- only seed when no admin users exist
- require both seed email and seed password to be present
- never commit real seed credentials to source control

## Authorization Design

Use policy-based authorization rather than string checks scattered in handlers.

Initial roles:

- `Admin`
- `InventoryManager`

Initial policies:

- `AdminInventoryRead`
- `AdminInventoryWrite`

Suggested mapping:

- `Admin` can read and write all admin inventory routes
- `InventoryManager` can read and write inventory only

Protected routes:

- `GET /api/admin/inventory/summary` requires `AdminInventoryRead`
- `POST /api/admin/inventory/items` requires `AdminInventoryWrite`
- `POST /api/admin/inventory/adjustments` requires `AdminInventoryWrite`
- future `/api/admin/orders/*` routes should reuse the same session infrastructure with order-specific policies

## Inventory Hardening

Inventory mutation security is not complete unless adjustments are attributable.

Extend `InventoryMovement` with actor metadata:

- `ActorAdminUserId`
- `ActorDisplayName`

Rules:

- manual inventory creation and stock adjustment must record the authenticated admin actor
- automatic reservation movements created from payment confirmation keep their existing system-generated semantics and may leave actor fields null
- inventory APIs must return `401` when unauthenticated and `403` when authenticated without the required role

## Frontend Design

Because the frontend is a static export, admin gating must happen through client-side session checks against backend APIs.

Add:

- `/admin/login` page
- login form component that posts to `/api/admin/auth/login`
- session probe endpoint such as `GET /api/admin/session`
- client-side admin gate wrapper for `/admin/inventory`

User flow:

1. Unauthenticated user opens `/admin/inventory`.
2. Page checks `GET /api/admin/session`.
3. If no session, render or redirect to `/admin/login`.
4. Successful login creates the auth cookie and returns current admin session metadata.
5. Inventory page reloads and fetches protected inventory data with `credentials: "include"` for session-backed requests.

## Security Details

- use `HttpOnly` cookies
- use `Secure` cookies outside local development
- use `SameSite=Lax` for the first slice because admin traffic is same-site
- keep admin APIs same-origin in production
- return generic login failure messages
- log login success/failure through normal application logging, but never log passwords
- apply light rate limiting to the login endpoint if the slice can accommodate it cleanly

## Testing Strategy

Backend:

- login success and invalid credential tests
- unauthenticated request to inventory summary returns `401`
- authenticated user without inventory role returns `403`
- authenticated inventory manager can create item and adjust stock
- inventory adjustment persists actor metadata

Frontend:

- login page renders and posts credentials
- inventory route shows auth gate when session is absent
- inventory route renders content after a valid session response
- protected inventory fetches include credentials where needed for cookie auth

## Rollout Notes

- This slice should land before broader admin order operations are expanded further.
- Existing local and deployed environments will need seed admin credentials injected through configuration before the admin UI is usable.
- No customer login is introduced in this slice; checkout and public catalog remain anonymous.
