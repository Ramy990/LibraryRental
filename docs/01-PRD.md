# Library Book Rental System — PRD (Day 1)

## BRD — why the business needs this
**Problem:** The library tracks who has which book on a paper card index. Copies get "lost" between shelves, two members are sometimes told they can borrow the same copy, and nobody knows what is overdue until a member complains.
**Goal:** A system where members reserve an available copy of a book and track their rentals, and librarians manage their catalog and process pickups/returns.

## Scenario
Librarians add **Book Copies** to their catalog → Members reserve a copy → a **Rental** is created.

Rental status flow:

`Reserved → CheckedOut → Returned`  
`Reserved → Cancelled` (by the member)  
`CheckedOut → Overdue` (by the system, once the due date has passed)

Actors: **Member**, **Librarian**, **Admin**, **System** (background jobs).

---

## Data Model
| Entity | Fields |
|---|---|
| Member | Id, UserId, FullName, PhoneNumber, CreatedAt |
| Librarian | Id, UserId, FullName, Department, IsActive, DeactivatedAt, CreatedAt |
| BookCopy | Id, LibrarianId, Title, Author, ISBN, IsRented, RowVersion |
| Rental | Id, MemberId, LibrarianId, BookCopyId, Status, Notes, ReservedAt, StatusUpdatedAt, CancelledAt, DueDate, ReturnedAt |

Relationships: Librarian 1—m BookCopy, Member 1—m Rental, Librarian 1—m Rental, BookCopy 1—m Rental (only one *active* rental per copy at a time).

**Why is Rental its own entity?** It carries its own data (Status, Notes, DueDate, ReturnedAt), so it is not just a link table.

---

## Feature 1 — Register & Login
1. **Business problem:** Nobody can be identified, so ownership rules (my rentals, my catalog) cannot be enforced.
2. **Feature:** Register as Member or Librarian, login, receive a JWT.
3. **Actors:** Member, Librarian, Admin (seeded).
4. **Business rules**
   - BR-A1: Role at registration is `Member` or `Librarian`. `Admin` is never self-registered.
   - BR-A2: Email is unique.
   - BR-A3: A Librarian must supply a Department.
5. **Data:** Identity user + Member or Librarian row.
6. **Endpoints:** `POST /api/auth/register`, `POST /api/auth/login`
7. **Acceptance criteria**
   - Given a new email, when a member registers, then a user and a Member record exist and a token is returned.
   - Given an email already used, when registering, then the API rejects with 400.
   - Given wrong password, when logging in, then the API returns 401.

## Feature 2 — Manage Catalog
1. **Business problem:** Members cannot see which books are available to borrow.
2. **Feature:** Librarian adds book copies to their catalog.
3. **Actor:** Librarian
4. **Business rules**
   - BR-S1: Only a Librarian can add copies, and only to their own catalog.
   - BR-S2: A copy must have a Title, an Author, and a valid 10 or 13 digit ISBN.
5. **Data:** BookCopy
6. **Endpoints:** `POST /api/librarians/me/copies`, `GET /api/librarians/{id}/copies`
7. **Acceptance criteria**
   - Given a valid title/author/ISBN, when a librarian adds a copy, then it exists with `IsRented = false`.
   - Given an invalid ISBN, when adding a copy, then the API rejects with 400.

## Feature 3 — Rental Management
1. **Business problem:** Two members can be promised the same physical copy, and nobody tracks who has what.
2. **Feature:** Members reserve, cancel, and librarians move rentals through pickup and return.
3. **Actors:** Member, Librarian
4. **Business rules**
   - BR-01: Only registered members can reserve.
   - BR-02: The librarian's catalog must be active.
   - BR-03: The copy must belong to the librarian and be available.
   - BR-04: A member cannot hold more than 5 active rentals at once.
   - BR-05: A new rental starts as `Reserved`; the copy becomes unavailable immediately.
   - BR-06: A race between two members for the same copy resolves to one winner (409 for the loser).
   - BR-07: Only a `Reserved` rental can be cancelled (owner only); it frees the copy.
   - BR-08: A librarian can only move rentals forward: `Reserved → CheckedOut → Returned`, and only for their own rentals.
5. **Data:** Rental
6. **Endpoints:** `POST /api/rentals`, `GET /api/rentals/my`, `GET /api/rentals/{id}`, `DELETE /api/rentals/{id}`, `PUT /api/rentals/{id}/status`
7. **Acceptance criteria**
   - Given a free copy, when a member reserves it, then a Rental is created as `Reserved` and the copy is marked rented.
   - Given a copy already rented, when reserving it, then the API rejects with 400.
   - Given a `Reserved` rental, when the member cancels it, then it becomes `Cancelled` and the copy is free again.
   - Given a `Reserved` rental, when the librarian checks it out, then a `DueDate` (14 days out) is set.

## Feature 4 — Catalog Administration
1. **Business problem:** A librarian who has left should stop taking new reservations.
2. **Feature:** Admin deactivates a librarian.
3. **Actor:** Admin
4. **Business rules**
   - BR-D1: A deactivated librarian cannot add new copies or accept new reservations.
   - BR-D2: Existing rentals are left untouched.
5. **Endpoints:** `PUT /api/librarians/{id}/deactivate`

## Feature 5 — Automation
1. **Business problem:** Nobody notices when a book is overdue until a member is asked to return it.
2. **Feature:** Notifications and an hourly job marking overdue rentals.
3. **Actor:** System (Hangfire)
4. **Business rules**
   - BR-N1: Librarians are notified of new reservations and cancellations; members are notified of status changes. Notifications never block the request.
   - BR-N2: A `CheckedOut` rental becomes `Overdue` once its due date is more than 1 day in the past.

---

## Clarification Log
- Deactivating a librarian does not cancel or otherwise touch their existing rentals — only new activity is blocked.
- The loan period is fixed at 14 days from checkout; a per-book loan period is a possible future extension.
