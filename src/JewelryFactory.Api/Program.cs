using System.Text;
using JewelryFactory.Api.Middleware;
using JewelryFactory.Api.Services;
using JewelryFactory.Application;
using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Infrastructure;
using JewelryFactory.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog (per CLAUDE.md §6) ────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ─── Layers (Clean Architecture) ───────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ─── Current user (HttpContext-based) ──────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ─── JWT Authentication ────────────────────────────────────────────────
var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ─── CORS — Angular dev server ─────────────────────────────────────────
const string CorsPolicy = "AngularApp";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
    .WithOrigins("http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

// ─── Controllers + Swagger ─────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JewelryFactory API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ─── Middleware pipeline (order matters) ───────────────────────────────
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Required so WebApplicationFactory can locate the entry point in tests
public partial class Program { }
