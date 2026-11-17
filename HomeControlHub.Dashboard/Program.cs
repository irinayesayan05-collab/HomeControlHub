var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("devices", client =>
{
    client.BaseAddress = new Uri("http://localhost:5100");
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/overview", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("devices");

    try
    {
        var devices = await client.GetFromJsonAsync<object[]>("/api/devices");
        return Results.Ok(devices);
    }
    catch
    {
        return Results.Problem("Unable to connect to device API");
    }
});

app.MapPost("/api/toggle/{id:int}", async (int id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("devices");

    try
    {
        var response = await client.PostAsync($"/api/devices/{id}/toggle", null);
        return Results.StatusCode((int)response.StatusCode);
    }
    catch
    {
        return Results.Problem("Unable to connect to device API");
    }
});

app.Run();
