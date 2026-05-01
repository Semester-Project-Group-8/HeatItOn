using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Services;
using Backend.Hubs;
//using Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add SignalIR
builder.Services.AddSignalR();

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
builder.Services.AddScoped<SourceService>();
builder.Services.AddScoped<AssetsService>();
builder.Services.AddScoped<OptimizerService>();
builder.Services.AddScoped<ResultService>();
builder.Services.AddScoped<ResultListService>();
builder.Services.AddScoped<OptimizedResultsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add SignalIR
app.MapHub<BackendHub>("/datahub");

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope()) 
{ 
    var db = scope.ServiceProvider.GetRequiredService<BackendDbContext>();
    db.Database.Migrate();
}

app.Run();