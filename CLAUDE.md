# JEWELRY FACTORY MANAGEMENT SYSTEM — PROJECT CONTEXT

You are a senior full-stack developer working on a comprehensive Jewelry Factory Management System. Use this context for ALL code generation, design decisions, and recommendations throughout this project.

---

## 1. PROJECT IDENTITY

**Project Name:** JewelryFactory ERP
**Type:** Internal B2B Manufacturing & Distribution ERP
**Domain:** Fine Jewelry Manufacturing (gold, silver, diamonds, gemstones)
**Users:** Production workers, designers, QC staff, sales/CS team, management, admins
**Geography:** Thailand-based factory; international B2B customers (AU, NZ, Asia, EU, US, UK)

---

## 2. BUSINESS CONTEXT

The factory operates the full jewelry production lifecycle:
**Design → Sample → Order → Production → QC → Packaging → Shipping**

Key characteristics:
- B2B (NOT retail POS) — customers are wholesalers, retailers, catalogue publishers
- Manufactures fine jewelry from raw materials (gold, silver, gems, diamonds)
- Tracks individual high-value items AND batch/lot for small parts
- Customers may consign their own gold ("gold loan account")
- Loss/scrap of precious metals MUST be tracked at every stage
- Multi-currency invoicing (THB, USD, EUR)
- Subject to Thai tax rules (VAT 7%, Withholding Tax)

---

## 3. TECH STACK

**Backend**
- ASP.NET Core 9 Web API (C# 13)
- Entity Framework Core 9 (primary ORM)
- Dapper (for complex reports / read-heavy queries)
- FluentValidation (request validation)
- Mapster (DTO ↔ Entity mapping; chosen over AutoMapper due to security advisory + better performance)
- Serilog (structured logging)
- MediatR (CQRS pattern)

**Frontend**
- Angular 17+ (LTS)
- TypeScript (strict mode)
- Angular Material OR PrimeNG (component library — to be chosen)
- ag-Grid (for large data tables — inventory, production lists)
- RxJS (state + async)
- NgRx (if state grows complex; defer to Phase 2)

**Database**
- Azure SQL Database (SQL Server compatible)
- Code-First migrations via EF Core

**Cloud / Infrastructure**
- Azure App Service (Web API hosting)
- Azure Static Web Apps (Angular hosting)
- Azure Blob Storage (CAD files, product photos, certificates)
- Azure Key Vault (secrets, connection strings)
- Application Insights (monitoring)

**DevOps**
- Git + GitHub (or Azure DevOps)
- GitFlow branching: main / develop / feature/* / hotfix/*
- Conventional Commits (feat:, fix:, docs:, refactor:, test:, chore:)
- CI/CD via GitHub Actions or Azure Pipelines

**Testing**
- xUnit + Moq + FluentAssertions (Backend unit tests)
- Jasmine + Karma (Angular unit tests)
- Postman / REST Client (API testing)

---

## 4. ARCHITECTURE — CLEAN ARCHITECTURE

Follow strict Clean Architecture with these layers:

```
┌──────────────────────────────────────────┐
│  JewelryFactory.Api  (Controllers)       │ ← HTTP entry point
├──────────────────────────────────────────┤
│  JewelryFactory.Application              │ ← Use cases, DTOs, Validators, MediatR handlers
├──────────────────────────────────────────┤
│  JewelryFactory.Domain                   │ ← Entities, Value Objects, Domain Events
├──────────────────────────────────────────┤
│  JewelryFactory.Infrastructure           │ ← EF Core, Dapper, External services, Blob storage
└──────────────────────────────────────────┘
```

**Dependency Rule:** Dependencies point INWARD only. Domain depends on nothing. Application depends only on Domain. Infrastructure & Api depend on Application/Domain.

**Folder structure example:**
```
src/
├── JewelryFactory.Api/
│   ├── Controllers/
│   ├── Middleware/
│   └── Program.cs
├── JewelryFactory.Application/
│   ├── Common/ (Interfaces, Behaviors)
│   ├── Features/
│   │   ├── Inventory/
│   │   │   ├── Commands/  (CreateItem, UpdateStock, ...)
│   │   │   ├── Queries/   (GetItems, GetItemById, ...)
│   │   │   └── DTOs/
│   │   ├── Orders/
│   │   └── Production/
├── JewelryFactory.Domain/
│   ├── Entities/
│   ├── ValueObjects/    (Money, Weight, Karat, StoneCertificate)
│   ├── Enums/
│   └── Common/          (BaseEntity, AuditableEntity)
├── JewelryFactory.Infrastructure/
│   ├── Persistence/     (DbContext, Configurations, Migrations)
│   ├── Repositories/
│   ├── Services/        (BlobStorage, EmailService, GoldPriceService)
│   └── DependencyInjection.cs
tests/
├── JewelryFactory.UnitTests/
└── JewelryFactory.IntegrationTests/
```

**Pattern:** CQRS via MediatR — separate Command (write) and Query (read) handlers.

---

## 5. MODULE STRUCTURE

The system has 17 modules. Build in phase order — DO NOT jump ahead.

### Phase 1 — MVP (Core Operations)
1. **User & Auth** — JWT, Role-based, Audit Trail
2. **Master Data** — Customer, Supplier, Worker, Material types
3. **Inventory** — Raw Material + Finished Goods, Stone individual tracking
4. **Sales Order** — B2B order entry, status tracking
5. **Production Tracking** — Work Order, stage workflow, time tracking
6. **Basic Reports** — Inventory level, order status, production progress

### Phase 2 — Advanced Operations
7. **BOM & Costing** — Bill of Materials, cost calculation
8. **Quality Control** — Incoming/In-process/Final QC, defect tracking
9. **Gold/Metal Account** — Customer consigned gold tracking (in pure metal weight)
10. **Loss / Scrap / Refining** — Precious metal yield reconciliation
11. **Shipping & Invoice** — Packing list, commercial invoice, multi-currency
12. **Dashboard** — KPIs, production performance, sales analytics

### Phase 3 — Enhancements
13. **Traceability / Provenance** — Material origin tracking, optional blockchain
14. **Shop Floor Mobile App** — Tablet-friendly worker UI (PWA)
15. **Advanced BI** — Custom reports, data export
16. **HR / Piece-rate Wages** — Worker productivity, pay calculation
17. **Equipment / Maintenance** — Asset register, maintenance schedule

---

## 6. CODING STANDARDS

**General**
- C# 12 features (primary constructors, collection expressions, etc.) where appropriate
- Async/await ALWAYS — never block with `.Result` or `.Wait()`
- Method names: `GetProductByIdAsync`, `CreateOrderAsync` (suffix `Async`)
- Use nullable reference types (`#nullable enable`)

**API / Controllers**
- Controllers are THIN — only orchestrate, delegate to MediatR
- Use `[ApiController]`, `[Route("api/[controller]")]`
- Return `IActionResult` with proper status codes
- NEVER expose Entity directly — always use DTO

**DTO & Validation**
- Separate DTOs: `CreateXxxDto`, `UpdateXxxDto`, `XxxResponseDto`
- Validate via FluentValidation, NOT data annotations
- Use AutoMapper profiles per feature

**Error Handling**
- Global exception middleware → standardized error response
- Custom exceptions: `NotFoundException`, `ValidationException`, `BusinessRuleException`
- Never swallow exceptions silently

**Logging**
- Use `ILogger<T>` everywhere; structured logging via Serilog
- Log entry/exit of important operations with correlation ID
- NEVER log sensitive data (passwords, tokens, full payment details)

**Dependency Injection**
- Constructor injection only (no service locator)
- Service lifetime defaults: Repositories = Scoped, Services = Scoped, Singletons only when truly stateless

---

## 7. DATABASE CONVENTIONS

**Naming**
- Tables: PascalCase, plural — `Products`, `WorkOrders`, `StoneItems`
- Columns: PascalCase — `Id`, `CreatedAt`, `GoldWeightGrams`
- Foreign keys: `<EntityName>Id` — `CustomerId`, `OrderId`
- Indexes: `IX_<Table>_<Columns>` — `IX_Orders_CustomerId_Status`

**Required base columns on every business entity (AuditableEntity):**
- `Id` (int or Guid — choose Guid for entities synced across systems)
- `CreatedAt` (datetime, UTC)
- `CreatedBy` (string — user ID)
- `UpdatedAt` (datetime, UTC, nullable)
- `UpdatedBy` (string, nullable)
- `IsDeleted` (bool, soft delete)

**Migrations**
- One migration per logical change — descriptive names: `AddPriceColumnToProducts`
- NEVER edit applied migrations — create a new one

**Decimal precision**
- Money: `decimal(18, 2)`
- Weight (grams): `decimal(10, 4)` — jewelry needs 4 decimal places
- Carat: `decimal(8, 4)`
- Gold purity (%): `decimal(5, 4)`

---

## 8. API CONVENTIONS — REST

**URL pattern**
```
GET    /api/v1/products              List
GET    /api/v1/products/{id}         Get one
POST   /api/v1/products              Create
PUT    /api/v1/products/{id}         Replace
PATCH  /api/v1/products/{id}         Partial update
DELETE /api/v1/products/{id}         Delete
```

**Status codes**
- 200 OK (GET/PUT successful)
- 201 Created (POST — return Location header)
- 204 No Content (DELETE successful)
- 400 Bad Request (validation failed)
- 401 Unauthorized (no/invalid token)
- 403 Forbidden (no permission)
- 404 Not Found
- 409 Conflict (business rule violation)
- 500 Internal Server Error

**Standard response envelope**
```json
{
  "success": true,
  "data": { ... },
  "errors": null,
  "timestamp": "2026-05-06T10:00:00Z"
}
```

**Error response**
```json
{
  "success": false,
  "data": null,
  "errors": [
    { "field": "Email", "message": "Email is required" }
  ],
  "timestamp": "2026-05-06T10:00:00Z"
}
```

**Pagination**
```
GET /api/v1/products?page=1&pageSize=20&sort=name&order=asc
Response: { items: [], totalCount, page, pageSize, totalPages }
```

**API Versioning** — URL-based (`/api/v1/...`).

---

## 9. SECURITY

- JWT Bearer Authentication (access token 15 min + refresh token 7 days)
- Refresh tokens stored hashed in DB
- Passwords hashed with BCrypt (work factor 12)
- Role-based authorization: `Admin`, `Manager`, `Sales`, `Production`, `QC`, `Designer`, `Worker`
- Resource-based authorization where needed (worker can only see own jobs)
- All sensitive endpoints require `[Authorize]`
- HTTPS only
- CORS — whitelist Angular origin only
- Audit log for ALL data modifications
- Rate limiting on auth endpoints

---

## 10. BUSINESS RULES — JEWELRY-SPECIFIC

These are CRITICAL domain rules. NEVER violate them:

1. **Gold weight = ALWAYS in grams with 4 decimal places**
   Track both gross weight (with stones) and net weight (metal only).

2. **Gold purity tracked as karat AND as decimal fraction**
   24k = 1.0000 (pure), 18k = 0.7500, 14k = 0.5833, 9k = 0.3750.

3. **Pure metal accounting**
   For consigned gold, always convert to "pure gold equivalent":
   `pure_grams = gross_grams × purity_fraction`

4. **Stone tracking**
   - Stones ≥ 0.20 carat OR with certificate → individual tracking (unique ID)
   - Stones < 0.20 carat → parcel tracking (lot + count + total carat weight)

5. **Production loss tolerance**
   Each production stage has a max acceptable loss %. Exceeding triggers a flag.

6. **Stage-by-stage weight reconciliation**
   At each production stage, record IN weight and OUT weight. Difference = loss. Must reconcile at job close.

7. **BOM (Bill of Materials)**
   Every finished design has a BOM listing materials, quantities, expected loss %. Cost = sum of material costs + labor cost + overhead.

8. **Daily gold price**
   Cache daily gold price (THB/baht weight, USD/oz). All cost calculations reference the order date's price.

9. **Order status flow** — strict transitions only:
   `Draft → Confirmed → InProduction → QC → Packed → Shipped → Delivered`
   (or → Cancelled from any state with reason)

10. **Soft delete only** — never hard-delete business records (legal/audit reasons).

---

## 11. ANGULAR FRONTEND CONVENTIONS

**Project structure (feature-based)**
```
src/app/
├── core/         (singletons: AuthService, HttpInterceptor, Guards)
├── shared/       (shared components, pipes, directives)
├── features/
│   ├── inventory/
│   ├── orders/
│   └── production/
├── layouts/      (MainLayout, AuthLayout)
└── app-routing.module.ts
```

- Use **standalone components** (Angular 17+ default)
- Use **signal-based** state where possible
- HTTP via interceptor that attaches JWT and handles 401 (refresh token)
- Lazy-load every feature module
- Forms: Reactive Forms only (no Template-driven)
- All API responses typed via interfaces matching backend DTOs
- Generate TypeScript interfaces from backend Swagger spec when possible

---

## 12. WORKING PRINCIPLES

When generating code or recommendations:

1. **Always state which Phase a feature belongs to.** Don't add Phase 2 features in Phase 1 code.
2. **Always start from the Domain entity, then Application use case, then Infrastructure, then API.** Never start from the controller.
3. **Always include FluentValidation rules and unit tests for new use cases.**
4. **Always reference business rules from Section 10** when building jewelry-specific logic.
5. **If unsure about a business rule, STOP and ASK** — do not assume.
6. **Suggest indexes** when creating new tables that will be queried often.
7. **Consider concurrency** — gold/stone inventory must use optimistic concurrency (RowVersion) to prevent double-deduction.
8. **Default language for code/comments: English. Default UI language: Thai + English bilingual where appropriate.**

---

END OF PROJECT CONTEXT
