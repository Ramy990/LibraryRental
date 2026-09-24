using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LibraryRental.Application.Services;

public interface IRentalMaintenanceService
{
    Task MarkOverdueRentalsAsync();
}

/// <summary>
/// Runs as a Hangfire recurring job (hourly).
/// CheckedOut rentals still open 1 day after their due date become Overdue.
/// </summary>
public class RentalMaintenanceService : IRentalMaintenanceService
{
    private static readonly TimeSpan GracePeriod = TimeSpan.FromDays(1);

    private readonly IRentalRepository _rentals;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<RentalMaintenanceService> _logger;

    public RentalMaintenanceService(
        IRentalRepository rentals,
        IUnitOfWork uow,
        ILogger<RentalMaintenanceService> logger)
    {
        _rentals = rentals;
        _uow = uow;
        _logger = logger;
    }

    public async Task MarkOverdueRentalsAsync()
    {
        var now = DateTime.UtcNow;
        var overdue = await _rentals.GetOverdueEligibleAsync(now - GracePeriod);

        foreach (var rental in overdue)
        {
            rental.Status = RentalStatus.Overdue;
            rental.StatusUpdatedAt = now;
        }

        if (overdue.Count > 0)
            await _uow.SaveChangesAsync();

        _logger.LogInformation("MarkOverdueRentals finished: {Count} rental(s) marked as Overdue.", overdue.Count);
    }
}
