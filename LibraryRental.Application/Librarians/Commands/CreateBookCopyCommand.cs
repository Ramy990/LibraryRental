using System.Text.RegularExpressions;
using LibraryRental.Application.Common.Exceptions;
using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Domain.Entities;
using MediatR;

namespace LibraryRental.Application.Librarians.Commands;

public record CreateBookCopyCommand(string UserId, string Title, string Author, string ISBN) : IRequest<Guid>;

public class CreateBookCopyCommandHandler : IRequestHandler<CreateBookCopyCommand, Guid>
{
    private static readonly Regex IsbnDigits = new("^[0-9]{10}([0-9]{3})?$", RegexOptions.Compiled);

    private readonly ILibrarianRepository _librarians;
    private readonly IBookCopyRepository _copies;
    private readonly IUnitOfWork _uow;

    public CreateBookCopyCommandHandler(ILibrarianRepository librarians, IBookCopyRepository copies, IUnitOfWork uow)
    {
        _librarians = librarians;
        _copies = copies;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateBookCopyCommand request, CancellationToken ct)
    {
        // BR-S1: only a librarian, and only for themselves (the librarian is resolved from the token)
        var librarian = await _librarians.GetByUserIdAsync(request.UserId, ct)
            ?? throw new ForbiddenException("Only registered librarians can add book copies.");

        if (!librarian.IsActive)
            throw new BusinessRuleException("A deactivated librarian cannot add book copies.");

        var title = request.Title.Trim();
        var author = request.Author.Trim();
        var isbn = request.ISBN.Replace("-", "").Replace(" ", "");

        // BR-S2: title and author required, ISBN must be a valid 10 or 13 digit ISBN
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
            throw new BusinessRuleException("A book copy must have a Title and an Author.");

        if (!IsbnDigits.IsMatch(isbn))
            throw new BusinessRuleException("ISBN must be a valid 10 or 13 digit ISBN.");

        var copy = new BookCopy { LibrarianId = librarian.Id, Title = title, Author = author, ISBN = isbn };
        await _copies.AddAsync(copy, ct);
        await _uow.SaveChangesAsync(ct);
        return copy.Id;
    }
}
