using LibraryRental.Domain.Enums;

namespace LibraryRental.Domain.Entities;

public class Rental
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Guid LibrarianId { get; set; }
    public Guid BookCopyId { get; set; }
    public RentalStatus Status { get; set; } = RentalStatus.Reserved;
    public string? Notes { get; set; }
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StatusUpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public Member Member { get; set; } = null!;
    public Librarian Librarian { get; set; } = null!;
    public BookCopy BookCopy { get; set; } = null!;
}
