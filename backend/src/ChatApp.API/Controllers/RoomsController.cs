using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController(IRoomService roomService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetMyRooms(CancellationToken ct)
    {
        var rooms = await roomService.GetUserRoomsAsync(GetUserId(), ct);
        return Ok(rooms);
    }

    [HttpPost]
    public async Task<ActionResult<RoomDto>> CreateRoom(
        [FromBody] CreateRoomRequest req, CancellationToken ct)
    {
        var room = await roomService.CreateRoomAsync(GetUserId(), req, ct);
        return CreatedAtAction(nameof(GetMyRooms), new { id = room.Id }, room);
    }

    [HttpPost("{roomId:guid}/join")]
    public async Task<IActionResult> JoinRoom(Guid roomId, CancellationToken ct)
    {
        await roomService.JoinRoomAsync(GetUserId(), roomId, ct);
        return NoContent();
    }

    [HttpDelete("{roomId:guid}/leave")]
    public async Task<IActionResult> LeaveRoom(Guid roomId, CancellationToken ct)
    {
        await roomService.LeaveRoomAsync(GetUserId(), roomId, ct);
        return NoContent();
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")!);
}