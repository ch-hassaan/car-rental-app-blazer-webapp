using System.ComponentModel.DataAnnotations; 

namespace RentalBlazorApp.Models; 


public enum BookingStatus
{
    Pending,   
    Approved,  
    Rejected,  
    Completed  
}


public class Booking
{
    [Key] 
    public string Id { get; set; } = Guid.NewGuid().ToString(); 

    public string? UserId { get; set; } 

    public string CarId { get; set; } = string.Empty; 
    public string CarName { get; set; } = string.Empty; 

    
    public string FullName { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty; 
    public string Phone { get; set; } = string.Empty; 
    public string LicenseNo { get; set; } = string.Empty; 
    public string Address { get; set; } = string.Empty; 

    
    public int Days { get; set; } = 1; 
    public int DailyRate { get; set; } 
    public int TotalAmount { get; set; } 

    public string PaymentMethod { get; set; } = string.Empty; 

    public BookingStatus Status { get; set; } = BookingStatus.Pending; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
}
