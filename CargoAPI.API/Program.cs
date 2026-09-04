using CargoAPI.API.Middleware;
using CargoAPI.Business.Services;
using CargoAPI.DataAccess;
using CargoAPI.DataAccess.Repositories;
using Hangfire;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICarrierConfigurationRepository, CarrierConfigurationRepository>();

builder.Services.AddScoped<ICarrierService, CarrierService>();
builder.Services.AddScoped<ICarrierConfigurationService, CarrierConfigurationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICarrierReportService, CarrierReportService>();

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(connectionString);
});
builder.Services.AddHangfireServer();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard("/hangfire");
}

app.UseMiddleware<GlobalExceptionMiddleware>();

RecurringJob.AddOrUpdate<ICarrierReportService>(
    "carrier-reports",
    service => service.GenerateReportsAsync(),
    Cron.Hourly,
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

app.MapGet("/health/live", () => Results.Ok(new
{
    status = "ok",
    service = "CargoAPI"
}));

app.MapGet("/health/ready", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
{
    if (await dbContext.Database.CanConnectAsync(cancellationToken))
    {
        return Results.Ok(new
        {
            status = "ready",
            database = "reachable"
        });
    }

    return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

app.MapControllers();

app.Run();
