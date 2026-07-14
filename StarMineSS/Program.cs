using Microsoft.AspNetCore.HttpOverrides;
using StarMineSS.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddMemoryCache(o => o.SizeLimit = 1024);
builder.Services.AddSingleton<GameStore>();
builder.Services.AddSingleton<HighwayStore>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// თუ ოდესმე რევერს-პროქსის მიღმა გექნება (IIS/Nginx)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors();

// Static Files ერთხელ რეგისტრირდება Cache-Control ჰედერებით
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "index.html" }
});
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        ctx.Context.Response.Headers["Pragma"] = "no-cache";
        ctx.Context.Response.Headers["Expires"] = "0";
    }
});

app.MapControllers();

// ნებისმიერი უცნობი GET გზა დააბრუნოს index.html (SPA fallback)
app.MapFallbackToFile("index.html");

app.Run();