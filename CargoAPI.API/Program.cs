using CargoAPI.API.Middleware;
using CargoAPI.Business.Services;
using CargoAPI.DataAccess;
using CargoAPI.DataAccess.Repositories;
using Hangfire;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICarrierConfigurationRepository, CarrierConfigurationRepository>();

builder.Services.AddScoped<ICarrierService, CarrierService>();
builder.Services.AddScoped<ICarrierConfigurationService, CarrierConfigurationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICarrierReportService, CarrierReportService>();

// Hangfire configuration using same SQL Server connection
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHangfireServer();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handling - must be before other middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// Register recurring job - runs every hour
RecurringJob.AddOrUpdate<ICarrierReportService>(
    "carrier-reports",
    service => service.GenerateReportsAsync(),
    Cron.Hourly,
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

app.MapControllers();

app.Run();
