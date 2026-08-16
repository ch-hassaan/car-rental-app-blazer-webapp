using Microsoft.EntityFrameworkCore;
using RentalBlazorApp.Data;
using RentalBlazorApp.Models;
using RentalBlazorApp.Services.Interfaces;

namespace RentalBlazorApp.Services;

public class MonthlyReportService : IMonthlyReportService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public MonthlyReportService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<MonthlyReportData?> GetMonthlyReportDataAsync(int year, int month)
    {
        using var ctx = _dbFactory.CreateDbContext();

        // Start and end dates for the month
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        // Fetch all bookings for the specified month
        var bookings = await ctx.Bookings
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();

        if (bookings == null || !bookings.Any())
        {
            return null; // No data available
        }

        var report = new MonthlyReportData
        {
            ReportMonth = startDate.ToString("MMMM yyyy"),
            GeneratedOn = DateTime.Now,
            Bookings = bookings
        };

        // Categorize bookings
        var revenueGeneratingBookings = bookings
            .Where(b => b.Status == BookingStatus.Approved || b.Status == BookingStatus.Completed)
            .ToList();

        report.TotalBookings = bookings.Count;
        report.CompletedBookings = revenueGeneratingBookings.Count;
        report.CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Rejected);
        report.PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending);

        // Financials
        if (revenueGeneratingBookings.Any())
        {
            report.GrossRevenue = revenueGeneratingBookings.Sum(b => b.TotalAmount);
            report.AverageBookingValue = report.GrossRevenue / revenueGeneratingBookings.Count;
            report.HighestValueBooking = revenueGeneratingBookings.Max(b => b.TotalAmount);
            report.TotalRentalDays = revenueGeneratingBookings.Sum(b => b.Days);
        }

        // Vehicle Performance (only based on revenue generating bookings, or all? Let's use revenue generating to be accurate for revenue)
        report.VehiclePerformances = revenueGeneratingBookings
            .GroupBy(b => b.CarName)
            .Select(g => new VehiclePerformance
            {
                VehicleName = g.Key,
                NumberOfRentals = g.Count(),
                RentalDays = g.Sum(b => b.Days),
                Revenue = g.Sum(b => b.TotalAmount)
            })
            .OrderByDescending(v => v.Revenue)
            .ToList();

        // Daily Revenue
        report.DailyRevenues = revenueGeneratingBookings
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new DailyRevenue
            {
                Date = g.Key,
                BookingsCount = g.Count(),
                Revenue = g.Sum(b => b.TotalAmount)
            })
            .OrderBy(d => d.Date)
            .ToList();

        return report;
    }
}
