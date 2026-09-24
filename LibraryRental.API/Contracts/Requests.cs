using System.ComponentModel.DataAnnotations;
using LibraryRental.Domain.Enums;

namespace LibraryRental.API.Contracts;

public class RegisterRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string FullName { get; set; } = string.Empty;

    /// <summary>"Member" or "Librarian".</summary>
    [Required] public string Role { get; set; } = "Member";

    public string? PhoneNumber { get; set; }

    /// <summary>Required when Role is Librarian.</summary>
    public string? Department { get; set; }
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class CreateBookCopyRequest
{
    [Required, MaxLength(300)] public string Title { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Author { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string ISBN { get; set; } = string.Empty;
}

public class ReserveBookRequest
{
    [Required] public Guid LibrarianId { get; set; }
    [Required] public Guid BookCopyId { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public class UpdateRentalStatusRequest
{
    /// <summary>"CheckedOut" or "Returned".</summary>
    [Required] public RentalStatus Status { get; set; }
}
