using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RentalBlazorApp.Models;
using RentalBlazorApp.Services.Interfaces;

namespace RentalBlazorApp.Services;

public class PdfService : IPdfService
{
    public byte[] GenerateBookingReceipt(Booking booking)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(x => ComposeContent(x, booking));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("PDM Rentals").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text("Booking Confirmation / Rental Receipt").FontSize(14).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(100).Height(50).Placeholder(); // Could be a logo here
        });
    }

    private void ComposeContent(IContainer container, Booking booking)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(20);

            column.Item().Row(row =>
            {
                row.RelativeItem().Component(new AddressComponent("Customer Details", new[]
                {
                    booking.FullName,
                    booking.Email,
                    booking.Phone,
                    $"License No: {booking.LicenseNo}",
                    booking.Address
                }));

                row.ConstantItem(50);

                row.RelativeItem().Component(new AddressComponent("Booking Details", new[]
                {
                    $"Booking ID: {booking.Id.Substring(0, 8).ToUpper()}",
                    $"Date: {booking.CreatedAt:dd MMM yyyy HH:mm}",
                    $"Status: {booking.Status}"
                }));
            });

            column.Item().Element(x => ComposeTable(x, booking));

            var totalPrice = booking.TotalAmount;
            column.Item().AlignRight().Text($"Total Amount: {totalPrice:N0} Rs").FontSize(14).SemiBold();
        });
    }

    private void ComposeTable(IContainer container, Booking booking)
    {
        container.Table(table =>
        {
            // step 1
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            // step 2
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Vehicle");
                header.Cell().Element(CellStyle).AlignRight().Text("Daily Rate");
                header.Cell().Element(CellStyle).AlignRight().Text("Duration");
                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            // step 3
            table.Cell().Element(CellStyle).Text(booking.CarName);
            table.Cell().Element(CellStyle).AlignRight().Text($"{booking.DailyRate:N0} Rs");
            table.Cell().Element(CellStyle).AlignRight().Text($"{booking.Days} days");
            table.Cell().Element(CellStyle).AlignRight().Text($"{booking.TotalAmount:N0} Rs");

            static IContainer CellStyle(IContainer container)
            {
                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
            x.Span(" of ");
            x.TotalPages();
        });
    }

    public byte[] GenerateMonthlyReport(MonthlyReportData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(x => ComposeReportHeader(x, data));
                page.Content().Element(x => ComposeReportContent(x, data));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeReportHeader(IContainer container, MonthlyReportData data)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("PDM Rentals").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text("Monthly Sales & Revenue Report").FontSize(14).FontColor(Colors.Grey.Medium);
            });

            row.RelativeItem().AlignRight().Column(column =>
            {
                column.Item().AlignRight().Text($"Report Period: {data.ReportMonth}").FontSize(12).SemiBold();
                column.Item().AlignRight().Text($"Generated on: {data.GeneratedOn:MMM dd, yyyy HH:mm}");
                column.Item().AlignRight().Text("Admin/Company: PDM Rentals");
            });
        });
    }

    private void ComposeReportContent(IContainer container, MonthlyReportData data)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(20);

            // Statistics and Financial Summary
            column.Item().Row(row =>
            {
                row.RelativeItem().Component(new AddressComponent("Booking Statistics", new[]
                {
                    $"Total Bookings: {data.TotalBookings}",
                    $"Completed/Confirmed: {data.CompletedBookings}",
                    $"Cancelled Bookings: {data.CancelledBookings}",
                    $"Pending Bookings: {data.PendingBookings}"
                }));

                row.ConstantItem(50);

                row.RelativeItem().Component(new AddressComponent("Financial Summary", new[]
                {
                    $"Gross Sales: {data.GrossRevenue:N0} Rs",
                    $"Average Booking Value: {data.AverageBookingValue:N0} Rs",
                    $"Highest-Value Booking: {data.HighestValueBooking:N0} Rs",
                    $"Total Rental Days: {data.TotalRentalDays}"
                }));
            });

            // Vehicle Performance Table
            if (data.VehiclePerformances.Any())
            {
                column.Item().PaddingBottom(5).Text("Vehicle Performance").FontSize(14).SemiBold();
                column.Item().Element(x => ComposeVehiclePerformanceTable(x, data));
            }

            // Daily Revenue Table
            if (data.DailyRevenues.Any())
            {
                column.Item().PaddingBottom(5).Text("Daily Revenue").FontSize(14).SemiBold();
                column.Item().Element(x => ComposeDailyRevenueTable(x, data));
            }

            // Detailed Bookings Table
            if (data.Bookings.Any())
            {
                column.Item().PaddingBottom(5).Text("Booking Breakdown").FontSize(14).SemiBold();
                column.Item().Element(x => ComposeDetailedBookingsTable(x, data));
            }

            // Total Gross Revenue at the bottom
            column.Item().PaddingTop(20).AlignRight().Text($"Total Gross Revenue: {data.GrossRevenue:N0} Rs").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
        });
    }

    private void ComposeVehiclePerformanceTable(IContainer container, MonthlyReportData data)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Vehicle");
                header.Cell().Element(CellStyle).AlignRight().Text("Rentals");
                header.Cell().Element(CellStyle).AlignRight().Text("Rental Days");
                header.Cell().Element(CellStyle).AlignRight().Text("Revenue");

                static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
            });

            foreach (var vp in data.VehiclePerformances)
            {
                table.Cell().Element(CellStyle).Text(vp.VehicleName);
                table.Cell().Element(CellStyle).AlignRight().Text($"{vp.NumberOfRentals}");
                table.Cell().Element(CellStyle).AlignRight().Text($"{vp.RentalDays}");
                table.Cell().Element(CellStyle).AlignRight().Text($"{vp.Revenue:N0} Rs");
            }

            static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        });
    }

    private void ComposeDailyRevenueTable(IContainer container, MonthlyReportData data)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Date");
                header.Cell().Element(CellStyle).AlignRight().Text("Bookings");
                header.Cell().Element(CellStyle).AlignRight().Text("Revenue");

                static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
            });

            foreach (var dr in data.DailyRevenues)
            {
                table.Cell().Element(CellStyle).Text(dr.Date.ToString("dd MMM yyyy"));
                table.Cell().Element(CellStyle).AlignRight().Text($"{dr.BookingsCount}");
                table.Cell().Element(CellStyle).AlignRight().Text($"{dr.Revenue:N0} Rs");
            }

            static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        });
    }

    private void ComposeDetailedBookingsTable(IContainer container, MonthlyReportData data)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2); // ID
                columns.RelativeColumn(3); // Customer
                columns.RelativeColumn(3); // Vehicle
                columns.RelativeColumn(2); // Date
                columns.RelativeColumn(1); // Days
                columns.RelativeColumn(2); // Status
                columns.RelativeColumn(2); // Amount
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("ID");
                header.Cell().Element(CellStyle).Text("Customer");
                header.Cell().Element(CellStyle).Text("Vehicle");
                header.Cell().Element(CellStyle).Text("Date");
                header.Cell().Element(CellStyle).AlignRight().Text("Days");
                header.Cell().Element(CellStyle).Text("Status");
                header.Cell().Element(CellStyle).AlignRight().Text("Amount");

                static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
            });

            foreach (var b in data.Bookings)
            {
                table.Cell().Element(CellStyle).Text(b.Id.Substring(0, 8).ToUpper());
                table.Cell().Element(CellStyle).Text(b.FullName);
                table.Cell().Element(CellStyle).Text(b.CarName);
                table.Cell().Element(CellStyle).Text(b.CreatedAt.ToString("dd MMM yyyy"));
                table.Cell().Element(CellStyle).AlignRight().Text($"{b.Days}");
                table.Cell().Element(CellStyle).Text(b.Status.ToString());
                table.Cell().Element(CellStyle).AlignRight().Text($"{b.TotalAmount:N0} Rs");
            }

            static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        });
    }
}

public class AddressComponent : IComponent
{
    private string Title { get; }
    private string[] Lines { get; }

    public AddressComponent(string title, string[] lines)
    {
        Title = title;
        Lines = lines;
    }

    public void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(2);

            column.Item().BorderBottom(1).PaddingBottom(5).Text(Title).SemiBold();

            foreach (var line in Lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    column.Item().Text(line);
            }
        });
    }
}
