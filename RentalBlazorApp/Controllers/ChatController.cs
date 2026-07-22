using Microsoft.AspNetCore.Mvc;
using RentalBlazorApp.Models.AI;
using RentalBlazorApp.Services.AI.Interfaces;

namespace RentalBlazorApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public sealed class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    
    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger      = logger;
    }

    
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        
        
        throw new NotImplementedException("ChatController.SendMessage will be implemented in the next phase.");
    }

    
    [HttpDelete("session/{sessionId}")]
    public async Task<IActionResult> ResetSession(string sessionId)
    {
        
        
        throw new NotImplementedException("ChatController.ResetSession will be implemented in the next phase.");
    }
}
