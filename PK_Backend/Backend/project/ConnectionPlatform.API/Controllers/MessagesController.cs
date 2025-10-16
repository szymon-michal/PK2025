using Microsoft.AspNetCore.Mvc;
using ConnectionPlatform.Core.DTOs;
using ConnectionPlatform.Core.Interfaces;

namespace ConnectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IConversationService _conversationService;

    public MessagesController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<MessageDto>> EditMessage(
        long id, 
        [FromQuery] int userId, 
        [FromBody] EditMessageDto editDto)
    {
        try
        {
            var message = await _conversationService.EditMessageAsync(id, userId, editDto.Content);
            if (message == null)
                return NotFound();

            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(long id, [FromQuery] int userId)
    {
        try
        {
            var success = await _conversationService.DeleteMessageAsync(id, userId);
            if (!success)
                return NotFound();

            return Ok(new { message = "Message deleted successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("{id}/read")]
    public async Task<ActionResult> MarkAsRead(long id, [FromQuery] int userId)
    {
        var success = await _conversationService.MarkMessageAsReadAsync(id, userId);
        if (!success)
            return NotFound();

        return Ok(new { message = "Message marked as read" });
    }
}

public class EditMessageDto
{
    public string Content { get; set; } = string.Empty;
}