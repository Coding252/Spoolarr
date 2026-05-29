using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<FilamentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISpoolRepository, SpoolRepository>();
builder.Services.AddScoped<IPrinterRepository, PrinterRepository>();
builder.Services.AddScoped<IPrintJobRepository, PrintJobRepository>();
builder.Services.AddScoped<INfcTagRepository, NfcTagRepository>();
builder.Services.AddScoped<ISpoolService, SpoolService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FilamentDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        db.Database.Migrate();
        await SeedData.InitialiseAsync(db);
    }
    catch (Exception ex) when (ex is InvalidOperationException or IOException)
    {
        logger.LogError(ex, "Database file could not be created or accessed. Check the connection string and file permissions.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred applying migrations. The application will continue but the database may be out of date.");
    }
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
