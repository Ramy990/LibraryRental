using LibraryRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryRental.Infrastructure.Persistence.Configurations;

public class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
{
    public void Configure(EntityTypeBuilder<BookCopy> b)
    {
        b.HasKey(c => c.Id);
        b.Property(c => c.Title).IsRequired().HasMaxLength(300);
        b.Property(c => c.Author).IsRequired().HasMaxLength(200);
        b.Property(c => c.ISBN).IsRequired().HasMaxLength(20);
        b.HasIndex(c => new { c.LibrarianId, c.Title });

        // Concurrency token: two parallel reservations of the same copy -> the second one fails.
        b.Property(c => c.RowVersion).IsRowVersion();
    }
}
