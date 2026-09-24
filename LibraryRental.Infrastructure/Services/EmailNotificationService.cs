using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LibraryRental.Infrastructure.Services;

/// <summary>
/// Runs inside Hangfire, not inside the HTTP request.
/// It currently LOGS the email. Replace the Send method with SMTP / MailKit / SendGrid for real emails.
/// A failure here shows up in the Hangfire Dashboard (Failed jobs) and is retried automatically.
/// </summary>
public class EmailNotificationService : INotificationService
{
    private readonly IRentalRepository _rentals;
    private readonly UserManager<IdentityUser> _users;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IRentalRepository rentals, UserManager<IdentityUser> users, ILogger<EmailNotificationService> logger)
    {
        _rentals = rentals;
        _users = users;
        _logger = logger;
    }

    public async Task NotifyLibrarianOfNewReservation(Guid rentalId)
    {
        var r = await LoadAsync(rentalId);
        var email = await EmailOfAsync(r.Librarian.UserId);
        Send(email, "New reservation",
            $"{r.Librarian.FullName}: {r.Member.FullName} reserved '{r.BookCopy.Title}'.");
    }

    public async Task NotifyLibrarianOfCancellation(Guid rentalId)
    {
        var r = await LoadAsync(rentalId);
        var email = await EmailOfAsync(r.Librarian.UserId);
        Send(email, "Reservation cancelled",
            $"{r.Librarian.FullName}: {r.Member.FullName} cancelled the reservation for '{r.BookCopy.Title}'.");
    }

    public async Task NotifyMemberOfStatusChange(Guid rentalId)
    {
        var r = await LoadAsync(rentalId);
        var email = await EmailOfAsync(r.Member.UserId);
        Send(email, "Rental update",
            $"Hi {r.Member.FullName}: your rental of '{r.BookCopy.Title}' is now {r.Status}.");
    }

    private async Task<Rental> LoadAsync(Guid id) =>
        await _rentals.GetByIdAsync(id)
        ?? throw new InvalidOperationException($"Rental {id} was not found for notification.");

    private async Task<string> EmailOfAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        return user?.Email ?? throw new InvalidOperationException($"User {userId} has no email.");
    }

    private void Send(string to, string subject, string body) =>
        _logger.LogInformation("EMAIL to {To} | {Subject} | {Body}", to, subject, body);
}
