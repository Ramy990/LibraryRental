using LibraryRental.Domain.Entities;

namespace LibraryRental.Application.Common.Interfaces;

public interface IMemberRepository
{
    Task<Member?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task AddAsync(Member member, CancellationToken ct = default);
}
