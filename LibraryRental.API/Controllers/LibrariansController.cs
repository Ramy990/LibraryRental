using System.Security.Claims;
using LibraryRental.API.Contracts;
using LibraryRental.Application.Common.Constants;
using LibraryRental.Application.Common.Models;
using LibraryRental.Application.Librarians.Commands;
using LibraryRental.Application.Librarians.Queries;
using LibraryRental.Application.Rentals.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryRental.API.Controllers;

[ApiController]
[Route("api/librarians")]
public class LibrariansController : ControllerBase
{
    private readonly IMediator _mediator;
    public LibrariansController(IMediator mediator) => _mediator = mediator;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>List active librarians, optionally filtered by department.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<LibrarianDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LibrarianDto>>> GetAll([FromQuery] string? department) =>
        Ok(await _mediator.Send(new GetLibrariansQuery(department)));

    /// <summary>Get one librarian.</summary>
    /// <response code="404">Librarian not found.</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LibrarianDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LibrarianDto>> GetById(Guid id) =>
        Ok(await _mediator.Send(new GetLibrarianByIdQuery(id)));

    /// <summary>List the available book copies in a librarian's catalog.</summary>
    /// <response code="404">Librarian not found.</response>
    [HttpGet("{id:guid}/copies")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<BookCopyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<BookCopyDto>>> GetCopies(Guid id) =>
        Ok(await _mediator.Send(new GetLibrarianCopiesQuery(id)));

    /// <summary>Add a book copy to the logged-in librarian's catalog.</summary>
    /// <response code="201">Copy created. The copy id is returned.</response>
    /// <response code="400">Missing title/author, or an invalid ISBN.</response>
    /// <response code="403">Not a librarian.</response>
    [HttpPost("me/copies")]
    [Authorize(Roles = Roles.Librarian)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCopy(CreateBookCopyRequest request)
    {
        var id = await _mediator.Send(new CreateBookCopyCommand(UserId, request.Title, request.Author, request.ISBN));
        return Created($"/api/librarians/me/copies/{id}", new { id });
    }

    /// <summary>The logged-in librarian's rentals.</summary>
    [HttpGet("me/rentals")]
    [Authorize(Roles = Roles.Librarian)]
    [ProducesResponseType(typeof(List<RentalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<RentalDto>>> MyRentals() =>
        Ok(await _mediator.Send(new GetLibrarianRentalsQuery(UserId)));

    /// <summary>Deactivate a librarian (Admin). Existing rentals stay valid; new reservations are rejected.</summary>
    /// <response code="204">Deactivated.</response>
    /// <response code="400">Already deactivated.</response>
    /// <response code="403">Not an admin.</response>
    /// <response code="404">Librarian not found.</response>
    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _mediator.Send(new DeactivateLibrarianCommand(id));
        return NoContent();
    }
}
