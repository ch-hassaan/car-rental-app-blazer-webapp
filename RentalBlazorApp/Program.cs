using Microsoft.EntityFrameworkCore; 
using RentalBlazorApp.Components; 
using RentalBlazorApp.Configuration; 
using RentalBlazorApp.Data; 
using RentalBlazorApp.Models; 
using RentalBlazorApp.Services; 
using RentalBlazorApp.Services.AI; 
using RentalBlazorApp.Services.AI.Interfaces; 
using RentalBlazorApp.Services.Interfaces;
#nullable enable

using Supabase; 


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorComponents()
    
    .AddInteractiveServerComponents();


var mongoConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "mongodb://localhost:27017";
var mongoDbName = builder.Configuration["MongoDbSettings:DatabaseName"] ?? "pdmrentals";

builder.Services.AddDbContextFactory<AppDbContext>(opts =>
    opts.UseMongoDB(mongoConnectionString, mongoDbName));


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

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMonthlyReportService, MonthlyReportService>();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;


builder.Services.AddControllers();

// ── HttpClient for Blazor Server components (ChatWindow) ──────────────────────
builder.Services.AddHttpClient("BlazorSelf")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });

// Register a default HttpClient that Blazor components can @inject.
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("BlazorSelf"));


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    
    try 
    {
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

        // For MongoDB, we can just ensure the database/collections are ready
        using (var ctx = dbFactory.CreateDbContext())
            await ctx.Database.EnsureCreatedAsync();

        
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
    catch (Exception ex)
    {
        Console.WriteLine($"[STARTUP ERROR] {ex.Message}");
        app.MapGet("/startup-error", () => ex.ToString());
    }
}

// Debug endpoint to verify configuration is loaded (remove after testing)
app.MapGet("/debug-config", (IConfiguration cfg) => new {
    GroqApiKeyPresent = !string.IsNullOrWhiteSpace(cfg["Groq:ApiKey"]),
    GroqApiKeyLength  = cfg["Groq:ApiKey"]?.Length ?? 0,
    GroqModel         = cfg["Groq:ModelName"],
    GroqBaseUrl       = cfg["Groq:BaseUrl"],
    MongoConnPresent  = !string.IsNullOrWhiteSpace(cfg.GetConnectionString("DefaultConnection"))
});

// Debug endpoint: makes a real Groq API call and returns raw result or error
app.MapGet("/test-groq", async (IConfiguration cfg) => {
    var apiKey  = cfg["Groq:ApiKey"]  ?? "";
    var baseUrl = cfg["Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1/";
    var model   = cfg["Groq:ModelName"] ?? "llama-3.1-8b-instant";

    using var client = new System.Net.Http.HttpClient();
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    client.Timeout = TimeSpan.FromSeconds(30);

    var body = System.Text.Json.JsonSerializer.Serialize(new {
        model,
        messages = new[] { new { role = "user", content = "Say hello in one word." } },
        max_tokens = 10
    });

    try
    {
        var response = await client.PostAsync(
            baseUrl.TrimEnd('/') + "/chat/completions",
            new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        var raw = await response.Content.ReadAsStringAsync();
        return Results.Ok(new { StatusCode = (int)response.StatusCode, Body = raw });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { Error = ex.GetType().Name, Message = ex.Message });
    }
});


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
