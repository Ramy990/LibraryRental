using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Domain.Entities;
using LibraryRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryRental.Infrastructure.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly AppDbContext _db;
    public MemberRepository(AppDbContext db) => _db = db;

    public Task<Member?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        _db.Members.FirstOrDefaultAsync(m => m.UserId == userId, ct);

    public async Task AddAsync(Member member, CancellationToken ct = default) =>
        await _db.Members.AddAsync(member, ct);
}
