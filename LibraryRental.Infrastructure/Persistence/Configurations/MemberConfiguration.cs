using LibraryRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryRental.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> b)
    {
        b.HasKey(m => m.Id);
        b.Property(m => m.UserId).IsRequired().HasMaxLength(450);
        b.HasIndex(m => m.UserId).IsUnique();
        b.Property(m => m.FullName).IsRequired().HasMaxLength(200);
        b.Property(m => m.PhoneNumber).HasMaxLength(30);
    }
}
