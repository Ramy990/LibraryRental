namespace LibraryRental.Application.Common.Models;

public record AuthResult(string Token, string Email, string Role, DateTime ExpiresAtUtc);

public record LibrarianDto(Guid Id, string FullName, string Department, bool IsActive);

public record BookCopyDto(Guid Id, string Title, string Author, string ISBN);

public record RentalDto(
    Guid Id,
    Guid LibrarianId,
    string LibrarianName,
    Guid MemberId,
    string MemberName,
    Guid BookCopyId,
    string Title,
    string Author,
    string Status,
    string? Notes,
    DateTime ReservedAt,
    DateTime? StatusUpdatedAt,
    DateTime? CancelledAt,
    DateTime? DueDate,
    DateTime? ReturnedAt);
