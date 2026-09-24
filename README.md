# Library Rental API

Library book rental built with the same approach as the bootcamp project:
**PRD → Backlog → Clean Architecture → CQRS/MediatR → Swagger → Hangfire**.

| Day | Where it lives |
|---|---|
| 1 — PRD | `docs/01-PRD.md` |
| 2 — Delivery plan | `docs/02-Backlog.md` |
| 3 — Clean Architecture + Identity | `LibraryRental.Domain / Application / Infrastructure / API`, JWT in `Infrastructure/Identity` |
| 4 — CQRS + MediatR + Swagger | `Application/*/Commands`, `Application/*/Queries`, XML comments + `ProducesResponseType` on controllers |
| 5 — Hangfire + debugging | `INotificationService` (fire-and-forget), `RentalMaintenanceService` (recurring), Dashboard at `/hangfire` |

## Layers (dependencies point inward)
```
API  ──►  Application  ──►  Domain
 │             ▲
 └► Infrastructure ┘   (implements Application's interfaces; API references it only to wire DI)
```

## Run it
Requirements: .NET 9 SDK, SQL Server (LocalDB is fine).

```bash
dotnet tool install --global dotnet-ef

# 1) Create the first migration (only once)
dotnet ef migrations add InitialCreate -p LibraryRental.Infrastructure -s LibraryRental.API -o Persistence/Migrations

# 2) Run (migrations are applied and roles + admin are seeded on startup)
dotnet run --project LibraryRental.API
```
- Swagger: http://localhost:5090/swagger
- Hangfire Dashboard: http://localhost:5090/hangfire
- Seeded admin: `admin@library.local` / `Admin#12345` (change it in `appsettings.json`, and change `Jwt:Key`).

## Try the whole flow in Swagger
1. `POST /api/auth/register` → a librarian (`role: "Librarian"`, `department`). Copy the token → **Authorize**.
2. `POST /api/librarians/me/copies` → a book copy, e.g. `{ "title": "Clean Code", "author": "Robert C. Martin", "isbn": "9780132350884" }`.
3. Register a second user as `Member`, authorize with that token.
4. `GET /api/librarians` → `GET /api/librarians/{id}/copies` → `POST /api/rentals`.
5. Open `/hangfire` → the `NotifyLibrarianOfNewReservation` job ran (the "email" is written to the log).
6. Reserve the same copy again → 400. Cancel with `DELETE /api/rentals/{id}` → 204 (only while it is still `Reserved`).
7. Librarian: `PUT /api/rentals/{id}/status` with `{ "status": "CheckedOut" }`, then again with `{ "status": "Returned" }`.
8. Admin: `PUT /api/librarians/{id}/deactivate`, then try reserving from that librarian's catalog → 400.
9. Dashboard → **Recurring jobs** → `mark-overdue-rentals`, then **Trigger** it.

## Design notes
- **Business rules live in the Application layer** (handlers), not in controllers.
- **Race-safe reservation:** `BookCopy.RowVersion` (optimistic concurrency) + a filtered unique index on `Rental.BookCopyId` → the second member gets **409**.
- **Notifications never block the request:** handlers call `BackgroundJob.Enqueue<INotificationService>(...)` after saving. `EmailNotificationService` only logs the email for now; swap `Send` for SMTP / SendGrid.
- `Application` references `Hangfire.Core` to enqueue jobs directly, exactly like the bootcamp slides. A stricter variant would hide it behind an `IJobScheduler` interface.
- Registration creates the login user first, then the Member/Librarian row. If the second step fails you'd have an orphan login; a production version should wrap both in a compensation step.
- The loan period is a fixed 14 days from checkout, set on `Rental.DueDate` when a librarian marks it `CheckedOut`.

## Ideas to extend (mirrors the take-home tasks)
- Reactivate librarian, a shared book catalog (titles independent of a specific librarian's stock), late fees actually charged, waitlists for popular titles.
- Real email via MailKit, Dashboard authorization filter, integration tests with Testcontainers.
