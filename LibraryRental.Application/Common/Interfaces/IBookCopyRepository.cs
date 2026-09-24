using LibraryRental.Domain.Entities;

namespace LibraryRental.Application.Common.Interfaces;

public interface IBookCopyRepository
{
    Task<BookCopy?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BookCopy>> GetAvailableAsync(Guid librarianId, CancellationToken ct = default);
    Task AddAsync(BookCopy copy, CancellationToken ct = default);
}
