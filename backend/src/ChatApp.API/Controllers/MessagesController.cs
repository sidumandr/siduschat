using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/rooms/{roomId:guid}/messages")]
[Authorize]
public class MessagesController(IChatService chatService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(
        Guid roomId,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct     = default)
    {
        var messages = await chatService.GetRoomMessagesAsync(roomId, page, pageSize, ct);
        return Ok(messages);
    }

    [HttpDelete("{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId, CancellationToken ct)
    {
        await chatService.DeleteMessageAsync(messageId, GetUserId(), ct);
        return NoContent();
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")!);
}