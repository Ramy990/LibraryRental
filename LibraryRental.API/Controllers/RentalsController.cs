using System.Security.Claims;
using LibraryRental.API.Contracts;
using LibraryRental.Application.Common.Constants;
using LibraryRental.Application.Common.Models;
using LibraryRental.Application.Rentals.Commands;
using LibraryRental.Application.Rentals.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryRental.API.Controllers;

[ApiController]
[Authorize]
[Route("api/rentals")]
public class RentalsController : ControllerBase
{
    private readonly IMediator _mediator;
    public RentalsController(IMediator mediator) => _mediator = mediator;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Reserve a free book copy.</summary>
    /// <response code="201">Rental created with status Reserved.</response>
    /// <response code="400">Librarian inactive, copy already rented, or rental limit reached.</response>
    /// <response code="403">Not a registered member.</response>
    /// <response code="404">Librarian or copy not found.</response>
    /// <response code="409">Someone else reserved the copy at the same moment.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Member)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reserve(ReserveBookRequest request)
    {
        var id = await _mediator.Send(
            new ReserveBookCommand(UserId, request.LibrarianId, request.BookCopyId, request.Notes));
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>The logged-in member's rentals.</summary>
    [HttpGet("my")]
    [Authorize(Roles = Roles.Member)]
    [ProducesResponseType(typeof(List<RentalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<RentalDto>>> My() =>
        Ok(await _mediator.Send(new GetMyRentalsQuery(UserId)));

    /// <summary>Get one rental (its member, its librarian, or an admin).</summary>
    /// <response code="403">Not related to this rental.</response>
    /// <response code="404">Rental not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RentalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalDto>> GetById(Guid id) =>
        Ok(await _mediator.Send(new GetRentalByIdQuery(id, UserId, User.IsInRole(Roles.Admin))));

    /// <summary>Cancel my reservation (only while it is still Reserved).</summary>
    /// <response code="204">Cancelled. The copy is free again.</response>
    /// <response code="400">The rental is no longer Reserved.</response>
    /// <response code="403">Not your rental.</response>
    /// <response code="404">Rental not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Member)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _mediator.Send(new CancelRentalCommand(id, UserId));
        return NoContent();
    }

    /// <summary>Move a rental forward: Reserved → CheckedOut → Returned (its librarian only).</summary>
    /// <response code="204">Status updated.</response>
    /// <response code="400">Invalid transition.</response>
    /// <response code="403">Not your rental.</response>
    /// <response code="404">Rental not found.</response>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = Roles.Librarian)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateRentalStatusRequest request)
    {
        await _mediator.Send(new UpdateRentalStatusCommand(id, UserId, request.Status));
        return NoContent();
    }
}
