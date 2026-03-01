# Iteration 3 Cart and Pricing Design

As of February 27, 2026.

## What Iteration 3 Is

Iteration 3 delivers a session-persistent shopping cart and a reliable pricing summary:
- add items to cart from product detail
- update quantities
- remove items
- show computed totals accurately
- keep cart state during the browser session

## Approaches Considered

### Approach 1: Frontend Session Cart (Recommended)

Store cart state in a React client provider and persist to `sessionStorage`.

Pros:
- no backend API changes for this iteration
- fastest path aligned with Iteration 3 scope
- works with static-export frontend served by .NET

Cons:
- cart is not shared across devices/browsers
- cart resets when browser session ends

### Approach 2: Backend Session API

Add API endpoints for cart mutation/read and persist per server session cookie.

Pros:
- cart survives tab closes until server session expiry
- central source of truth server-side

Cons:
- extra API and test surface before checkout/order iteration
- session scale behavior and cookie strategy needed now

### Approach 3: Hybrid (Frontend Cache + Backend Sync)

Optimistic local cart with periodic sync to backend endpoints.

Pros:
- can evolve toward checkout persistence

Cons:
- complexity is not justified for this iteration
- higher regression risk without business value gain now

## Recommended Design

Pick Approach 1 for Iteration 3, then evolve to persisted order/cart domain in Iteration 4.

### Architecture

- Add a client `CartProvider` at app shell level.
- Keep cart logic in a pure library module for deterministic tests.
- Persist cart payload under a versioned key in `sessionStorage`.
- Render cart page (`/cart`) for update/remove and pricing summary.
- Show cart count in top navigation.

### Components and Data Flow

1. Product detail loads product variants.
2. User clicks "Add to cart" for a variant.
3. Cart provider dispatches add action.
4. Cart reducer merges existing line or appends new line.
5. Updated state is written to `sessionStorage`.
6. Cart page renders lines and pricing summary from derived totals.

### Cart and Pricing Model

- Cart line identity: `productSlug + weightLabel`
- Stored fields: product name, variant label, unit price, quantity
- Derived values:
  - line total = `unitPriceInr * quantity`
  - subtotal = sum of line totals
  - total quantity = sum of quantities
  - unique items = number of lines

### Error Handling and Resilience

- invalid or corrupted storage payload falls back to empty cart
- quantity updates are clamped to minimum `1` from UI controls
- remove action always available per line

### Testing Strategy

- unit tests for cart reducer/derivations (`add`, `update`, `remove`, totals)
- component tests for cart page interactions and summary updates
- integration-style UI test for adding from product detail into cart context
- existing route/shell tests updated for new cart route/nav link

## Out of Scope (Iteration 3)

- DB persistence
- checkout address and order creation
- payment, inventory reservation, shipping charges/tax engines
