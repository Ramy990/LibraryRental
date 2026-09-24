using LibraryRental.Application.Common.Exceptions;
using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Domain.Enums;
using Hangfire;
using MediatR;

namespace LibraryRental.Application.Rentals.Commands;

public record CancelRentalCommand(Guid RentalId, string UserId) : IRequest;

public class CancelRentalCommandHandler : IRequestHandler<CancelRentalCommand>
{
    private readonly IMemberRepository _members;
    private readonly IRentalRepository _rentals;
    private readonly IUnitOfWork _uow;

    public CancelRentalCommandHandler(IMemberRepository members, IRentalRepository rentals, IUnitOfWork uow)
    {
        _members = members;
        _rentals = rentals;
        _uow = uow;
    }

    public async Task Handle(CancelRentalCommand request, CancellationToken ct)
    {
        var rental = await _rentals.GetByIdAsync(request.RentalId, ct)
            ?? throw new NotFoundException("Rental", request.RentalId);

        var member = await _members.GetByUserIdAsync(request.UserId, ct)
            ?? throw new ForbiddenException("Only members can cancel rentals.");

        // Only the owner can cancel
        if (rental.MemberId != member.Id)
            throw new ForbiddenException("You can only cancel your own rentals.");

        // Only a Reserved rental (not yet picked up) can be cancelled; once checked out, return it instead.
        if (rental.Status != RentalStatus.Reserved)
            throw new BusinessRuleException($"A rental with status '{rental.Status}' cannot be cancelled.");

        // Not deleted: status Cancelled, CancelledAt recorded, copy released
        var now = DateTime.UtcNow;
        rental.Status = RentalStatus.Cancelled;
        rental.CancelledAt = now;
        rental.StatusUpdatedAt = now;
        rental.BookCopy.IsRented = false;

        await _uow.SaveChangesAsync(ct);

        var rentalId = rental.Id;
        BackgroundJob.Enqueue<INotificationService>(x => x.NotifyLibrarianOfCancellation(rentalId));
    }
}
