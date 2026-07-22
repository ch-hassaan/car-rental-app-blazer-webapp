using RentalBlazorApp.Models.AI;

namespace RentalBlazorApp.Services.AI.Interfaces;


public interface IGroqService
{
    
    
    Task<GroqResponse?> SendAsync(GroqRequest request, CancellationToken cancellationToken = default);
}
