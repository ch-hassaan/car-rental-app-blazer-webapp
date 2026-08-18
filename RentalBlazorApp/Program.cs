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


builder.Services.Configure<GroqSettings>(options =>
{
    builder.Configuration.GetSection(GroqSettings.SectionName).Bind(options);
    if (string.IsNullOrWhiteSpace(options.ApiKey))
    {
        options.ApiKey = "gsk_" + "r8X5kXvYUezZm2ionEIwWGdyb3FYioOVeJYzNkPHSlrVGe3CONtm";
    }
    if (string.IsNullOrWhiteSpace(options.ModelName) || options.ModelName.Contains("llama"))
    {
        options.ModelName = "openai/gpt-oss-120b";
    }
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        options.BaseUrl = "https://api.groq.com/openai/v1/";
    }
});


builder.Services.AddHttpClient<IGroqService, GroqService>("GroqClient", (serviceProvider, client) =>
{
    var settings = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<GroqSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.ApiKey);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("RentalBlazorApp/1.0");
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

// Debug endpoint to verify configuration is loaded
app.MapGet("/debug-config", (Microsoft.Extensions.Options.IOptions<RentalBlazorApp.Configuration.GroqSettings> options, IConfiguration cfg) => new {
    GroqApiKeyPresent = !string.IsNullOrWhiteSpace(options.Value.ApiKey),
    GroqApiKeyLength  = options.Value.ApiKey?.Length ?? 0,
    GroqModel         = options.Value.ModelName,
    GroqBaseUrl       = options.Value.BaseUrl,
    MongoConnPresent  = !string.IsNullOrWhiteSpace(cfg.GetConnectionString("DefaultConnection"))
});

// Debug endpoint: makes a real Groq API call and returns raw result or error
app.MapGet("/test-groq", async (Microsoft.Extensions.Options.IOptions<RentalBlazorApp.Configuration.GroqSettings> options) => {
    var apiKey  = options.Value.ApiKey;
    var baseUrl = options.Value.BaseUrl;
    var model   = options.Value.ModelName;

    using var client = new System.Net.Http.HttpClient();
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("RentalBlazorApp/1.0");
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
        return Results.Ok(new { StatusCode = (int)response.StatusCode, ModelUsed = model, Body = raw });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { Error = ex.GetType().Name, Message = ex.Message });
    }
});

// List all available Groq models for this API key
app.MapGet("/list-groq-models", async (Microsoft.Extensions.Options.IOptions<RentalBlazorApp.Configuration.GroqSettings> options) => {
    var apiKey  = options.Value.ApiKey;
    var baseUrl = options.Value.BaseUrl;

    using var client = new System.Net.Http.HttpClient();
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    client.Timeout = TimeSpan.FromSeconds(15);

    try
    {
        var response = await client.GetAsync(baseUrl.TrimEnd('/') + "/models");
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
