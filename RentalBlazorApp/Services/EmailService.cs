using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using RentalBlazorApp.Configuration;
using RentalBlazorApp.Models;
using RentalBlazorApp.Services.Interfaces;

namespace RentalBlazorApp.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendBookingConfirmationAsync(Booking booking, byte[]? pdfAttachment = null)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(booking.FullName, booking.Email));
            message.Subject = "PDM Rentals – Booking Confirmation";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h2>Hello {booking.FullName},</h2>
                    <p>Thank you for choosing PDM Rentals! Your booking has been successfully created.</p>
                    
                    <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                        <tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Booking ID</td><td style='padding: 8px; border: 1px solid #ddd;'>{booking.Id.Substring(0, 8).ToUpper()}</td></tr>
                        <tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Vehicle</td><td style='padding: 8px; border: 1px solid #ddd;'>{booking.CarName}</td></tr>
                        <tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Duration</td><td style='padding: 8px; border: 1px solid #ddd;'>{booking.Days} days</td></tr>
                        <tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Total Amount</td><td style='padding: 8px; border: 1px solid #ddd;'>{booking.TotalAmount:N0} Rs</td></tr>
                        <tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Status</td><td style='padding: 8px; border: 1px solid #ddd;'>{booking.Status}</td></tr>
                    </table>

                    <p>We have attached your booking receipt as a PDF to this email.</p>
                    <p>If you have any questions, please contact our support team.</p>
                    
                    <br/>
                    <p>Best regards,<br/><strong>PDM Rentals Team</strong></p>
                </div>"
            };

            if (pdfAttachment != null)
            {
                builder.Attachments.Add($"PDM-Rentals-Booking-{booking.Id.Substring(0, 8).ToUpper()}.pdf", pdfAttachment, new ContentType("application", "pdf"));
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            
            // For development, you might accept all SSL certs, but MailKit is strict by default.
            if (_emailSettings.SmtpServer == "localhost")
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.None);
            }
            else
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                if (!string.IsNullOrEmpty(_emailSettings.SmtpUsername))
                {
                    await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                }
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Booking confirmation email sent successfully to {Email} for Booking {BookingId}", booking.Email, booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking confirmation email to {Email} for Booking {BookingId}", booking.Email, booking.Id);
            throw; // Re-throw to be handled by the caller, or just log and swallow depending on design.
            // The requirement says: "Email failure must NOT corrupt or undo a successful booking. Log the email failure appropriately."
            // So we will throw it here, and catch it in the component/BookingService caller.
        }
    }
}
