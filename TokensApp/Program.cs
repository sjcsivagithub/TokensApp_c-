using Microsoft.EntityFrameworkCore;
using TokensApp.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Render.com: bind to dynamic $PORT (fallback 8080 for local) ───────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ── Register PostgreSQL DbContext ──────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TokensDb")));

// ── Add Controllers & Swagger ───────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new() { Title = "Tokens App API", Version = "v1" });
});

var app = builder.Build();

// ── Enable Swagger in Development ──────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred during request pipeline execution.");
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
        }
    }
});

// Skip HTTPS redirect in Production — Render.com terminates TLS externally
if (!app.Environment.IsProduction())
    app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ── Startup DB connectivity test ──────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db  = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        db.Database.EnsureCreated();
        log.LogInformation("[STARTUP] Database connected and schema verified OK.");
    }
    catch (Exception ex)
    {
        // This message will appear in the Render Logs tab — shows the REAL connection error
        log.LogCritical(ex, "[STARTUP] FAILED to connect to database: {Message}", ex.Message);
    }
}

app.Run();
