using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<BackendDbContext>(
    options =>
    {
        options.UseMySql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            new MySqlServerVersion(new Version(8, 0, 45)),
            mySqlOptions => { mySqlOptions.EnableRetryOnFailure(); }
            );
    }
);
builder.Services.AddScoped<DemandService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();