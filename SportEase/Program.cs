using Microsoft.EntityFrameworkCore;
using SportEase.Web.Data;
using SportEase.Web.Repositories.Interfaces;
using SportEase.Web.Repositories.Implementations;
using SportEase.Web.Services.Interfaces;
using SportEase.Web.Services.Implementations;
using Microsoft.Data.SqlClient; // Add this

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// TEST: Display the connection string
Console.WriteLine(" CONNECTION STRING BEING USED:");
Console.WriteLine(connectionString);
Console.WriteLine();

// Test connection before configuring DbContext
try
{
    using var testConnection = new SqlConnection(connectionString);
    await testConnection.OpenAsync();
    Console.WriteLine(" Successfully connected to SQL Server!");

    var cmd = new SqlCommand("SELECT @@SERVERNAME, DB_NAME()", testConnection);
    using var reader = await cmd.ExecuteReaderAsync();
    if (reader.Read())
    {
        Console.WriteLine($" Server: {reader[0]}");
        Console.WriteLine($" Database: {reader[1]}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($" Connection test failed: {ex.Message}");
    Console.WriteLine($" Make sure SQL Server Express is running");
    Console.WriteLine($" Run 'services.msc' and check 'SQL Server (SQLEXPRESS)' is running");
}

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlServerOptions.CommandTimeout(60); // Increase timeout
        }
    ));

// Configure Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "SportEase.Session";
});

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITerrainRepository, TerrainRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITerrainService, TerrainService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

// Configure HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// CORRECT ORDER:

app.UseStaticFiles();  
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

// Initialize Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        // This creates the database if it doesn't exist
        await dbContext.Database.EnsureCreatedAsync();
        Console.WriteLine(" Database initialization successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Database initialization failed: {ex.Message}");
    }
}

app.Run();