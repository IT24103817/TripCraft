using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;
using TripCraft.Api.Middleware;
using TripCraft.Api.Setup;
using TripCraft.Application;
using TripCraft.Infrastructure;
using TripCraft.Infrastructure.Persistence;
using TripCraft.Infrastructure.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Structured console logging with request context. Never log tokens or passwords.
// JSON lines outside Development (Render collects stdout); readable text when developing.
builder.Host.UseSerilog((context, logger) =>
{
    logger.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext();
    if (context.HostingEnvironment.IsDevelopment())
        logger.WriteTo.Console();
    else
        logger.WriteTo.Console(new RenderedCompactJsonFormatter());
});

var jwtSettings = AuthenticationSetup.ReadJwtSettings(builder.Configuration);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, jwtSettings);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddProblemDetails();

builder.Services.AddJwtAuthentication(jwtSettings);
builder.Services.AddLoginRateLimiting();
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddSwaggerWithJwt();

// Render (and Vercel/Neon-style hosting) terminates HTTPS at a proxy. Trust the one proxy hop in front of the
// app so Request.Scheme is https and RemoteIpAddress is the client (the login rate limit is per client IP).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages(); // ProblemDetails bodies for bare 401/403/404/429 responses
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsSetup.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    // RUN_MIGRATIONS=true (Render, Docker): bring the schema up to date before serving. Locally you run
    // `dotnet ef database update` instead. The seeder is idempotent: each part only runs when its table is empty.
    if (string.Equals(app.Configuration["RUN_MIGRATIONS"], "true", StringComparison.OrdinalIgnoreCase))
    {
        Log.Information("RUN_MIGRATIONS=true: applying database migrations");
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }
    await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
}

app.Run();

// Lets WebApplicationFactory<Program> in the test project find this class.
public partial class Program;
