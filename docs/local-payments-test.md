# Local Payment Testing

As of May 6, 2026.

## Current Scope

Session 2 adds:
- order checkout persistence
- payment attempt creation
- Razorpay-style callback signature verification
- Razorpay-style webhook signature verification
- duplicate webhook idempotency
- frontend payment handoff state

This implementation does **not** yet open the real Razorpay Checkout widget or call Razorpay's live API. The current `RazorpayGateway` returns a placeholder order ID locally, so you cannot lose money while testing this session.

The authoritative state transitions are:
- `PendingPayment` after checkout
- `PaymentAuthorized` after a verified callback
- `Paid` after a verified `payment.captured` webhook

## Local Runtime

Start the stack from the repository root:

```powershell
$env:Payments__Razorpay__KeyId = "rzp_test_placeholder"
$env:Payments__Razorpay__KeySecret = "test_key_secret"
$env:Payments__Razorpay__WebhookSecret = "test_webhook_secret"
docker compose up --build app postgres adminer
```

Local endpoints:
- app: `http://localhost:8080`
- postgres: `localhost:5433`
- Adminer: `http://localhost:8081`

PostgreSQL connection values:
- server: `postgres` inside Compose, `localhost` from the host
- database: `gaaunikh`
- username: `gaaunikh`
- password: `gaaunikh`

## Basic UI Flow

1. Open `http://localhost:8080/shop/`.
2. Add a product to the cart.
3. Open `http://localhost:8080/checkout/`.
4. Fill the checkout form and submit it.
5. Confirm the page shows `Order reference ... created successfully.`
6. Click `Continue to Payment`.
7. Confirm the page shows:
   - `Razorpay order ... created.`
   - `Awaiting verified backend confirmation before marking this order paid.`

Expected database state after step 7:
- one `orders` row with `status = PendingPayment`
- one `payment_attempts` row with `status = Created`

## Inspecting Database State

You can use Adminer or `psql`.

Example host-side `psql` connection:

```powershell
psql "host=localhost port=5433 dbname=gaaunikh user=gaaunikh password=gaaunikh"
```

Useful queries:

```sql
select id, reference, status, total_inr, created_utc
from orders
order by created_utc desc;

select id, order_id, provider, status, provider_order_id, provider_payment_id, amount_inr, created_utc
from payment_attempts
order by created_utc desc;

select id, provider, event_type, external_event_id, processed, received_utc
from provider_callback_logs
order by received_utc desc;
```

## Simulating Verified Callback

After checkout and create-payment, read the latest `provider_order_id` from `payment_attempts`.

Set values:
- `orderId`: the `provider_order_id` from `payment_attempts`
- `paymentId`: any test payment ID, for example `pay_local_001`
- `keySecret`: the same value used in startup, for example `test_key_secret`

Generate the signature and send the callback:

```powershell
$orderId = "order_replace_me"
$paymentId = "pay_local_001"
$keySecret = "test_key_secret"
$message = "$orderId|$paymentId"

$hmac = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($keySecret))
$signatureBytes = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($message))
$signature = ([System.BitConverter]::ToString($signatureBytes)).Replace("-", "").ToLower()

Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8080/api/payments/razorpay/callback" `
  -ContentType "application/json" `
  -Body "{`"razorpayOrderId`":`"$orderId`",`"razorpayPaymentId`":`"$paymentId`",`"razorpaySignature`":`"$signature`"}"
```

Expected result:
- HTTP 200
- `orders.status = PaymentAuthorized`
- `payment_attempts.status = Authorized`
- one `provider_callback_logs` row with `event_type = payment.callback` and `processed = true`

To verify signature rejection, send the same payload with a different signature. Expected result:
- HTTP 400
- order and payment state unchanged
- callback log still stored with `processed = false`

## Simulating Verified Webhook

Use the same `provider_order_id` and a raw JSON payload. The webhook signature is computed from the exact raw request body using the webhook secret.

```powershell
$payload = '{"event":"payment.captured","payload":{"payment":{"entity":{"id":"pay_local_001","order_id":"order_replace_me","status":"captured"}}}}'
$webhookSecret = "test_webhook_secret"

$hmac = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($webhookSecret))
$signatureBytes = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($payload))
$signature = ([System.BitConverter]::ToString($signatureBytes)).Replace("-", "").ToLower()

Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8080/api/payments/razorpay/webhook" `
  -Headers @{ "X-Razorpay-Signature" = $signature } `
  -ContentType "application/json" `
  -Body $payload
```

Expected result:
- HTTP 200
- `orders.status = Paid`
- `payment_attempts.status = Captured`
- one `provider_callback_logs` row with `event_type = payment.captured` and `processed = true`

To verify signature rejection, send the same payload with a different `X-Razorpay-Signature`. Expected result:
- HTTP 400
- order and payment state unchanged
- callback log still stored with `processed = false`

## Verifying Duplicate Webhook Safety

Send the exact same verified webhook a second time.

Expected result:
- HTTP 200
- order remains `Paid`
- payment attempt remains `Captured`
- a second callback log row is stored
- no duplicate payment state transition occurs

This session treats duplicate safety as a processing rule, not a payload drop rule. The duplicate request is still recorded durably.

## What Real Razorpay Testing Will Look Like Later

When the app is wired to the real Razorpay API and Checkout widget, use Razorpay **Test Mode** only. Razorpay's test mode uses separate test keys and does not charge real money. Test cards and test UPI IDs can be used there, and webhook delivery will require a public URL rather than plain `localhost`.

Official references:
- test/live modes: `https://razorpay.com/docs/payments/dashboard/test-live-modes/`
- test cards: `https://razorpay.com/docs/payments/payments/test-card-details/`
- test UPI IDs: `https://razorpay.com/docs/payments/payments/test-upi-details/`
- webhook validation/testing: `https://razorpay.com/docs/webhooks/validate-test/`

## Related Files

- `src/backend/Gaaunikh.Api/Program.cs`
- `src/backend/Gaaunikh.Api/Features/Payments/PaymentService.cs`
- `src/backend/Gaaunikh.Api/Infrastructure/Payments/RazorpayGateway.cs`
- `src/frontend/components/payment-step.tsx`
- `src/backend/Gaaunikh.Api.Tests/RazorpayPaymentTests.cs`
