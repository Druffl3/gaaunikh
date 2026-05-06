Execute Session 6 of the commerce implementation plan in this repository.

Use `superpowers:executing-plans` and follow the approved plan exactly from:
- `docs/plans/2026-05-05-orders-payments-fulfillment.md`

Also read for context:
- `docs/plans/2026-05-05-orders-payments-fulfillment-design.md`

Session scope:
- Task 7: Add Admin Order and Fulfillment Operations

Requirements:
- Stay within Session 6 only. Do not start final runtime hardening beyond what this task strictly requires.
- Build admin order list/detail operations on top of prior sessions.
- Include retry actions for shipment sync and notification delivery as planned.
- Keep auth/authorization structure ready even if full auth UX is deferred.
- Use tests first and run the planned verification commands before completion.
- Create a commit for Session 6 only after tests pass.

Deliverables expected from this session:
- admin order APIs
- admin order list and detail pages
- retry controls for shipment and notification jobs
- allowed fulfillment state transitions enforced by business rules

When you finish, summarize:
- what changed
- what verification passed
- the commit hash
- any remaining risks for Session 7
