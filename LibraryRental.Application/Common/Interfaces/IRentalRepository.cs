using LibraryRental.Domain.Entities;

namespace LibraryRental.Application.Common.Interfaces;

public interface IRentalRepository
{
    /// <summary>Returns the rental with Member, Librarian and BookCopy loaded.</summary>
    Task<Rental?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<int> CountActiveByMemberAsync(Guid memberId, CancellationToken ct = default);
    Task<List<Rental>> GetByMemberAsync(Guid memberId, CancellationToken ct = default);
    Task<List<Rental>> GetByLibrarianAsync(Guid librarianId, CancellationToken ct = default);
    /// <summary>CheckedOut rentals whose due date is before the given time.</summary>
    Task<List<Rental>> GetOverdueEligibleAsync(DateTime dueBeforeUtc, CancellationToken ct = default);
    Task AddAsync(Rental rental, CancellationToken ct = default);
}
