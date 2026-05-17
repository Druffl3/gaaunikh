# Admin Authentication and Inventory Authorization Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add authenticated admin sessions and role-based authorization so inventory management is no longer publicly accessible and every manual inventory change is attributable to an authenticated admin.

**Architecture:** Keep the storefront public and add a first-party admin auth slice inside the existing ASP.NET Core monolith. The backend becomes the source of truth for admin sessions and authorization, while the static Next.js admin pages use client-side session checks against same-origin APIs.

**Tech Stack:** ASP.NET Core 8 minimal APIs, C#, EF Core, PostgreSQL, ASP.NET Core cookie authentication/authorization, xUnit integration tests, Next.js 14 static export, React 18, TypeScript, Jest + Testing Library.

---

### Task 1: Add Admin Auth Domain, Configuration, and Database Support

**Files:**
- Create: `src/backend/Gaaunikh.Api/Configuration/AdminAuthOptions.cs`
- Create: `src/backend/Gaaunikh.Api/Data/Entities/AdminUser.cs`
- Modify: `src/backend/Gaaunikh.Api/Data/Entities/InventoryMovement.cs`
- Modify: `src/backend/Gaaunikh.Api/Data/CommerceDbContext.cs`
- Create: `src/backend/Gaaunikh.Api/Features/Auth/AdminRoles.cs`
- Create: `src/backend/Gaaunikh.Api/Features/Auth/AdminPolicies.cs`
- Create: `src/backend/Gaaunikh.Api/Features/Auth/AdminBootstrapService.cs`
- Modify: `src/backend/Gaaunikh.Api/appsettings.json`
- Create: `src/backend/Gaaunikh.Api/Data/Migrations/*AdminAuth*.cs`
- Create: `src/backend/Gaaunikh.Api.Tests/AdminAuthenticationTests.cs`

**Step 1: Write the failing test**

Add backend tests that assert:
- the app can resolve admin auth configuration
- the first admin user can be seeded from configuration when the admin table is empty
- `InventoryMovement` persists actor metadata fields for manual adjustments

Suggested assertions:

```csharp
Assert.True(await dbContext.AdminUsers.AnyAsync());
Assert.Equal("owner@gaaunikh.test", seededAdmin.Email);
Assert.NotNull(movement.ActorDisplayName);
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter AdminAuthenticationTests
```

Expected: FAIL because admin auth types, DB entities, and migration-backed fields do not exist.

**Step 3: Write minimal implementation**

Implement:

- `AdminAuthOptions` with seed and cookie/session settings
- `AdminUser` entity with email, display name, password hash, role, active flag, and timestamps
- `DbSet<AdminUser>` plus EF configuration for unique email index
- `InventoryMovement.ActorAdminUserId` and `InventoryMovement.ActorDisplayName`
- startup bootstrap service that seeds the first admin only when:
  - zero admin users exist
  - seed email and password are present in config
- placeholder `AdminAuth` section in `appsettings.json` with empty values
- EF migration for the new table and inventory movement columns

Representative implementation shape:

```csharp
public sealed class AdminUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = AdminRoles.Admin;
    public bool IsActive { get; set; }
}
```

**Step 4: Run test to verify it passes**

Run the same command.

Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api/Configuration/AdminAuthOptions.cs src/backend/Gaaunikh.Api/Data/Entities/AdminUser.cs src/backend/Gaaunikh.Api/Data/Entities/InventoryMovement.cs src/backend/Gaaunikh.Api/Data/CommerceDbContext.cs src/backend/Gaaunikh.Api/Features/Auth src/backend/Gaaunikh.Api/appsettings.json src/backend/Gaaunikh.Api/Data/Migrations src/backend/Gaaunikh.Api.Tests/AdminAuthenticationTests.cs
git commit -m "feat(auth): add admin auth domain and bootstrap support"
```

### Task 2: Add Cookie Login, Logout, Session, and Authorization Policies

**Files:**
- Create: `src/backend/Gaaunikh.Api/Features/Auth/AdminAuthService.cs`
- Create: `src/backend/Gaaunikh.Api/Features/Auth/AdminAuthContracts.cs`
- Modify: `src/backend/Gaaunikh.Api/Program.cs`
- Modify: `src/backend/Gaaunikh.Api.Tests/AdminAuthenticationTests.cs`

**Step 1: Write the failing test**

Extend backend tests to assert:
- valid credentials create an authenticated admin session
- invalid credentials return `401`
- `GET /api/admin/session` returns session details when logged in
- `POST /api/admin/auth/logout` clears the session cookie

Suggested flow:

```csharp
var loginResponse = await client.PostAsJsonAsync("/api/admin/auth/login", new
{
    email = "owner@gaaunikh.test",
    password = "Admin#12345"
});

Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter AdminAuthenticationTests
```

Expected: FAIL because auth endpoints and cookie auth are not wired.

**Step 3: Write minimal implementation**

Implement:

- cookie auth registration
- authorization policy registration
- `app.UseAuthentication()` and `app.UseAuthorization()`
- `POST /api/admin/auth/login`
- `POST /api/admin/auth/logout`
- `GET /api/admin/session`
- password verification through `PasswordHasher<AdminUser>`

Keep responses small and explicit:

```csharp
app.MapGet("/api/admin/session", (ClaimsPrincipal user) =>
{
    return user.Identity?.IsAuthenticated == true
        ? Results.Ok(new AdminSessionResponse(...))
        : Results.Unauthorized();
});
```

Important behavior:

- normalize email lookups to lowercase
- reject inactive admins
- emit `401` for bad credentials
- include role and display name in the session response

**Step 4: Run test to verify it passes**

Run the same command.

Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api/Features/Auth src/backend/Gaaunikh.Api/Program.cs src/backend/Gaaunikh.Api.Tests/AdminAuthenticationTests.cs
git commit -m "feat(auth): add admin login logout and session endpoints"
```

### Task 3: Protect Inventory APIs and Record Authenticated Actor Metadata

**Files:**
- Modify: `src/backend/Gaaunikh.Api/Features/Inventory/InventoryContracts.cs`
- Modify: `src/backend/Gaaunikh.Api/Features/Inventory/InventoryService.cs`
- Modify: `src/backend/Gaaunikh.Api/Program.cs`
- Modify: `src/backend/Gaaunikh.Api.Tests/InventoryTests.cs`
- Modify: `src/backend/Gaaunikh.Api.Tests/AdminAuthenticationTests.cs`

**Step 1: Write the failing test**

Add and update backend tests that assert:
- anonymous `GET /api/admin/inventory/summary` returns `401`
- authenticated non-inventory admin returns `403`
- authenticated `InventoryManager` can create inventory items and adjust stock
- manual adjustments persist `ActorAdminUserId` and `ActorDisplayName`

Suggested assertions:

```csharp
Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
Assert.Equal("Priya Ops", movement.ActorDisplayName);
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter "AdminAuthenticationTests|InventoryTests"
```

Expected: FAIL because inventory handlers are still public and do not capture actor identity.

**Step 3: Write minimal implementation**

Implement:

- authorization on inventory endpoints:
  - summary requires `AdminInventoryRead`
  - item creation requires `AdminInventoryWrite`
  - stock adjustment requires `AdminInventoryWrite`
- authenticated actor extraction from `ClaimsPrincipal`
- inventory service overloads or context parameters that accept actor metadata for manual mutations
- response DTO updates if movement history now exposes actor display name

Representative handler shape:

```csharp
app.MapPost("/api/admin/inventory/adjustments", async (
    StockAdjustmentRequest request,
    ClaimsPrincipal user,
    InventoryService inventoryService,
    CancellationToken cancellationToken) =>
{
    var actor = AdminActorContext.From(user);
    var item = await inventoryService.AdjustStockAsync(request, actor, cancellationToken);
    return Results.Ok(item);
}).RequireAuthorization(AdminPolicies.AdminInventoryWrite);
```

Keep automatic reservation movements unchanged except for leaving actor fields null.

**Step 4: Run test to verify it passes**

Run the same command.

Expected: PASS.

**Step 5: Commit**

```bash
git add src/backend/Gaaunikh.Api/Features/Inventory src/backend/Gaaunikh.Api/Program.cs src/backend/Gaaunikh.Api.Tests/InventoryTests.cs src/backend/Gaaunikh.Api.Tests/AdminAuthenticationTests.cs
git commit -m "feat(inventory): require admin authorization and audit actors"
```

### Task 4: Add Admin Login UI and Protect the Inventory Page

**Files:**
- Create: `src/frontend/components/admin/admin-login-form.tsx`
- Create: `src/frontend/components/admin/admin-login-form.test.tsx`
- Create: `src/frontend/components/admin/admin-session-gate.tsx`
- Create: `src/frontend/components/admin/admin-session-gate.test.tsx`
- Modify: `src/frontend/components/admin/inventory-page.tsx`
- Modify: `src/frontend/lib/api.ts`
- Create: `src/frontend/app/admin/login/page.tsx`
- Modify: `src/frontend/app/admin/inventory/page.tsx`
- Modify: `src/frontend/components/route-content.ts`
- Modify: `src/frontend/app/routes.test.tsx`

**Step 1: Write the failing test**

Add frontend tests that assert:
- `/admin/login` renders email/password fields and login button
- inventory route shows a sign-in gate when no session exists
- inventory page renders after a successful session check
- protected admin fetches send credentials so the browser can include the auth cookie

Suggested assertions:

```tsx
expect(screen.getByLabelText("Admin Email")).toBeInTheDocument();
expect(screen.getByText("Admin sign-in required.")).toBeInTheDocument();
```

**Step 2: Run test to verify it fails**

Run:

```bash
cd src/frontend
npm test -- components/admin/admin-login-form.test.tsx components/admin/admin-session-gate.test.tsx components/admin/inventory-page.test.tsx app/routes.test.tsx
```

Expected: FAIL because there is no login page, no session gate, and inventory still assumes public access.

**Step 3: Write minimal implementation**

Implement:

- login form posting to `/api/admin/auth/login`
- session gate that calls `/api/admin/session`
- inventory page fetches with `credentials: "include"`
- `/admin/login` page shell
- `/admin/inventory` route wrapped by the session gate

Recommended client behavior:

- show loading state while checking admin session
- show a sign-in-required panel with a link to `/admin/login` on `401`
- keep inventory fetch and mutation error messages distinct from auth failures

Representative fetch shape:

```ts
await fetch(resolveApiUrl("/api/admin/session"), {
  credentials: "include",
  cache: "no-store"
});
```

**Step 4: Run test to verify it passes**

Run the same command.

Expected: PASS.

**Step 5: Commit**

```bash
git add src/frontend/components/admin src/frontend/lib/api.ts src/frontend/app/admin/login/page.tsx src/frontend/app/admin/inventory/page.tsx src/frontend/components/route-content.ts src/frontend/app/routes.test.tsx
git commit -m "feat(frontend): add admin login and inventory session gate"
```

### Task 5: Run End-to-End Verification and Document Bootstrap Expectations

**Files:**
- Modify: `docs/PROMPTS/README.md`
- Modify: `docs/PLAN.md`
- Modify: `docs/ITERATION-1-NOTES.md`

**Step 1: Write the verification checklist**

Capture the exact expected outcomes for:
- anonymous admin inventory access blocked
- valid admin login succeeds
- inventory manager can load, create, and adjust inventory
- actor attribution appears in recent movement history or persisted records

**Step 2: Run verification**

Run:

```bash
dotnet test src/backend/Gaaunikh.Api.Tests/Gaaunikh.Api.Tests.csproj --filter "AdminAuthenticationTests|InventoryTests"
cd src/frontend
npm test -- components/admin/admin-login-form.test.tsx components/admin/admin-session-gate.test.tsx components/admin/inventory-page.test.tsx app/routes.test.tsx
npm run build
cd ..\..
dotnet build src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj
```

Expected:

- backend auth and inventory tests pass
- frontend auth and route tests pass
- frontend production build passes
- backend build passes

**Step 3: Write minimal documentation updates**

Document:

- required `AdminAuth__SeedEmail` and `AdminAuth__SeedPassword` configuration for first login
- the fact that admin inventory is now protected
- the recommended order for future admin-order work to reuse this auth foundation

**Step 4: Re-run any affected verification**

Run the same verification commands if docs or build wiring changed in a way that could affect outputs.

Expected: PASS.

**Step 5: Commit**

```bash
git add docs/PROMPTS/README.md docs/PLAN.md docs/ITERATION-1-NOTES.md
git commit -m "docs(auth): document admin auth bootstrap and verification"
```

## Notes for Execution

- Do not add customer login or storefront auth in this slice.
- Do not store bearer tokens in local storage; keep admin auth server-issued and cookie-backed.
- Keep admin roles simple for now: `Admin` and `InventoryManager` are enough.
- Preserve backward compatibility for public catalog, cart, checkout, and payment routes.
- If local frontend development against a separate backend origin needs extra support, prefer explicit `credentials: "include"` and narrowly scoped backend configuration rather than weakening cookie settings globally.
