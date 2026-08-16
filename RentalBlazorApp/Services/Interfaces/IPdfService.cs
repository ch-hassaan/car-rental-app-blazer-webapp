using RentalBlazorApp.Models;

namespace RentalBlazorApp.Services.Interfaces;

public interface IPdfService
{
    byte[] GenerateBookingReceipt(Booking booking);
    byte[] GenerateMonthlyReport(MonthlyReportData data);
}
