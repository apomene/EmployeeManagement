using EmployeeManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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
var app = builder.Build();

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

app.Run();
