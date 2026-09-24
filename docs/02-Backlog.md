# Delivery Plan (Day 2)

Priority: Highest / High / Medium / Low / Lowest. Points: 1, 2, 3, 5, 8 (13 = split it).

## Epic 1 — Identity & Access
| Story | AC | Priority | Pts |
|---|---|---|---|
| As a Member, I want to register and log in, so that I can reserve books | User + Member created, JWT returned | Highest | 3 |
| As a Librarian, I want to register with my department, so that I can manage a catalog | Librarian row created, department required | High | 2 |
| As the system, I want roles enforced on endpoints, so that users only do what their role allows | Wrong role → 403 | High | 2 |

## Epic 2 — Librarian Catalog
| Story | AC | Priority | Pts |
|---|---|---|---|
| As a Librarian, I want to add book copies, so that members can borrow them | Title/author/ISBN valid, saved available | Highest | 3 |
| As a Librarian, I want invalid ISBNs rejected, so that catalog data stays clean | Bad ISBN → 400 | High | 2 |
| As a Member, I want to list a librarian's available copies, so that I can choose a book | Only unrented copies | High | 2 |

## Epic 3 — Rental Management
| Story | AC | Priority | Pts |
|---|---|---|---|
| As a Member, I want to reserve a free copy, so that I get first claim on the book | Created as Reserved, copy marked rented | Highest | 5 |
| As a Member, I want the system to stop double reservations, so that nobody loses a copy | Rented copy → 400; race → 409 | Highest | 3 |
| As a Member, I want a cap on active rentals, so that copies stay available for everyone | 6th active rental → 400 | Medium | 2 |
| As a Member, I want to cancel my reservation, so that the librarian can offer it to someone else | 204, Cancelled, copy freed | High | 3 |
| As a Member, I want to cancel only my own reservations, so that my data is protected | Non-owner → 403 | Medium | 1 |
| As a Librarian, I want to move a rental forward, so that I can track pickup and return | Valid forward transition, DueDate set on checkout | High | 3 |
| As a Librarian, I want invalid transitions rejected, so that history stays accurate | Skip/back → 400 | High | 2 |
| As a Librarian, I want to update only my own rentals | Other librarian → 403 | Medium | 1 |
| As an Admin, I want to deactivate a librarian, so that no new reservations arrive | Reserve → 400 afterwards | Medium | 2 |

## Epic 4 — Notifications & Automation
| Story | AC | Priority | Pts |
|---|---|---|---|
| As a Librarian, I want an email on new reservation / cancellation, so that I stay informed | Hangfire job succeeds | Medium | 3 |
| As a Member, I want an email when my rental status changes | Hangfire job succeeds | Medium | 2 |
| As a Library, I want overdue rentals marked automatically | Recurring job visible in Dashboard | Low | 3 |

## Epic 5 — Engineering Quality
| Story | Priority | Pts |
|---|---|---|
| Swagger docs with response codes on every endpoint | Medium | 2 |
| Central exception handling mapped to 400/401/403/404/409 | High | 2 |

---

## Task breakdown — "Member reserves a free copy" (Story, 5 pts)
1. Create `Librarian`, `Member`, `BookCopy`, `Rental` entities + `RentalStatus` enum (Domain)
2. Add EF configurations, filtered unique index on `Rental.BookCopyId`, RowVersion on `BookCopy` + migration (Infrastructure)
3. Define repository interfaces + `IUnitOfWork` (Application)
4. Create `ReserveBookCommand` + handler
5. Validation — member exists (BR-01)
6. Validation — librarian is active (BR-02)
7. Validation — copy belongs to librarian and is available (BR-03)
8. Validation — active rental cap for the member (BR-04)
9. Implement `POST /api/rentals` in `RentalsController`
10. Enqueue `NotifyLibrarianOfNewReservation` with Hangfire
11. Swagger docs (summary + `ProducesResponseType`)
12. Test each acceptance criterion from Swagger
