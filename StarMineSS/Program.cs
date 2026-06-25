using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
using StarMineSS.Model;
using StarMineSS.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 👇 1. ვამატებთ კონტროლერების მხარდაჭერას ხვალინდელი დღისთვის
builder.Services.AddControllers();

builder.Services.AddMemoryCache(o => o.SizeLimit = 1024);
builder.Services.AddSingleton<GameStore>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// თუ ოდესმე რევერს-პროქსის მიღმა გექნება (IIS/Nginx)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors();

app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "index.html" }
});

// Static Files ერთხელ რეგისტრირდება Cache-Control ჰედერებით
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => {
        ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        ctx.Context.Response.Headers["Pragma"] = "no-cache";
        ctx.Context.Response.Headers["Expires"] = "0";
    }
});

// 👇 2. ვააქტიურებთ კონტროლერების მარშრუტებს
app.MapControllers();

// ------------------------------------------------------------------
// ეს ძველი Minimal API-ები დროებით რჩება, რომ დღეს თამაშმა იმუშაოს.
// ხვალ, როცა Controllers-ში გადაიტან ამ ლოგიკას, ამ ხაზებს წავშლით!
// ------------------------------------------------------------------

// სწრაფი ჯანმრთელობის ტესტი
app.MapGet("/api/ping", () => Results.Ok(new { ok = true, env = app.Environment.EnvironmentName }));

// ახალი თამაშის შექმნა
app.MapPost("/api/game/new", (int rows, int cols, int mines, GameStore store) =>
{
    var g = GameState.Create(rows, cols, mines);
    store.Save(g);
    return Results.Json(g.ToClient());
});

// უჯრის გახსნა
app.MapPost("/api/game/{id:guid}/reveal", (Guid id, int r, int c, GameStore store) =>
{
    var g = store.Get(id);
    if (g is null) return Results.NotFound(new { error = "not_found" });

    g.Reveal(r, c);
    store.Save(g);
    return Results.Json(g.ToClient());
});

// ნებისმიერი უცნობი GET გზა დააბრუნოს index.html (SPA fallback)
app.MapFallbackToFile("index.html");

app.Run();