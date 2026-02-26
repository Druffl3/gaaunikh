using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new HealthResponse("healthy", DateTimeOffset.UtcNow));
});

app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    var indexPath = Path.Combine(app.Environment.WebRootPath, "index.html");
    if (!File.Exists(indexPath))
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new { error = "frontend_not_built" });
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(indexPath);
});

app.Run();

internal sealed record HealthResponse(string Status, DateTimeOffset TimestampUtc);

public partial class Program;
