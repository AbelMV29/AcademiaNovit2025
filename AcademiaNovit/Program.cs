using AcademiaNovit;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region configuracion del Serilog

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

# endregion

#region leer variables de entorno y secrets

builder.Configuration.AddEnvironmentVariables();

string connectionString;

var secretFilePath = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING_FILE");

if (!string.IsNullOrEmpty(secretFilePath) && File.Exists(secretFilePath))
{
    connectionString = File.ReadAllText(secretFilePath).Trim();
    Console.WriteLine($"Connection string loaded from Docker secret: {secretFilePath}");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    Console.WriteLine("Connection string loaded from appsettings.json (local development)");
}

#endregion


builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddOpenApi();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.MapControllers();

#region keep alive endpoint

app.MapGet("/keep-alive", () => new
{
    status = "alive",
    timestamp = DateTime.UtcNow
});

#endregion

app.Run();

public partial class Program { } // This partial class is required for the WebApplicationFactory to work properly in tests. 