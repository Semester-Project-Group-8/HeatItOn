using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Services;
//using Backend.Data;

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
builder.Services.AddScoped<SourceService>();
builder.Services.AddScoped<AssetsService>();
builder.Services.AddScoped<ResultService>();
builder.Services.AddScoped<ResultListService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope()) 
{ 
    var db = scope.ServiceProvider.GetRequiredService<BackendDbContext>();
    db.Database.Migrate();

    string csvPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "heating.csv");
    string assetCsvPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "assets.csv");

    var demandService = scope.ServiceProvider.GetRequiredService<SourceService>();
    var assetsService = scope.ServiceProvider.GetRequiredService<AssetsService>();
    ReadCsv importer = new ReadCsv(demandService, csvPath);
    ReadAssetCsv assetImporter = new ReadAssetCsv(assetsService, assetCsvPath);

    var insertedSources = await importer.ImportCsv();
    var insertedAssets = await assetImporter.ImportCsv();
}

app.Run();