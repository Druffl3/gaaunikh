# Gaaunikh Masala - Iteration 1 Plan

As of February 25, 2026.

## 1. Iteration Objective
Ship a production-deployable **Brand Landing MVP** where:
- the backend serves the built frontend assets,
- the app runs as a single Docker image,
- the landing page is live with basic navigation.

## 2. Business Outcome
- Gaaunikh Masala has a public branded website.
- Technical foundation is ready for later ecommerce iterations without changing deployment model.

## 3. Scope
### In Scope
- React + Vite landing page implementation
- .NET static file hosting from `wwwroot`
- SPA fallback route for client-side navigation
- API health endpoint (`/api/health`)
- Single multi-stage Docker build (frontend build + backend runtime image)
- Compose wiring for local run with one app container

### Out of Scope
- Product catalog and product APIs
- Authentication and admin features
- Checkout, payments, inventory, and tracking workflows

## 4. Architecture Plan (Iteration 1)
### Runtime Shape
- One app container exposes frontend pages and backend API from same origin.
- Postgres and Redis can exist in Compose but are not required for landing page render path.

### Request Flow
1. Browser requests `/` or frontend route.
2. Backend serves static assets from `wwwroot`.
3. Unknown non-API routes fallback to `index.html`.
4. `/api/health` returns service health response.

## 5. Implementation Workstreams
### A. Frontend Landing UI
- Build hero section, brand copy, and primary CTA.
- Add navigation links: Home, Shop, Track Order, Contact.
- Ensure mobile-first responsiveness.

### B. Backend Static Hosting
- Configure static files middleware.
- Configure SPA fallback for non-API routes.
- Add `/api/health` endpoint for runtime checks.

### C. Containerization
- Create/adjust multi-stage Dockerfile:
  - Stage 1: build frontend assets.
  - Stage 2: publish backend.
  - Stage 3: copy published backend + frontend dist into final runtime image.
- Ensure container starts with backend entrypoint and serves both API and UI.

### D. Local Verification
- Compose up and validate:
  - `GET /` serves landing page.
  - frontend navigation works on refresh.
  - `GET /api/health` returns 200.

## 6. Acceptance Criteria
- Single Docker image contains both backend and frontend artifacts.
- Landing page is accessible via backend container endpoint.
- Direct navigation to frontend routes does not 404 (SPA fallback works).
- `/api/health` is reachable and returns healthy response.
- Page is usable on desktop and mobile widths.

## 7. Deliverables
- Updated frontend landing page code.
- Backend static hosting + fallback configuration.
- Dockerfile and Compose updates for unified image flow.
- Iteration notes: commands used for local verify and deploy.

## 8. Execution Plan (2-3 Days)
### Day 1
- Set up backend static hosting + health endpoint.
- Implement landing page structure and navigation.

### Day 2
- Complete responsive styling and content polish.
- Build unified Docker image and validate local Compose flow.

### Day 3 (Buffer)
- Fix defects, finalize docs, and perform deployment dry run.

## 9. Risks and Mitigations
- Risk: SPA deep links return 404 in production.
  - Mitigation: add explicit backend fallback rule and test with direct URL refresh.
- Risk: static assets not copied into final runtime image.
  - Mitigation: add deterministic Docker copy paths and verify image contents at build time.
- Risk: mixed-origin assumptions in frontend API calls.
  - Mitigation: use same-origin API base paths (relative `/api/...`).

## 10. Definition of Done
- All acceptance criteria pass locally in Docker Compose.
- Documented run and smoke test steps are committed.
- Iteration 1 can be deployed as one app image to VM without architecture changes.
