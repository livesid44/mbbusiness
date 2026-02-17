var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Enable static files (for serving index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

// First API endpoint - receives message from query string
app.MapGet("/api/first", (string? message) =>
{
    var response = new
    {
        success = true,
        timestamp = DateTime.UtcNow,
        message = message ?? "Default message",
        source = "First API",
        description = "This response was generated from the query string parameter"
    };
    return Results.Json(response);
});

// Second API endpoint - called from button click
app.MapGet("/api/second", () =>
{
    var response = new
    {
        success = true,
        timestamp = DateTime.UtcNow,
        message = "This is the second API response",
        source = "Second API",
        description = "This response was generated from a button click",
        randomNumber = new Random().Next(1, 1000)
    };
    return Results.Json(response);
});

app.Run();
