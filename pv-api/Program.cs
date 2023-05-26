using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PvDbContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));
var app = builder.Build();

app.MapPost("/installations", async (PvDbContext dbContext, AddPvInstallationDto pvInstallationDto) =>
{
    var newPvInstallation = new PvInstallation
    {
        Longitude = pvInstallationDto.Longitude,
        Latitude = pvInstallationDto.Latitude,
        Address = pvInstallationDto.Address,
        OwnerName = pvInstallationDto.OwnerName,
        Comments = pvInstallationDto.Comments,
    };
    await dbContext.PvInstallations.AddAsync(newPvInstallation);
    await dbContext.SaveChangesAsync();

    return Results.Ok(newPvInstallation.ID);
});

app.MapPost("/installations/{id}/deactivate", async (int id, PvDbContext dbContext) =>
{
    var installation = await dbContext.PvInstallations.FirstOrDefaultAsync(i => i.ID == id);
    if (installation is null)
    {
        return Results.NotFound();
    }

    installation.IsActive = false;
    await dbContext.SaveChangesAsync();

    return Results.Ok(installation);
});

app.MapPost("/installations/{id}/reports", async (int id, PvDbContext dbContext, AddProductionReportDto productionReportDto) =>
{
    DateTime currTime = DateTime.UtcNow;
    DateTime truncTime = DateTime.UtcNow.AddSeconds(-currTime.Second).AddMilliseconds(-currTime.Millisecond);

    var newProductionReport = new ProductionReport
    {
        Timestamp = truncTime,
        ProducedWattage = productionReportDto.ProducedWattage,
        HouseholdWattage = productionReportDto.HouseholdWattage,
        BatteryWattage = productionReportDto.BatteryWattage,
        GridWattage = productionReportDto.GridWattage,
        PvInstallationId = id
    };

    var installation = await dbContext.PvInstallations.FirstOrDefaultAsync(i => i.ID == id);

    if (installation == null)
    {
        return Results.NotFound();
    }

    await dbContext.ProductionReports.AddAsync(newProductionReport);
    await dbContext.SaveChangesAsync();

    return Results.Ok(installation);
});

app.MapGet("/installations/{id}/reports", async (int id, DateTime timestamp, int duration, PvDbContext dbContext) =>
{
    var installation = await dbContext.PvInstallations.FirstOrDefaultAsync(i => i.ID == id);

    if (installation == null)
    {
        return Results.NotFound();
    }

    var endTimeStamp = timestamp.AddMinutes(duration);

    float producedWattageSum = await dbContext.ProductionReports.
        Where(r => r.PvInstallationId == id && r.Timestamp >= timestamp && r.Timestamp <= endTimeStamp).
        SumAsync(r => r.ProducedWattage);

    return Results.Ok(producedWattageSum);
});

app.Run();

record AddProductionReportDto(float ProducedWattage, float HouseholdWattage, float BatteryWattage, float GridWattage);
record AddPvInstallationDto(float Longitude, float Latitude, string Address, string OwnerName, string? Comments);

class PvInstallation
{
    public int ID { get; set; }
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    [MaxLength(1024)]
    public string Address { get; set; } = "";
    [MaxLength(512)]
    public string OwnerName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public string? Comments { get; set; } = "";

    [JsonIgnore]
    public List<ProductionReport> ProductionReports { get; set; } = new();
}

class ProductionReport
{
    public int ID { get; set; }
    public DateTime Timestamp { get; set; }
    public float ProducedWattage { get; set; }
    public float HouseholdWattage { get; set; }
    public float BatteryWattage { get; set; }
    public float GridWattage { get; set; }

    public int PvInstallationId { get; set; }
    [JsonIgnore]
    public PvInstallation? PvInstallation { get; set; }
}

class PvDbContext : DbContext
{
    public PvDbContext(DbContextOptions<PvDbContext> options) : base(options) { }
    public DbSet<PvInstallation> PvInstallations => Set<PvInstallation>();
    public DbSet<ProductionReport> ProductionReports => Set<ProductionReport>();
}