Execute Session 2 of the commerce implementation plan in this repository.

Use `superpowers:executing-plans` and follow the approved plan exactly from:
- `docs/plans/2026-05-05-orders-payments-fulfillment.md`

Also read for context:
- `docs/plans/2026-05-05-orders-payments-fulfillment-design.md`

Session scope:
- Task 3: Add Razorpay Order Creation and Verified Payment Webhook Handling

Requirements:
- Stay within Session 2 only. Do not start inventory, shipping, notifications, or broader admin work.
- Build on top of completed Session 1 work rather than replacing it.
- Keep payment state authoritative on the backend.
- Store callback/webhook payloads durably and make handlers idempotent.
- Use placeholder config values only; do not require real Razorpay credentials.
- Use tests first, then minimal implementation, then verification as specified in the plan.
- Create a commit for Session 2 only after tests pass.

Deliverables expected from this session:
- Razorpay gateway abstraction
- create-payment flow
- verified callback and webhook handling
- duplicate webhook safety
- frontend payment handoff state

When you finish, summarize:
- what changed
- what verification passed
- the commit hash
- any assumptions that Session 3 must preserve
