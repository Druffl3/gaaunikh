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

## Iteration 2 Verification Commands

```powershell
# backend catalog tests (run manually)
dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj

# backend compile check (run manually)
dotnet build src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj

# frontend tests and static export build
cd src/frontend
npm test
npm run build
```

## Iteration 2 Smoke Checks

- `GET /api/catalog/products` returns seeded product list.
- `GET /api/catalog/products?search=chili` returns filtered products.
- `GET /api/catalog/products/kashmiri-chili-powder` returns product details with variants.
- `GET /shop` renders catalog controls and product cards.
- `GET /shop/product?slug=kashmiri-chili-powder` renders product detail with variants.

## Iteration 3 Verification Commands

```powershell
# frontend test and static export build
cd src/frontend
npm test
npm run build

# backend compile check
cd ..
dotnet build src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj

# backend API tests (requires NuGet feed access)
dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj
```

## Iteration 3 Smoke Checks

- `GET /shop/product?slug=kashmiri-chili-powder` shows variant-level "Add to Cart" actions.
- Clicking "Add to Cart" for a variant increases cart quantity and line totals.
- `GET /cart` renders cart line items with increment/decrement and remove actions.
- Cart subtotal updates accurately when quantity changes.
- Removing all lines returns the empty-cart state.
- Refreshing `/cart` in the same browser session preserves cart contents.
