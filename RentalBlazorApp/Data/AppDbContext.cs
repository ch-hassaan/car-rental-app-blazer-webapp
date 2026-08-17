using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
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
        base.OnModelCreating(modelBuilder);
        
        // MongoDB collection mappings
        modelBuilder.Entity<Car>().ToCollection("cars");
        modelBuilder.Entity<Booking>().ToCollection("bookings");
        modelBuilder.Entity<User>().ToCollection("users");

        // Note: MongoDB natively stores enums as strings or integers.
        // We will keep the HasConversion to store them as ints to match existing SQLite behavior.
        modelBuilder.Entity<Car>()
            .Property(c => c.Status)
            .HasConversion<int>(); 

        modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion<int>(); 
    }
}
