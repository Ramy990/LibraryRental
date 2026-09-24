using LibraryRental.Application.Common.Exceptions;
using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Application.Common.Mappings;
using LibraryRental.Application.Common.Models;
using MediatR;

namespace LibraryRental.Application.Rentals.Queries;

// ---- Member: my rentals ----
public record GetMyRentalsQuery(string UserId) : IRequest<List<RentalDto>>;

public class GetMyRentalsQueryHandler : IRequestHandler<GetMyRentalsQuery, List<RentalDto>>
{
    private readonly IMemberRepository _members;
    private readonly IRentalRepository _rentals;

    public GetMyRentalsQueryHandler(IMemberRepository members, IRentalRepository rentals)
    {
        _members = members;
        _rentals = rentals;
    }

    public async Task<List<RentalDto>> Handle(GetMyRentalsQuery request, CancellationToken ct)
    {
        var member = await _members.GetByUserIdAsync(request.UserId, ct)
            ?? throw new ForbiddenException("Only members have rentals to list here.");

        var items = await _rentals.GetByMemberAsync(member.Id, ct);
        return items.Select(r => r.ToDto()).ToList();
    }
}

// ---- Librarian: rentals from their catalog ----
public record GetLibrarianRentalsQuery(string UserId) : IRequest<List<RentalDto>>;

public class GetLibrarianRentalsQueryHandler : IRequestHandler<GetLibrarianRentalsQuery, List<RentalDto>>
{
    private readonly ILibrarianRepository _librarians;
    private readonly IRentalRepository _rentals;

    public GetLibrarianRentalsQueryHandler(ILibrarianRepository librarians, IRentalRepository rentals)
    {
        _librarians = librarians;
        _rentals = rentals;
    }

    public async Task<List<RentalDto>> Handle(GetLibrarianRentalsQuery request, CancellationToken ct)
    {
        var librarian = await _librarians.GetByUserIdAsync(request.UserId, ct)
            ?? throw new ForbiddenException("Only librarians have a rental list here.");

        var items = await _rentals.GetByLibrarianAsync(librarian.Id, ct);
        return items.Select(r => r.ToDto()).ToList();
    }
}

// ---- One rental (its member, its librarian, or admin) ----
public record GetRentalByIdQuery(Guid Id, string UserId, bool IsAdmin) : IRequest<RentalDto>;

public class GetRentalByIdQueryHandler : IRequestHandler<GetRentalByIdQuery, RentalDto>
{
    private readonly IMemberRepository _members;
    private readonly ILibrarianRepository _librarians;
    private readonly IRentalRepository _rentals;

    public GetRentalByIdQueryHandler(
        IMemberRepository members, ILibrarianRepository librarians, IRentalRepository rentals)
    {
        _members = members;
        _librarians = librarians;
        _rentals = rentals;
    }

    public async Task<RentalDto> Handle(GetRentalByIdQuery request, CancellationToken ct)
    {
        var rental = await _rentals.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Rental", request.Id);

        if (!request.IsAdmin)
        {
            var member = await _members.GetByUserIdAsync(request.UserId, ct);
            var librarian = await _librarians.GetByUserIdAsync(request.UserId, ct);

            var isOwnerMember = member is not null && rental.MemberId == member.Id;
            var isOwnerLibrarian = librarian is not null && rental.LibrarianId == librarian.Id;

            if (!isOwnerMember && !isOwnerLibrarian)
                throw new ForbiddenException("You do not have access to this rental.");
        }

        return rental.ToDto();
    }
}
