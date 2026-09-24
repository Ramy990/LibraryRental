namespace LibraryRental.Application.Common.Interfaces;

public interface IUnitOfWork
{
    /// <summary>Saves everything in one transaction. Throws ConflictException on a concurrent reservation.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
