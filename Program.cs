using EmployeeManagement.API.Services;
using EmployeeManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using NLog.Web;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Replace default logger with NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1.0", new OpenApiInfo { Title = "Employee API", Version = "v1.0" });
});



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add SQLite database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddSwaggerGen(c =>
{
    // Get XML documentation path
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    // Include XML comments
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});


 // Add MongoDB services

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = MongoClientSettings.FromConnectionString(
        builder.Configuration.GetConnectionString("MongoDb"));
    return new MongoClient(settings);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(builder.Configuration.GetValue<string>("MongoDBName"));
});

builder.Services.AddScoped<IAuditLogger, AuditLogger>();





var app = builder.Build();
app.Use(async (context, next) =>
{
    context.Items["traceId"] = context.TraceIdentifier;
    await next.Invoke();
});


app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Employee API V1.0"); // doc name is v1
    c.RoutePrefix = string.Empty; // Swagger as default page
});


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

try
{

    app.Run();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Application stopped because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}

