# Codex Session Prompts

This folder contains ready-to-use prompts for executing the commerce implementation plan in separate Codex sessions.

Source documents:
- [Commerce design](../plans/2026-05-05-orders-payments-fulfillment-design.md)
- [Commerce implementation plan](../plans/2026-05-05-orders-payments-fulfillment.md)

## How to use these prompts

1. Open a fresh Codex session in this repository.
2. Copy one prompt file from this folder.
3. Paste it as the first user message in the new session.
4. Run the prompts in numeric order.

## Prompt order

1. `01-persistence-and-checkout.md`
2. `02-razorpay-payments.md`
3. `03-inventory-admin.md`
4. `04-shiprocket-shipping-and-tracking.md`
5. `05-notifications-resend-twilio.md`
6. `06-admin-operations.md`
7. `07-runtime-hardening-and-smoke.md`

## Intent

Each prompt tells Codex to:
- use the approved implementation plan
- execute only the assigned session scope
- follow the repo's skill workflow
- verify changes before claiming completion
- commit only the work for that session
