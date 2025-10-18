using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using InventoryManagement2025.Data;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// ✅ Confirm environment
// -------------------------
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"🚀 Starting in environment: {environment}");

// -------------------------
// ✅ Load configuration (automatic by default)
// appsettings.json + appsettings.{Environment}.json
// -------------------------

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings in API responses
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Minimal APIs JSON options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// -------------------------
// ✅ Configure EF Core + SQLite
// -------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"🔗 Using SQLite connection: {connectionString}");

builder.Services.AddDbContext<SchoolInventory>(options =>
    options.UseSqlite(connectionString));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -------------------------
// ✅ Apply migrations & seed DB
// -------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SchoolInventory>();
        Console.WriteLine($"📁 Database file location: {context.Database.GetDbConnection().DataSource}");
        context.Database.Migrate();
        DbInit.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error occurred while creating or seeding the DB.");
    }
}

// -------------------------
// ✅ Middleware pipeline
// -------------------------
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
