using Microsoft.EntityFrameworkCore; 
using RentalBlazorApp.Data; 
using RentalBlazorApp.Models; 

namespace RentalBlazorApp.Services;


public class BookingService
{
    
    private readonly IDbContextFactory<AppDbContext> _db;

    
    public BookingService(IDbContextFactory<AppDbContext> db) => _db = db;

    
    public async Task<List<Booking>> GetAllAsync()
    {
        using var ctx = _db.CreateDbContext(); 
        
        return await ctx.Bookings.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    
    public async Task<List<Booking>> GetByUserAsync(string userId)
    {
        using var ctx = _db.CreateDbContext();
        
        return await ctx.Bookings.Where(b => b.UserId == userId).OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    
    public async Task AddAsync(Booking booking)
    {
        using var ctx = _db.CreateDbContext();
        ctx.Bookings.Add(booking); 
        await ctx.SaveChangesAsync(); 
    }

    
    public async Task UpdateStatusAsync(string id, BookingStatus status)
    {
        using var ctx = _db.CreateDbContext();
        var b = await ctx.Bookings.FindAsync(id); 
        if (b != null) 
        { 
            b.Status = status; 
            await ctx.SaveChangesAsync(); 
        }
    }

    
    public async Task DeleteAsync(string id)
    {
        using var ctx = _db.CreateDbContext();
        var b = await ctx.Bookings.FindAsync(id); 
        if (b != null) 
        { 
            ctx.Bookings.Remove(b); 
            await ctx.SaveChangesAsync(); 
        }
    }

    
    public async Task<int> GetPendingCountAsync()
    {
        using var ctx = _db.CreateDbContext();
        
        return await ctx.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
    }

    
    public async Task<int> GetTotalRevenueAsync()
    {
        using var ctx = _db.CreateDbContext();
        return await ctx.Bookings
            
            .Where(b => b.Status == BookingStatus.Approved || b.Status == BookingStatus.Completed)
            
            .SumAsync(b => b.TotalAmount);
    }
}
