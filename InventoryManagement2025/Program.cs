using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using InventoryManagement2025.Data;
using InventoryManagement2025.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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

// -------------------------
// ✅ Identity + Auth
// -------------------------
// Add ASP.NET Core Identity (Users + Roles) using EF Core store
builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        // Dev-friendly password settings; tighten for production
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SchoolInventory>()
    .AddSignInManager();

// Configure JWT Bearer authentication
var jwtKey = builder.Configuration["JwtSettings:Key"] ?? "DevSigningKey_MUST_CHANGE_AtLeast32Chars_Long_123456!";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // OK for local dev
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Add simple Bearer auth support in Swagger UI
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "InventoryManagement2025 API", Version = "v1" });
    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste the JWT token only (Swagger will add 'Bearer ' automatically)"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    // Reference the scheme by name to ensure requirement links correctly
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new List<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

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
        
        // Check if we should force refresh equipment data
        var forceRefresh = builder.Configuration["ForceEquipmentRefresh"];
        if (!string.IsNullOrEmpty(forceRefresh) && forceRefresh.ToLower() == "true")
        {
            Console.WriteLine("🔄 Force refreshing equipment data...");
            DbInit.ForceRefreshEquipment(context);
        }
        else
        {
            DbInit.Initialize(context);
        }
        
        // Seed roles and an initial admin user
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        var adminEmail = builder.Configuration["SeedAdmin:Email"] ?? "admin@school.local";
        var adminPassword = builder.Configuration["SeedAdmin:Password"] ?? "Admin123!";
        var adminRole = "Admin";
        var userRole = "User";

        if (!roleManager.RoleExistsAsync(adminRole).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(adminRole)).GetAwaiter().GetResult();
        }
        if (!roleManager.RoleExistsAsync(userRole).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(userRole)).GetAwaiter().GetResult();
        }

        var existingAdmin = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (existingAdmin == null)
        {
            var adminUser = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, Name = "System Admin" };
            var createResult = userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
            if (createResult.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, adminRole).GetAwaiter().GetResult();
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation($"Seeded admin user '{adminEmail}' with default password. Change the password immediately.");
            }
        }
        else
        {
            // Ensure existing admin is in Admin role
            var inRole = userManager.IsInRoleAsync(existingAdmin, adminRole).GetAwaiter().GetResult();
            if (!inRole)
            {
                userManager.AddToRoleAsync(existingAdmin, adminRole).GetAwaiter().GetResult();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error occurred while creating or seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
app.UseCors("AllowAll");

// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI();

// Only redirect to HTTPS in development (Azure handles SSL termination)
if (app.Environment.IsDevelopment())
{
    // HTTPS redirection handled by Azure in production
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Simple health check endpoint
app.MapGet("/health", (IWebHostEnvironment env) => Results.Json(new { status = "ok", environment = env.EnvironmentName }));

app.Run();
