using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Domain.Entities;
using LibraryRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryRental.Infrastructure.Repositories;

public class BookCopyRepository : IBookCopyRepository
{
    private readonly AppDbContext _db;
    public BookCopyRepository(AppDbContext db) => _db = db;

    public Task<BookCopy?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.BookCopies.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<BookCopy>> GetAvailableAsync(Guid librarianId, CancellationToken ct = default) =>
        _db.BookCopies.AsNoTracking()
            .Where(c => c.LibrarianId == librarianId && !c.IsRented)
            .OrderBy(c => c.Title)
            .ToListAsync(ct);

    public async Task AddAsync(BookCopy copy, CancellationToken ct = default) =>
        await _db.BookCopies.AddAsync(copy, ct);
}
