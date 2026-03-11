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
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
