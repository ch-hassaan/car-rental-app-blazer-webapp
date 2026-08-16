using RentalBlazorApp.Models;

namespace RentalBlazorApp.Services.Interfaces;

public interface IMonthlyReportService
{
    Task<MonthlyReportData?> GetMonthlyReportDataAsync(int year, int month);
}
