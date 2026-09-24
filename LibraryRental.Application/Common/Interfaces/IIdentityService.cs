using LibraryRental.Application.Common.Models;

namespace LibraryRental.Application.Common.Interfaces;

public interface IIdentityService
{
    /// <summary>Creates the login user and assigns the role. Returns the new UserId.</summary>
    Task<string> CreateUserAsync(string email, string password, string role, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default);
}
