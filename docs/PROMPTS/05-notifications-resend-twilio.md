Execute Session 5 of the commerce implementation plan in this repository.

Use `superpowers:executing-plans` and follow the approved plan exactly from:
- `docs/plans/2026-05-05-orders-payments-fulfillment.md`

Also read for context:
- `docs/plans/2026-05-05-orders-payments-fulfillment-design.md`

Session scope:
- Task 6: Add Resend Email and Twilio WhatsApp Notification Pipeline

Requirements:
- Stay within Session 5 only. Do not start general admin order operations or runtime hardening work.
- Keep notifications non-blocking for order progression.
- Persist notification attempts and make retries explicit.
- Support both email and WhatsApp adapter abstractions with placeholder config only.
- Do not require live Resend or Twilio credentials.
- Use tests first and run the planned verification commands before completion.
- Create a commit for Session 5 only after tests pass.

Deliverables expected from this session:
- notification pipeline and durable records
- Resend email adapter
- Twilio WhatsApp adapter
- retry handling for failed deliveries
- order confirmation page with tracking entry point

When you finish, summarize:
- what changed
- what verification passed
- the commit hash
- any contracts Session 6 must preserve
