using Apex.Api.Middleware;
using Apex.Application.DTOs;
using Apex.Application.Interfaces;
using Apex.Application.Mappings;
using Apex.Application.Services;
using Apex.Application.Validators;
using Apex.Domain.Interfaces;
using Apex.Infrastructure.BackgroundJobs;
using Apex.Infrastructure.Data;
using Apex.Infrastructure.Identity;
using Apex.Infrastructure.Persistence.Context;
using Apex.Infrastructure.Persistence.Repositories;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Setup Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/apex-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// 1. Database & Infrastructure
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 2. Repositories & The Decorator
// Register the base implementation first
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Apply the Decorator (Scrutor handles the "wrapping" logic automatically)
builder.Services.Decorate<IProductRepository, CachedProductService>();

// 3. Caching & Background Jobs
builder.Services.AddDistributedMemoryCache();

builder.Services.AddHostedService<SalesStatisticsWorker>();

// 4. Application Services
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // Grab all the validation errors
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        // Wrap them in your professional ErrorResponse DTO
        var errorResponse = new ErrorResponse(
            400,
            "Validation Failed",
            string.Join(" | ", errors)
        );

        return new BadRequestObjectResult(errorResponse);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version")
    );
}).AddMvc();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
// This is the "Modern" way to add security without instantiating abstract classes
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true);
builder.Services.AddScoped<IAuthService, AuthService>();

// 5. Validation & Mapping
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PlaceOrderRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

var app = builder.Build();

// 6. Database Seeding Logic
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        Console.WriteLine("--> Database Seeding Started...");
        await DbInitializer.SeedAsync(context);
        Console.WriteLine("--> Database Seeding Completed!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"--> CRITICAL STARTUP ERROR: {ex.Message}");
    throw;
}

// 7. Middleware Pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<PerformanceHeaderMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();