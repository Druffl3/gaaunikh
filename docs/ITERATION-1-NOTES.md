# Iteration 1 Notes

## Local Verification Commands

```powershell
# frontend deps + test + build
cd src/frontend
npm install
npm test
npm run build

# backend build
cd ../backend/Gaaunikh.Api
dotnet build

# unified container
cd ../../..
docker compose up --build
```

## Smoke Tests

- `GET http://localhost:8080/` returns the landing page.
- `GET http://localhost:8080/shop` returns landing app route (SPA fallback).
- `GET http://localhost:8080/track-order` returns landing app route (SPA fallback).
- `GET http://localhost:8080/contact` returns landing app route (SPA fallback).
- `GET http://localhost:8080/api/health` returns 200 and JSON status.
