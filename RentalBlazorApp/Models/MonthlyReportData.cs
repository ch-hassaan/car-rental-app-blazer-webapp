using System.Collections.Generic;

namespace RentalBlazorApp.Models;

public class MonthlyReportData
{
    public string ReportMonth { get; set; } = string.Empty;
    public DateTime GeneratedOn { get; set; } = DateTime.Now;

    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }

    public int GrossRevenue { get; set; }
    public int AverageBookingValue { get; set; }
    public int HighestValueBooking { get; set; }
    public int TotalRentalDays { get; set; }

    public List<Booking> Bookings { get; set; } = new();
    public List<VehiclePerformance> VehiclePerformances { get; set; } = new();
    public List<DailyRevenue> DailyRevenues { get; set; } = new();
}

public class VehiclePerformance
{
    public string VehicleName { get; set; } = string.Empty;
    public int NumberOfRentals { get; set; }
    public int RentalDays { get; set; }
    public int Revenue { get; set; }
}

public class DailyRevenue
{
    public DateTime Date { get; set; }
    public int BookingsCount { get; set; }
    public int Revenue { get; set; }
}
