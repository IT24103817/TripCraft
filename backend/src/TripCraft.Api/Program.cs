using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using Serilog;
using TripCraft.Api.Middleware;
using TripCraft.Api.Setup;
using TripCraft.Application;
using TripCraft.Infrastructure;
using TripCraft.Infrastructure.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Structured console logging with request context. Never log tokens or passwords.
builder.Host.UseSerilog((context, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

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

var app = builder.Build();

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
    await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
}

app.Run();

// Lets WebApplicationFactory<Program> in the test project find this class.
public partial class Program;
