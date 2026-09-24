using LibraryRental.Application.Common.Constants;
using LibraryRental.Application.Common.Exceptions;
using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Application.Common.Models;
using LibraryRental.Domain.Entities;
using MediatR;

namespace LibraryRental.Application.Auth.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string Role,
    string? PhoneNumber,
    string? Department) : IRequest<AuthResult>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IIdentityService _identity;
    private readonly ILibrarianRepository _librarians;
    private readonly IMemberRepository _members;
    private readonly IUnitOfWork _uow;

    public RegisterCommandHandler(
        IIdentityService identity, ILibrarianRepository librarians, IMemberRepository members, IUnitOfWork uow)
    {
        _identity = identity;
        _librarians = librarians;
        _members = members;
        _uow = uow;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var isLibrarian = string.Equals(request.Role, Roles.Librarian, StringComparison.OrdinalIgnoreCase);
        var isMember = string.Equals(request.Role, Roles.Member, StringComparison.OrdinalIgnoreCase);

        // BR-A1: only Member or Librarian can self-register
        if (!isLibrarian && !isMember)
            throw new BusinessRuleException("Role must be 'Member' or 'Librarian'.");

        // BR-A3: a librarian needs a department (validated BEFORE creating the login user)
        if (isLibrarian && string.IsNullOrWhiteSpace(request.Department))
            throw new BusinessRuleException("A librarian must provide a Department.");

        // BR-A2: unique email is enforced by Identity and surfaces as a BusinessRuleException
        var userId = await _identity.CreateUserAsync(
            request.Email, request.Password, isLibrarian ? Roles.Librarian : Roles.Member, ct);

        if (isLibrarian)
        {
            await _librarians.AddAsync(new Librarian
            {
                UserId = userId,
                FullName = request.FullName,
                Department = request.Department!.Trim()
            }, ct);
        }
        else
        {
            await _members.AddAsync(new Member
            {
                UserId = userId,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return await _identity.LoginAsync(request.Email, request.Password, ct);
    }
}
