namespace LibraryRental.Domain.Entities;

public class BookCopy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LibrarianId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public bool IsRented { get; set; }

    // Optimistic concurrency token: stops two members reserving the same copy at once.
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Librarian Librarian { get; set; } = null!;
}
