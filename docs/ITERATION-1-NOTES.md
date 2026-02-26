# Iteration 1 Notes

## Local Verification Commands

```powershell
# frontend deps + test + static export build
cd src/frontend
npm install
npm test
npm run build
# verifies Next.js export output exists at src/frontend/out

# backend build
cd ../backend/Gaaunikh.Api
dotnet build

# unified container
cd ../../..
docker compose up --build
```

## Smoke Tests

- `GET http://localhost:8080/` returns the landing page.
- `GET http://localhost:8080/shop` returns the Shop page.
- `GET http://localhost:8080/track-order` returns the Track Order page.
- `GET http://localhost:8080/contact` returns the Contact page.
- `GET http://localhost:8080/api/health` returns 200 and JSON status.

## Current Limitation

- As of 2026-02-26, local Docker frontend build verification is still pending:
  - container build reaches frontend `npm run build`
  - `next` is not found inside the frontend build stage
  - app container smoke tests are blocked until this is resolved
