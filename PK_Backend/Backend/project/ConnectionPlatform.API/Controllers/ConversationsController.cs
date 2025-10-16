using Microsoft.AspNetCore.Mvc;
using ConnectionPlatform.Core.DTOs;
using ConnectionPlatform.Core.Interfaces;

namespace ConnectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;

    public ConversationsController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [HttpPost("direct/{otherUserId}")]
    public async Task<ActionResult<ConversationDto>> CreateDirectConversation(
        int otherUserId, 
        [FromQuery] int userId)
    {
        try
        {
            var conversation = await _conversationService.CreateDirectConversationAsync(userId, otherUserId);
            return Ok(conversation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations([FromQuery] int userId)
    {
        var conversations = await _conversationService.GetUserConversationsAsync(userId);
        return Ok(conversations);
    }

    [HttpPost("{id}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage(
        long id, 
        [FromQuery] int senderId, 
        [FromBody] CreateMessageDto messageDto)
    {
        try
        {
            var message = await _conversationService.SendMessageAsync(id, senderId, messageDto);
            return CreatedAtAction(nameof(GetMessages), new { id, cursor = message.Id }, message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("{id}/messages")]
    public async Task<ActionResult<List<MessageDto>>> GetMessages(
        long id, 
        [FromQuery] int userId,
        [FromQuery] long cursor = 0, 
        [FromQuery] int limit = 20)
    {
        try
        {
            var messages = await _conversationService.GetConversationMessagesAsync(id, userId, cursor, limit);
            return Ok(messages);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}