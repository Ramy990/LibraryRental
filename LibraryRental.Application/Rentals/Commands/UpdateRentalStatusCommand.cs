using LibraryRental.Application.Common.Exceptions;
using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Domain.Enums;
using Hangfire;
using MediatR;

namespace LibraryRental.Application.Rentals.Commands;

public record UpdateRentalStatusCommand(Guid RentalId, string UserId, RentalStatus NewStatus) : IRequest;

public class UpdateRentalStatusCommandHandler : IRequestHandler<UpdateRentalStatusCommand>
{
    private static readonly TimeSpan LoanPeriod = TimeSpan.FromDays(14);

    private readonly ILibrarianRepository _librarians;
    private readonly IRentalRepository _rentals;
    private readonly IUnitOfWork _uow;

    public UpdateRentalStatusCommandHandler(
        ILibrarianRepository librarians, IRentalRepository rentals, IUnitOfWork uow)
    {
        _librarians = librarians;
        _rentals = rentals;
        _uow = uow;
    }

    public async Task Handle(UpdateRentalStatusCommand request, CancellationToken ct)
    {
        var rental = await _rentals.GetByIdAsync(request.RentalId, ct)
            ?? throw new NotFoundException("Rental", request.RentalId);

        var librarian = await _librarians.GetByUserIdAsync(request.UserId, ct)
            ?? throw new ForbiddenException("Only librarians can update rental status.");

        // Only the rental's own librarian
        if (rental.LibrarianId != librarian.Id)
            throw new ForbiddenException("You can only update your own rentals.");

        // Forward-only: Reserved -> CheckedOut -> Returned
        var allowed =
            (rental.Status == RentalStatus.Reserved && request.NewStatus == RentalStatus.CheckedOut) ||
            (rental.Status == RentalStatus.CheckedOut && request.NewStatus == RentalStatus.Returned);

        if (!allowed)
            throw new BusinessRuleException(
                $"Invalid transition from '{rental.Status}' to '{request.NewStatus}'. Allowed: Reserved → CheckedOut → Returned.");

        var now = DateTime.UtcNow;

        if (request.NewStatus == RentalStatus.CheckedOut)
            rental.DueDate = now.Add(LoanPeriod);

        if (request.NewStatus == RentalStatus.Returned)
        {
            rental.ReturnedAt = now;
            rental.BookCopy.IsRented = false;
        }

        rental.Status = request.NewStatus;
        rental.StatusUpdatedAt = now;
        await _uow.SaveChangesAsync(ct);

        var rentalId = rental.Id;
        BackgroundJob.Enqueue<INotificationService>(x => x.NotifyMemberOfStatusChange(rentalId));
    }
}
