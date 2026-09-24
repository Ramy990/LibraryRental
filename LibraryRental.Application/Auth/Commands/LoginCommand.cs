using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Application.Common.Models;
using MediatR;

namespace LibraryRental.Application.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IIdentityService _identity;

    public LoginCommandHandler(IIdentityService identity) => _identity = identity;

    public Task<AuthResult> Handle(LoginCommand request, CancellationToken ct) =>
        _identity.LoginAsync(request.Email, request.Password, ct);
}
