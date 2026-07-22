using Microsoft.EntityFrameworkCore; 
using RentalBlazorApp.Components; 
using RentalBlazorApp.Configuration; 
using RentalBlazorApp.Data; 
using RentalBlazorApp.Models; 
using RentalBlazorApp.Services; 
using RentalBlazorApp.Services.AI; 
using RentalBlazorApp.Services.AI.Interfaces; 
#nullable enable

using Supabase; 


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorComponents()
    
    .AddInteractiveServerComponents();


builder.Services.AddDbContextFactory<AppDbContext>(opts =>
    
    opts.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


var supabaseUrl = builder.Configuration["Supabase:Url"]!;
var supabaseKey = builder.Configuration["Supabase:AnonKey"]!;


builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(supabaseUrl, supabaseKey,
        
        new SupabaseOptions { AutoRefreshToken = false, AutoConnectRealtime = false }));


builder.Services.AddSingleton<CarService>(); 
builder.Services.AddSingleton<BookingService>(); 


builder.Services.AddScoped<AuthService>(); 


builder.Services.Configure<GroqSettings>(
    builder.Configuration.GetSection(GroqSettings.SectionName));


builder.Services.AddHttpClient<IGroqService, GroqService>("GroqClient", (serviceProvider, client) =>
{
    var settings = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<GroqSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.ApiKey);
    client.Timeout = TimeSpan.FromSeconds(30);
});


builder.Services.AddScoped<IPromptService, PromptService>();
builder.Services.AddScoped<IChatService, ChatService>();


builder.Services.AddControllers();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

    
    using (var ctx = dbFactory.CreateDbContext())
        await ctx.Database.MigrateAsync();

    
    var carSvc = scope.ServiceProvider.GetRequiredService<CarService>();
    await carSvc.SeedCarsAsync();

    
    using var seedCtx = dbFactory.CreateDbContext();
    
    bool adminExists = await seedCtx.Users.AnyAsync(u => u.Email == "admin@pdmrentals.com");
    if (!adminExists)
    {
        
        var seedClient = new Supabase.Client(supabaseUrl, supabaseKey,
            new SupabaseOptions { AutoRefreshToken = false, AutoConnectRealtime = false });
        await seedClient.InitializeAsync();

        Supabase.Gotrue.Session? session = null;

        
        try { session = await seedClient.Auth.SignUp("admin@pdmrentals.com", "Admin@123"); }
        catch {  }

        
        if (session?.User?.Id == null)
        {
            try { session = await seedClient.Auth.SignIn("admin@pdmrentals.com", "Admin@123"); }
            catch {  }
        }

        
        if (session?.User?.Id != null)
        {
            
            seedCtx.Users.Add(new User
            {
                Id        = session.User.Id, 
                FullName  = "PDM Admin",
                Email     = "admin@pdmrentals.com",
                Role      = "Admin", 
                CreatedAt = DateTime.UtcNow
            });
            
            await seedCtx.SaveChangesAsync();
            Console.WriteLine("[PDM] Admin user seeded successfully."); 
        }
        else
        {
            
            Console.WriteLine("[PDM] WARNING: Could not seed admin — disable 'Confirm email' in Supabase Auth settings, then restart.");
        }
    }
}


if (!app.Environment.IsDevelopment())
{
    
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();


app.MapControllers();


app.MapRazorComponents<App>()
    
    .AddInteractiveServerRenderMode();


app.Run();
