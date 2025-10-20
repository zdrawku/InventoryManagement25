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
        DbInit.Initialize(context);
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
            var adminUser = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var createResult = userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
            if (createResult.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, adminRole).GetAwaiter().GetResult();
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation($"Seeded admin user '{adminEmail}' with default password. Change the password immediately.");
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
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
    // CORS active in production
    //app.UseCors("AllowAll"); 
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Minimal auth endpoints (register/login) for testing
app.MapPost("/api/auth/register", async (UserManager<AppUser> userManager, RegisterRequest request) =>
{
    var user = new AppUser { UserName = request.Email, Email = request.Email };
    var result = await userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded)
        return Results.BadRequest(result.Errors);

    return Results.Ok(new { user.Id, user.Email });
});

app.MapPost("/api/auth/login", async (UserManager<AppUser> userManager, IConfiguration config, LoginRequest request) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user == null) return Results.Unauthorized();
    var valid = await userManager.CheckPasswordAsync(user, request.Password);
    if (!valid) return Results.Unauthorized();

    var jwtKeyLocal = config["JwtSettings:Key"] ?? "DevSigningKey_MUST_CHANGE_AtLeast32Chars_Long_123456!";
    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var tokenKey = Encoding.ASCII.GetBytes(jwtKeyLocal);
    var roles = await userManager.GetRolesAsync(user);
    var claims = new List<System.Security.Claims.Claim>
    {
        new System.Security.Claims.Claim("id", user.Id ?? string.Empty),
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email ?? string.Empty)
    };
    foreach (var r in roles)
    {
        claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, r));
    }
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(12),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new { Token = tokenString });
});

// Who am I endpoint to verify token contents
app.MapGet("/api/auth/me", [Microsoft.AspNetCore.Authorization.Authorize] (System.Security.Claims.ClaimsPrincipal user) =>
{
    var id = user.FindFirst("id")?.Value;
    var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    var roles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray();
    return Results.Ok(new { id, email, roles });
});

// Simple health check endpoint
app.MapGet("/health", (IWebHostEnvironment env) => Results.Json(new { status = "ok", environment = env.EnvironmentName }));

app.Run();
