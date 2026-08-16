using RentalBlazorApp.Models;

namespace RentalBlazorApp.Services.Interfaces;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(Booking booking, byte[]? pdfAttachment = null);
}
