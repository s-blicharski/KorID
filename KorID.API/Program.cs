using System.IdentityModel.Tokens.Jwt;
using System.Text;
using KorID.API.Services;
using KorID.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();



// Add services to the container.
builder.Services.AddDataServices(builder.Configuration, "koriddb");

// Register authentication service
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Bez mapowania krótkich nazw claimów na długie URI -> "role" zostaje "role".
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

            // Kluczowe dla [Authorize(Roles="...")] i User.Identity.Name:
            RoleClaimType = "role",
            NameClaimType = JwtRegisteredClaimNames.UniqueName, // "unique_name"

            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// Configure controllers and JSON options to avoid object cycles when returning EF entities
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Konfiguracja CORS (pozwalamy na dostęp z frontendu)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => 
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Rejestracja serwisu hashowania haseł
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

// Włącza symulację rozproszonego cache (Redis) w pamięci RAM
builder.Services.AddDistributedMemoryCache(); 

// Poniżej powinieneś już mieć swój QrCodeService z wcześniejszych kroków
builder.Services.AddScoped<QrCodeService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// IMPORTANT: ensure CORS runs before any middleware that can short-circuit (like HTTPS redirect)
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
