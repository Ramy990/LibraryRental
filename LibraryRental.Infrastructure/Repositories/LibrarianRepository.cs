using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Domain.Entities;
using LibraryRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryRental.Infrastructure.Repositories;

public class LibrarianRepository : ILibrarianRepository
{
    private readonly AppDbContext _db;
    public LibrarianRepository(AppDbContext db) => _db = db;

    public Task<Librarian?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Librarians.FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<Librarian?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        _db.Librarians.FirstOrDefaultAsync(l => l.UserId == userId, ct);

    public Task<List<Librarian>> GetActiveAsync(string? department, CancellationToken ct = default)
    {
        var query = _db.Librarians.AsNoTracking().Where(l => l.IsActive);
        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(l => l.Department == department);
        return query.OrderBy(l => l.FullName).ToListAsync(ct);
    }

    public async Task AddAsync(Librarian librarian, CancellationToken ct = default) =>
        await _db.Librarians.AddAsync(librarian, ct);
}
