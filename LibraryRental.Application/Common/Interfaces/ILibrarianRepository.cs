using LibraryRental.Domain.Entities;

namespace LibraryRental.Application.Common.Interfaces;

public interface ILibrarianRepository
{
    Task<Librarian?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Librarian?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<List<Librarian>> GetActiveAsync(string? department, CancellationToken ct = default);
    Task AddAsync(Librarian librarian, CancellationToken ct = default);
}
