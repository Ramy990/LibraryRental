using LibraryRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryRental.Infrastructure.Persistence.Configurations;

public class LibrarianConfiguration : IEntityTypeConfiguration<Librarian>
{
    public void Configure(EntityTypeBuilder<Librarian> b)
    {
        b.HasKey(l => l.Id);
        b.Property(l => l.UserId).IsRequired().HasMaxLength(450);
        b.HasIndex(l => l.UserId).IsUnique();
        b.Property(l => l.FullName).IsRequired().HasMaxLength(200);
        b.Property(l => l.Department).IsRequired().HasMaxLength(100);

        b.HasMany(l => l.Copies)
            .WithOne(c => c.Librarian)
            .HasForeignKey(c => c.LibrarianId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
