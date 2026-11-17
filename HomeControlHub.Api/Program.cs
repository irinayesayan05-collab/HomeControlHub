using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policy => policy.AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

var devices = new ConcurrentDictionary<int, SmartDevice>();

devices[1] = new SmartDevice(1, "Living Room Sensor", "Living Room", true);
devices[2] = new SmartDevice(2, "Bedroom Sensor", "Bedroom", true);
devices[3] = new SmartDevice(3, "Kitchen Sensor", "Kitchen", false);

var startedAt = DateTime.UtcNow;

app.MapGet("/", () => "HomeControlHub API is running");

app.MapGet("/api/devices", () =>
{
    var result = devices.Values
        .OrderBy(d => d.Id)
        .Select(d => new
        {
            d.Id,
            d.Name,
            d.Room,
            d.IsOnline,
            Temperature = d.IsOnline ? d.Temperature : (double?)null,
            UptimeSeconds = d.IsOnline
                ? (int)(DateTime.UtcNow - startedAt).TotalSeconds
                : 0
        });

    return Results.Ok(result);
});

app.MapGet("/api/summary", () =>
{
    return Results.Ok(new
    {
        Total = devices.Count,
        Online = devices.Values.Count(d => d.IsOnline),
        Offline = devices.Values.Count(d => !d.IsOnline)
    });
});

app.MapPost("/api/devices/{id:int}/toggle", (int id) =>
{
    if (!devices.TryGetValue(id, out var device))
        return Results.NotFound(new { message = "Device not found" });

    lock (device)
    {
        device.IsOnline = !device.IsOnline;
    }

    return Results.Ok(device);
});

app.Run();

class SmartDevice
{
    public int Id { get; }
    public string Name { get; }
    public string Room { get; }
    public bool IsOnline { get; set; }
    public double Temperature { get; }

    public SmartDevice(int id, string name, string room, bool online)
    {
        Id = id;
        Name = name;
        Room = room;
        IsOnline = online;
        Temperature = 20 + id * 1.5;
    }
}
