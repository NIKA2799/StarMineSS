using Microsoft.AspNetCore.HttpOverrides;
using StarMineSS.Model;
using StarMineSS.Service;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
var builder = WebApplication.CreateBuilder(args);
//builder.Host.UseWindowsService();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddMemoryCache(o => o.SizeLimit = 1024);
builder.Services.AddSingleton<GameStore>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// თუ ოდესმე რევერს-პროქსის მიღმა გექნება (IIS/Nginx), ეს დაგჭრდება:
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors();
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "index.html" }
});
app.UseStaticFiles();

// სწრაფი ჯანმრთელობის ტესტი
app.MapGet("/api/ping", () => Results.Ok(new { ok = true, env = app.Environment.EnvironmentName }));

// ახალი თამაშის შექმნა
app.MapPost("/api/game/new", (int rows, int cols, int mines, GameStore store) =>
{
    var g = GameState.Create(rows, cols, mines);
    store.Save(g);
    return Results.Json(g.ToClient());
});
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => {
        ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        ctx.Context.Response.Headers["Pragma"] = "no-cache";
        ctx.Context.Response.Headers["Expires"] = "0";
    }
});

// უჯრის გახსნა
app.MapPost("/api/game/{id:guid}/reveal", (Guid id, int r, int c, GameStore store) =>
{
    var g = store.Get(id);
    if (g is null) return Results.NotFound(new { error = "not_found" });

    g.Reveal(r, c);
    store.Save(g);
    return Results.Json(g.ToClient());
    app.UseStaticFiles(new StaticFileOptions {
    OnPrepareResponse = ctx => {
        ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        ctx.Context.Response.Headers["Pragma"] = "no-cache";
        ctx.Context.Response.Headers["Expires"] = "0";
    }
});

});

// ნებისმიერი უცნობი GET გზა დააბრუნოს index.html (SPA fallback)
app.MapFallbackToFile("index.html");

app.Run();