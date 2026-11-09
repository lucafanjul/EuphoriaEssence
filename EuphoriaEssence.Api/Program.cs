using System.Text;
using EuphoriaEssence.Api.Data;
using EuphoriaEssence.Api.Models;
using EuphoriaEssence.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<SwaggerAuthTransformer>();
});

var connectionString = builder.Configuration
    .GetConnectionString("EuphoriaEssenceDB")
    ?? throw new InvalidOperationException(
        "No se encontró la conexión EuphoriaEssenceDB.");

builder.Services.AddDbContext<EuphoriaEssenceDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<
    IPasswordHasher<Usuarios>,
    PasswordHasher<Usuarios>>();

builder.Services.AddScoped<JwtService>();

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32 ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Revisá Jwt:Key, Jwt:Issuer y Jwt:Audience.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Preparar la carpeta antes de construir la aplicación.
var carpetaUploads = Path.Combine(
    builder.Environment.ContentRootPath,
    "wwwroot",
    "uploads");

Directory.CreateDirectory(carpetaUploads);

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Euphoria Essence API v1");

        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run(    );