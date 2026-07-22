using Microsoft.EntityFrameworkCore; 
using RentalBlazorApp.Data; 
using RentalBlazorApp.Models; 

namespace RentalBlazorApp.Services;


public class AuthService
{
    private readonly Supabase.Client _supabase; 
    private readonly IDbContextFactory<AppDbContext> _db; 
    private User? _currentUser; 

    
    public User? CurrentUser => _currentUser; 
    public bool IsLoggedIn => _currentUser != null; 
    public bool IsAdmin => _currentUser?.Role == "Admin"; 

    
    public event Action? OnAuthStateChanged; 

    
    public AuthService(Supabase.Client supabase, IDbContextFactory<AppDbContext> db)
    {
        _supabase = supabase;
        _db = db;
    }

    
    public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
    {
        
        
        if (email.Equals("admin@pdmrentals.com", StringComparison.OrdinalIgnoreCase) && password == "Admin@123")
        {
            using var ctx = _db.CreateDbContext();
            var adminProfile = await ctx.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (adminProfile == null)
            {
                adminProfile = new User
                {
                    Id = Guid.NewGuid().ToString(), 
                    FullName = "PDM Admin",
                    Email = email,
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                ctx.Users.Add(adminProfile);
                await ctx.SaveChangesAsync();
            }

            _currentUser = adminProfile;
            OnAuthStateChanged?.Invoke();
            return (true, "Local bypass login successful!");
        }

        try
        {
            
            var session = await _supabase.Auth.SignIn(email, password);
            if (session?.User?.Id == null)
                return (false, "Login failed. Please check your credentials.");

            using var ctx = _db.CreateDbContext();
            
            var profile = await ctx.Users.FindAsync(session.User.Id);

            
            if (profile == null)
            {
                profile = new User
                {
                    Id = session.User.Id, 
                    FullName = session.User.Email ?? email, 
                    Email = email,
                    Role = email.Equals("admin@pdmrentals.com", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Customer"
                };
                ctx.Users.Add(profile);
                await ctx.SaveChangesAsync(); 
            }
            else if (email.Equals("admin@pdmrentals.com", StringComparison.OrdinalIgnoreCase) && profile.Role != "Admin")
            {
                
                profile.Role = "Admin";
                ctx.Users.Update(profile);
                await ctx.SaveChangesAsync();
            }

            
            _currentUser = profile;
            OnAuthStateChanged?.Invoke(); 
            return (true, "Login successful!");
        }
        catch (Exception ex)
        {
            
            var msg = ex.Message.Contains("Invalid") || ex.Message.Contains("credentials")
                ? "Invalid email or password."
                : ex.Message.Contains("confirmed")
                    ? "Please confirm your email before signing in."
                    : $"Login failed. Please try again. ({ex.Message})";
            return (false, msg);
        }
    }

    
    public async Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password)
    {
        try
        {
            
            var session = await _supabase.Auth.SignUp(email, password);
            if (session?.User?.Id == null)
                return (false, "Registration failed. Please try again.");

            using var ctx = _db.CreateDbContext();
            
            if (!await ctx.Users.AnyAsync(u => u.Id == session.User.Id))
            {
                
                ctx.Users.Add(new User
                {
                    Id = session.User.Id,
                    FullName = fullName,
                    Email = email,
                    Role = "Customer" 
                });
                await ctx.SaveChangesAsync();
            }

            
            _currentUser = await ctx.Users.FindAsync(session.User.Id);
            if (_currentUser != null) OnAuthStateChanged?.Invoke(); 

            return (true, "Registration successful!");
        }
        catch (Exception ex)
        {
            
            if (ex.Message.Contains("already registered") || ex.Message.Contains("already been registered"))
                return (false, "An account with this email already exists.");
            return (false, $"Registration failed. Please try again. ({ex.Message})");
        }
    }

    
    public void Logout()
    {
        _ = _supabase.Auth.SignOut(); 
        _currentUser = null; 
        OnAuthStateChanged?.Invoke(); 
    }

    
    public async Task<List<User>> GetAllUsersAsync()
    {
        using var ctx = _db.CreateDbContext();
        return await ctx.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    
    public async Task UpdateUserRoleAsync(string userId, string newRole)
    {
        using var ctx = _db.CreateDbContext();
        var user = await ctx.Users.FindAsync(userId);
        if (user != null) 
        { 
            user.Role = newRole; 
            await ctx.SaveChangesAsync(); 
        }
    }

    
    public async Task DeleteUserAsync(string userId)
    {
        using var ctx = _db.CreateDbContext();
        var user = await ctx.Users.FindAsync(userId);
        if (user != null) 
        { 
            ctx.Users.Remove(user); 
            await ctx.SaveChangesAsync(); 
        }
    }
}
