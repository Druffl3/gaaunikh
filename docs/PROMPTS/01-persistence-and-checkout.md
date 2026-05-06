Execute Session 1 of the commerce implementation plan in this repository.

Use `superpowers:executing-plans` and follow the approved plan exactly from:
- `docs/plans/2026-05-05-orders-payments-fulfillment.md`

Also read for context:
- `docs/plans/2026-05-05-orders-payments-fulfillment-design.md`

Session scope:
- Task 1: Add Database, Domain Models, and Configuration Skeleton
- Task 2: Implement Checkout Persistence API and Customer Checkout UI

Requirements:
- Stay within Session 1 only. Do not start Razorpay, inventory, shipping, notifications, or admin operations.
- Keep the single-image deployment model intact.
- Add placeholder configuration for provider keys and secrets, but do not add real credentials.
- Use TDD as described in the plan.
- Run the exact verification commands required by the plan before completion.
- Update any docs only if required to keep Session 1 work coherent.
- Create a commit for Session 1 only after tests pass.

Deliverables expected from this session:
- PostgreSQL and EF Core foundation
- persisted order creation from checkout
- checkout frontend page and form
- configuration skeleton for Razorpay, Shiprocket, Resend, and Twilio

When you finish, summarize:
- what changed
- what verification passed
- the commit hash
- any remaining blockers for Session 2
