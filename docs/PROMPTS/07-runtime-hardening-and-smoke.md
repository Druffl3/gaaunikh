Execute Session 7 of the commerce implementation plan in this repository.

Use `superpowers:executing-plans` and follow the approved plan exactly from:
- `docs/plans/2026-05-05-orders-payments-fulfillment.md`

Also read for context:
- `docs/plans/2026-05-05-orders-payments-fulfillment-design.md`

Session scope:
- Task 8: Wire Single-Image Runtime, Provider Stub Config, and Operational Docs

Requirements:
- Stay within Session 7 only and focus on runtime wiring, configuration polish, docs, and verification.
- Preserve the unified Docker image model where the backend serves the frontend.
- Ensure the app can boot with placeholder provider settings and without live external credentials.
- Update runtime docs, smoke checklist, and relevant plan notes as described in the implementation plan.
- Run the full verification sequence from the plan before completion.
- Create a commit for Session 7 only after verification passes.

Deliverables expected from this session:
- final runtime wiring for the single-image container flow
- placeholder environment variable and config guidance
- runtime and smoke-check documentation
- end-to-end smoke verification evidence

When you finish, summarize:
- what changed
- what verification passed
- the commit hash
- any final deployment prerequisites for supplying real provider credentials
