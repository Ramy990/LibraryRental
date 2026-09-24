using LibraryRental.API.Contracts;
using LibraryRental.Application.Auth.Commands;
using LibraryRental.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryRental.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Register as a Member or a Librarian and receive a JWT.</summary>
    /// <response code="200">Registered. The token is returned.</response>
    /// <response code="400">Invalid role, email already used, weak password, or missing librarian data.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResult>> Register(RegisterRequest request) =>
        Ok(await _mediator.Send(new RegisterCommand(
            request.Email, request.Password, request.FullName, request.Role,
            request.PhoneNumber, request.Department)));

    /// <summary>Log in and receive a JWT.</summary>
    /// <response code="200">Logged in. The token is returned.</response>
    /// <response code="401">Wrong email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest request) =>
        Ok(await _mediator.Send(new LoginCommand(request.Email, request.Password)));
}
