using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Domain.Entities;
using LibraryRental.Domain.Enums;
using LibraryRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryRental.Infrastructure.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _db;
    public RentalRepository(AppDbContext db) => _db = db;

    private IQueryable<Rental> WithDetails() =>
        _db.Rentals
            .Include(r => r.Librarian)
            .Include(r => r.Member)
            .Include(r => r.BookCopy);

    public Task<Rental?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        WithDetails().FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<int> CountActiveByMemberAsync(Guid memberId, CancellationToken ct = default) =>
        _db.Rentals.CountAsync(r =>
            r.MemberId == memberId &&
            (r.Status == RentalStatus.Reserved || r.Status == RentalStatus.CheckedOut), ct);

    public Task<List<Rental>> GetByMemberAsync(Guid memberId, CancellationToken ct = default) =>
        WithDetails().AsNoTracking()
            .Where(r => r.MemberId == memberId)
            .OrderByDescending(r => r.ReservedAt)
            .ToListAsync(ct);

    public Task<List<Rental>> GetByLibrarianAsync(Guid librarianId, CancellationToken ct = default) =>
        WithDetails().AsNoTracking()
            .Where(r => r.LibrarianId == librarianId)
            .OrderBy(r => r.ReservedAt)
            .ToListAsync(ct);

    public Task<List<Rental>> GetOverdueEligibleAsync(DateTime dueBeforeUtc, CancellationToken ct = default) =>
        _db.Rentals
            .Where(r => r.Status == RentalStatus.CheckedOut && r.DueDate != null && r.DueDate < dueBeforeUtc)
            .ToListAsync(ct);

    public async Task AddAsync(Rental rental, CancellationToken ct = default) =>
        await _db.Rentals.AddAsync(rental, ct);
}
