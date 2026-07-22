using Microsoft.EntityFrameworkCore; 
using RentalBlazorApp.Models; 

namespace RentalBlazorApp.Data; 


public class AppDbContext : DbContext
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    
    public DbSet<Car>     Cars     => Set<Car>();    
    public DbSet<Booking> Bookings => Set<Booking>(); 
    public DbSet<User>    Users    => Set<User>();    

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        
        modelBuilder.Entity<Car>()
            .Property(c => c.Status)
            .HasConversion<int>(); 

        
        modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion<int>(); 
    }
}
