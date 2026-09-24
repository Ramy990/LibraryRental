using LibraryRental.Application.Common.Exceptions;
using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Application.Common.Mappings;
using LibraryRental.Application.Common.Models;
using MediatR;

namespace LibraryRental.Application.Librarians.Queries;

// ---- List active librarians ----
public record GetLibrariansQuery(string? Department) : IRequest<List<LibrarianDto>>;

public class GetLibrariansQueryHandler : IRequestHandler<GetLibrariansQuery, List<LibrarianDto>>
{
    private readonly ILibrarianRepository _librarians;
    public GetLibrariansQueryHandler(ILibrarianRepository librarians) => _librarians = librarians;

    public async Task<List<LibrarianDto>> Handle(GetLibrariansQuery request, CancellationToken ct)
    {
        var librarians = await _librarians.GetActiveAsync(request.Department, ct);
        return librarians.Select(l => l.ToDto()).ToList();
    }
}

// ---- One librarian ----
public record GetLibrarianByIdQuery(Guid Id) : IRequest<LibrarianDto>;

public class GetLibrarianByIdQueryHandler : IRequestHandler<GetLibrarianByIdQuery, LibrarianDto>
{
    private readonly ILibrarianRepository _librarians;
    public GetLibrarianByIdQueryHandler(ILibrarianRepository librarians) => _librarians = librarians;

    public async Task<LibrarianDto> Handle(GetLibrarianByIdQuery request, CancellationToken ct)
    {
        var librarian = await _librarians.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Librarian", request.Id);
        return librarian.ToDto();
    }
}

// ---- Available copies in a librarian's catalog ----
public record GetLibrarianCopiesQuery(Guid LibrarianId) : IRequest<List<BookCopyDto>>;

public class GetLibrarianCopiesQueryHandler : IRequestHandler<GetLibrarianCopiesQuery, List<BookCopyDto>>
{
    private readonly ILibrarianRepository _librarians;
    private readonly IBookCopyRepository _copies;

    public GetLibrarianCopiesQueryHandler(ILibrarianRepository librarians, IBookCopyRepository copies)
    {
        _librarians = librarians;
        _copies = copies;
    }

    public async Task<List<BookCopyDto>> Handle(GetLibrarianCopiesQuery request, CancellationToken ct)
    {
        var librarian = await _librarians.GetByIdAsync(request.LibrarianId, ct)
            ?? throw new NotFoundException("Librarian", request.LibrarianId);

        // A deactivated librarian accepts no reservations, so there is nothing to offer.
        if (!librarian.IsActive) return new List<BookCopyDto>();

        var copies = await _copies.GetAvailableAsync(librarian.Id, ct);
        return copies.Select(c => c.ToDto()).ToList();
    }
}
