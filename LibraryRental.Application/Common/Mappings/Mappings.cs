using LibraryRental.Application.Common.Models;
using LibraryRental.Domain.Entities;

namespace LibraryRental.Application.Common.Mappings;

public static class Mappings
{
    public static LibrarianDto ToDto(this Librarian l) =>
        new(l.Id, l.FullName, l.Department, l.IsActive);

    public static BookCopyDto ToDto(this BookCopy c) =>
        new(c.Id, c.Title, c.Author, c.ISBN);

    /// <summary>Requires Librarian, Member and BookCopy to be loaded.</summary>
    public static RentalDto ToDto(this Rental r) =>
        new(r.Id, r.LibrarianId, r.Librarian.FullName, r.MemberId, r.Member.FullName,
            r.BookCopyId, r.BookCopy.Title, r.BookCopy.Author, r.Status.ToString(),
            r.Notes, r.ReservedAt, r.StatusUpdatedAt, r.CancelledAt, r.DueDate, r.ReturnedAt);
}
