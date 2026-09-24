namespace LibraryRental.Application.Common.Interfaces;

/// <summary>
/// Executed by Hangfire in the background (never inside the request thread).
/// Implemented in Infrastructure.
/// </summary>
public interface INotificationService
{
    Task NotifyLibrarianOfNewReservation(Guid rentalId);
    Task NotifyLibrarianOfCancellation(Guid rentalId);
    Task NotifyMemberOfStatusChange(Guid rentalId);
}
