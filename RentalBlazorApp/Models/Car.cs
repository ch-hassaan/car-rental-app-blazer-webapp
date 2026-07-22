using System.ComponentModel.DataAnnotations; 

namespace RentalBlazorApp.Models; 


public enum CarStatus
{
    Available, 
    Booked,    
    Rented     
}


public class Car
{
    [Key] 
    public string Id { get; set; } = Guid.NewGuid().ToString(); 

    public string Name { get; set; } = string.Empty; 
    public string Category { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty; 
    public int Price { get; set; } 
    public string ImageUrl { get; set; } = string.Empty; 
    public CarStatus Status { get; set; } = CarStatus.Available; 

    
    public string Seats { get; set; } = string.Empty; 
    public string Fuel { get; set; } = string.Empty; 
    public string Transmission { get; set; } = string.Empty; 
    public string Mileage { get; set; } = string.Empty; 
    public string Condition { get; set; } = string.Empty; 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
}
