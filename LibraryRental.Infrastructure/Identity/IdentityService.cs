using LibraryRental.Application.Common.Exceptions;
using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace LibraryRental.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityUser> _users;
    private readonly JwtTokenGenerator _jwt;

    public IdentityService(UserManager<IdentityUser> users, JwtTokenGenerator jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<string> CreateUserAsync(string email, string password, string role, CancellationToken ct = default)
    {
        var user = new IdentityUser { UserName = email, Email = email };
        var result = await _users.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new BusinessRuleException(string.Join(" ", result.Errors.Select(e => e.Description)));

        var roleResult = await _users.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            throw new BusinessRuleException(string.Join(" ", roleResult.Errors.Select(e => e.Description)));

        return user.Id;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user is null || !await _users.CheckPasswordAsync(user, password))
            throw new UnauthorizedException("Invalid email or password.");

        var roles = await _users.GetRolesAsync(user);
        var (token, expires) = _jwt.Generate(user, roles);
        return new AuthResult(token, user.Email!, roles.FirstOrDefault() ?? string.Empty, expires);
    }
}
