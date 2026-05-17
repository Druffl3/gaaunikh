Execute the admin authentication and inventory authorization implementation plan in this repository.

Use `superpowers:executing-plans` and follow the approved plan exactly from:
- `docs/plans/2026-05-07-admin-auth-inventory-security.md`

Also read for context:
- `docs/plans/2026-05-07-admin-auth-inventory-security-design.md`

Scope:
- Protect admin inventory management with authentication and authorization
- Keep the public storefront anonymous
- Add audit attribution for manual inventory actions

Requirements:
- Stay within this plan only. Do not start customer accounts, storefront auth, or unrelated runtime hardening work.
- Do not use git worktrees in this repository.
- Create and use a regular git branch in the current workspace if you need a scoped branch.
- Use tests first and run the planned verification commands before completion.
- Keep admin auth same-origin and backend-authoritative.
- Do not store admin credentials or tokens in browser local storage.
- Do not commit real seed credentials or secrets.
- Create commits only after the relevant tests pass.

Deliverables expected from this session:
- backend admin auth domain and bootstrap support
- login, logout, and current-session endpoints
- authorization enforced on inventory admin APIs
- authenticated actor attribution for manual inventory mutations
- admin login page and protected inventory route behavior

When you finish, summarize:
- what changed
- what verification passed
- the commit hash or hashes
- any follow-up work needed before expanding admin order operations
- any deployment/config changes required for the first admin login
