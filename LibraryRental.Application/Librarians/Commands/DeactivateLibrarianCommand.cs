using LibraryRental.Application.Common.Exceptions;
using LibraryRental.Application.Common.Interfaces;
using MediatR;

namespace LibraryRental.Application.Librarians.Commands;

public record DeactivateLibrarianCommand(Guid LibrarianId) : IRequest;

public class DeactivateLibrarianCommandHandler : IRequestHandler<DeactivateLibrarianCommand>
{
    private readonly ILibrarianRepository _librarians;
    private readonly IUnitOfWork _uow;

    public DeactivateLibrarianCommandHandler(ILibrarianRepository librarians, IUnitOfWork uow)
    {
        _librarians = librarians;
        _uow = uow;
    }

    public async Task Handle(DeactivateLibrarianCommand request, CancellationToken ct)
    {
        // The Admin role itself is enforced on the controller.
        var librarian = await _librarians.GetByIdAsync(request.LibrarianId, ct)
            ?? throw new NotFoundException("Librarian", request.LibrarianId);

        if (!librarian.IsActive)
            throw new BusinessRuleException("The librarian is already deactivated.");

        // Existing rentals are intentionally left untouched (see PRD Clarification Log).
        librarian.IsActive = false;
        librarian.DeactivatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }
}
