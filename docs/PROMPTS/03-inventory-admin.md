Execute Session 3 of the commerce implementation plan in this repository.

Use `superpowers:executing-plans` and follow the approved plan exactly from:
- `docs/plans/2026-05-05-orders-payments-fulfillment.md`

Also read for context:
- `docs/plans/2026-05-05-orders-payments-fulfillment-design.md`

Session scope:
- Task 4: Add Inventory Ledger, Reservation Policy, and Admin Inventory Page

Requirements:
- Stay within Session 3 only. Do not start Shiprocket shipping, notifications, or general admin order operations yet.
- Model inventory per variant/SKU with movement history rather than direct quantity overwrites.
- Keep reservation policy explicit and testable.
- Add the admin inventory page and supporting endpoints, but avoid broader admin scope beyond what this task needs.
- Use tests first and run the planned verification commands before completion.
- Create a commit for Session 3 only after tests pass.

Deliverables expected from this session:
- inventory ledger domain
- stock summary and adjustment endpoints
- reservation logic tied to paid-order flow
- admin inventory UI with low-stock visibility and adjustment controls

When you finish, summarize:
- what changed
- what verification passed
- the commit hash
- any data-model decisions Session 4 must respect
