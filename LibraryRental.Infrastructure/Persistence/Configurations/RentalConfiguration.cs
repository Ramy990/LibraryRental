using LibraryRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryRental.Infrastructure.Persistence.Configurations;

public class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> b)
    {
        b.HasKey(r => r.Id);
        b.Property(r => r.Notes).HasMaxLength(500);
        b.Property(r => r.Status).HasConversion<int>();

        // Safety net at DB level: one ACTIVE rental per copy.
        // Status 1 = Reserved, 2 = CheckedOut, 3 = Returned. Cancelled (4) and Overdue (5) free the row.
        b.HasIndex(r => r.BookCopyId)
            .IsUnique()
            .HasFilter("[Status] IN (1, 2, 3)");

        b.HasIndex(r => r.MemberId);
        b.HasIndex(r => r.LibrarianId);

        // Restrict avoids multiple cascade paths in SQL Server
        b.HasOne(r => r.Member).WithMany().HasForeignKey(r => r.MemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(r => r.Librarian).WithMany().HasForeignKey(r => r.LibrarianId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(r => r.BookCopy).WithMany().HasForeignKey(r => r.BookCopyId).OnDelete(DeleteBehavior.Restrict);
    }
}
