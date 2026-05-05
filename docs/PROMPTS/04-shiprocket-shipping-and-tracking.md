Execute Session 4 of the commerce implementation plan in this repository.

Use `superpowers:executing-plans` and follow the approved plan exactly from:
- `docs/plans/2026-05-05-orders-payments-fulfillment.md`

Also read for context:
- `docs/plans/2026-05-05-orders-payments-fulfillment-design.md`

Session scope:
- Task 5: Add Shiprocket Shipment Creation, Tracking Sync, and Customer Tracking Page

Requirements:
- Stay within Session 4 only. Do not start Resend/Twilio notifications or general admin order operations yet.
- Keep Shiprocket behind an internal adapter.
- Make shipment creation retryable and tracking webhook handling idempotent.
- Include a polling fallback for stale shipment state as described in the design.
- Use placeholder Shiprocket config only; do not require live credentials.
- Use tests first and run the planned verification commands before completion.
- Create a commit for Session 4 only after tests pass.

Deliverables expected from this session:
- shipment creation orchestration
- shipment persistence with AWB/tracking metadata
- tracking event persistence
- customer tracking page with timeline
- secure lookup token flow

When you finish, summarize:
- what changed
- what verification passed
- the commit hash
- any operational gaps remaining for Session 5
