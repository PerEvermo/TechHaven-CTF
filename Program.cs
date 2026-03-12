using SecretWebsite.Components;
using SecretWebsite.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<FlagService>();
builder.Services.AddScoped<AuthStateService>();

var app = builder.Build();

// Ensure DB is initialised on startup
app.Services.GetRequiredService<DatabaseService>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// CTF Challenge 3: inject a secret HTTP response header on /about
// Students must open DevTools → Network tab → inspect the /about response headers
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/about"))
    {
        context.Response.OnStarting(() =>
        {
            // FLAG 3: HEADER_03
            context.Response.Headers.Append("X-Debug-Token", "FLAG3: HEADER_03");
            return Task.CompletedTask;
        });
    }
    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();

// FLAG 13: APIDEBUG_13 — exposed debug endpoint leaking internal information
// Students discover this by exploring API paths or reading source code
app.MapGet("/api/debug", () => Results.Json(new
{
    version = "2.3.1-dev",
    environment = "production",
    database = "SQLite",
    debug_mode = true,
    internal_token = "FLAG13: APIDEBUG_13",
    note = "This endpoint should never be exposed in production.",
    timestamp = DateTime.UtcNow,
}));

// FLAG 15: APIKEY_15 — protected internal endpoint
// The API key is found in /backup/config.txt (Flag 14 challenge)
// Students must chain the two flags: find the backup file → extract the key → call this endpoint
app.MapGet("/api/internal", (HttpContext ctx) =>
{
    var key = ctx.Request.Headers["X-API-Key"].FirstOrDefault();
    if (key == "th-internal-4829af")
    {
        return Results.Json(new
        {
            status = "authenticated",
            flag = "FLAG15: APIKEY_15",
            message = "You used the API key from the exposed backup file to authenticate this internal endpoint.",
        });
    }
    return Results.Json(new
    {
        status = "unauthorized",
        error = "Valid X-API-Key header required.",
        hint = "The key might be stored somewhere on this server…",
    });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
