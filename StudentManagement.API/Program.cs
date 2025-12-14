
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudentManagement.API.Data;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load user secrets for local development if available
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
}

// Basic runtime checks for required secrets/config to give clearer errors
var defaultConnCheck = builder.Configuration.GetConnectionString("DefaultConnection");
if (!builder.Environment.IsEnvironment("Testing") && string.IsNullOrWhiteSpace(defaultConnCheck))
{
    throw new InvalidOperationException("Database connection string 'ConnectionStrings:DefaultConnection' is not configured. Set it via environment variables or 'dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<connection>\"'.");
}

var jwtKeyCheck = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKeyCheck) || Encoding.UTF8.GetBytes(jwtKeyCheck).Length < 32)
{
    throw new InvalidOperationException("JWT key is not configured or too short. Set 'Jwt:Key' via environment variable 'Jwt__Key' or via 'dotnet user-secrets set \"Jwt:Key\" \"<your-long-key>\"'. The key should be at least 32 bytes.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    
    if (builder.Environment.IsEnvironment("Testing"))
    {
        var testConn = builder.Configuration.GetConnectionString("TestConnection");
        options.UseSqlite(testConn ?? "DataSource=:memory:");
    }
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();


var seedData = builder.Configuration.GetValue<bool>("Seed:Enabled", false);
if (builder.Environment.IsDevelopment() || seedData)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    SeedData.EnsureSeededAsync(db, config).GetAwaiter().GetResult();
}


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
