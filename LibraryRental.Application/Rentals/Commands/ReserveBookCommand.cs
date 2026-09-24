using LibraryRental.Application.Common.Exceptions;
using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Domain.Entities;
using LibraryRental.Domain.Enums;
using Hangfire;
using MediatR;

namespace LibraryRental.Application.Rentals.Commands;

public record ReserveBookCommand(string UserId, Guid LibrarianId, Guid BookCopyId, string? Notes) : IRequest<Guid>;

public class ReserveBookCommandHandler : IRequestHandler<ReserveBookCommand, Guid>
{
    private const int MaxActiveRentals = 5;

    private readonly IMemberRepository _members;
    private readonly ILibrarianRepository _librarians;
    private readonly IBookCopyRepository _copies;
    private readonly IRentalRepository _rentals;
    private readonly IUnitOfWork _uow;

    public ReserveBookCommandHandler(
        IMemberRepository members, ILibrarianRepository librarians, IBookCopyRepository copies,
        IRentalRepository rentals, IUnitOfWork uow)
    {
        _members = members;
        _librarians = librarians;
        _copies = copies;
        _rentals = rentals;
        _uow = uow;
    }

    public async Task<Guid> Handle(ReserveBookCommand request, CancellationToken ct)
    {
        // BR-01: only registered members can reserve
        var member = await _members.GetByUserIdAsync(request.UserId, ct)
            ?? throw new ForbiddenException("Only registered members can reserve books.");

        var librarian = await _librarians.GetByIdAsync(request.LibrarianId, ct)
            ?? throw new NotFoundException("Librarian", request.LibrarianId);

        // BR-02: the librarian must be active
        if (!librarian.IsActive)
            throw new BusinessRuleException("This librarian's catalog is not accepting reservations.");

        // BR-03: the copy must belong to the librarian and be available
        var copy = await _copies.GetByIdAsync(request.BookCopyId, ct);
        if (copy is null || copy.LibrarianId != librarian.Id)
            throw new NotFoundException("BookCopy", request.BookCopyId);

        if (copy.IsRented)
            throw new BusinessRuleException("This book copy is already rented.");

        // BR-04: a member cannot hold more than 5 active rentals at once
        if (await _rentals.CountActiveByMemberAsync(member.Id, ct) >= MaxActiveRentals)
            throw new BusinessRuleException($"You already have {MaxActiveRentals} active rentals. Return a book before reserving another.");

        // BR-05: new rentals start as Reserved, and the copy becomes unavailable
        var rental = new Rental
        {
            MemberId = member.Id,
            LibrarianId = librarian.Id,
            BookCopyId = copy.Id,
            Status = RentalStatus.Reserved,
            Notes = request.Notes
        };
        copy.IsRented = true;

        await _rentals.AddAsync(rental, ct);

        // BR-06: if another member took the copy in parallel, this throws ConflictException (409)
        await _uow.SaveChangesAsync(ct);

        // After saving: the notification runs in the background, the request does not wait for it.
        var rentalId = rental.Id;
        BackgroundJob.Enqueue<INotificationService>(x => x.NotifyLibrarianOfNewReservation(rentalId));

        return rentalId;
    }
}
