using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Identity;
using TripCraft.Application.Quotations;
using TripCraft.Application.Quotations.Documents;
using TripCraft.Application.Quotations.Reports;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Templates;
using TripCraft.Application.Vouchers;
using TripCraft.Application.Workflows;
using TripCraft.Infrastructure.External;
using TripCraft.Infrastructure.Identity;
using TripCraft.Infrastructure.Persistence;
using TripCraft.Infrastructure.Persistence.Auditing;
using TripCraft.Infrastructure.Persistence.Notifications;
using TripCraft.Infrastructure.Persistence.Reporting;
using TripCraft.Infrastructure.Persistence.Seeding;
using TripCraft.Infrastructure.Quotations;
using TripCraft.Infrastructure.Resources;
using TripCraft.Infrastructure.Trips;
using TripCraft.Infrastructure.Vouchers;
using TripCraft.Infrastructure.Workflows;

namespace TripCraft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, JwtSettings jwtSettings)
    {
        var databaseUrl = configuration["DATABASE_URL"];
        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.IsNullOrWhiteSpace(databaseUrl))
                throw new InvalidOperationException(
                    "DATABASE_URL is not set. Add it with 'dotnet user-secrets set DATABASE_URL ...' or an environment variable.");

            options.UseNpgsql(ConnectionStringParser.ToNpgsql(databaseUrl))
                   .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IDatabaseHealth, DatabaseHealth>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IAuditLogReader, AuditLogReader>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITripRequestRepository, TripRequestRepository>();
        services.AddScoped<IAttractionRepository, AttractionRepository>();
        services.AddScoped<ITripTemplateRepository, TripTemplateRepository>();
        services.AddSingleton<IPassportPhotoStore, LocalPassportPhotoStore>();
        services.AddScoped<IAgentWorkflowRepository, AgentWorkflowRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IQuotationRepository, QuotationRepository>();
        services.AddScoped<IReportQueries, ReportQueries>();

        // v1.1: notifications, email, vouchers and operator settings
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IVoucherRepository, VoucherRepository>();
        services.AddSingleton<IVoucherPdfRenderer, QuestPdfVoucherRenderer>();
        services.AddSingleton<IItineraryPdfRenderer, QuestPdfItineraryRenderer>();
        // Created on first use, so the API still starts without the key; issuing or scanning a voucher then fails.
        services.AddSingleton(_ => new VoucherSigner(configuration["VOUCHER_SIGNING_KEY"] ?? string.Empty));
        // Configuration defaults; an Admin's saved Settings row overrides them, read again on every request.
        var settingsDefaults = new TripSettings(
            int.TryParse(configuration["CANCELLATION_CUTOFF_DAYS"], out var cutoff) && cutoff >= 0
                ? cutoff
                : TripSettings.DefaultCancellationCutoffDays,
            configuration["OPERATOR_CONTACT"] is { Length: > 0 } contact ? contact : TripSettings.DefaultOperatorContact,
            configuration["OPERATOR_TIME_ZONE"] is { Length: > 0 } zone ? zone : TripSettings.DefaultTimeZoneId);
        services.AddScoped(sp => SettingsRepository.Load(sp.GetRequiredService<AppDbContext>(), settingsDefaults));
        services.AddScoped<ISettingsRepository, SettingsRepository>();

        services.AddWorkflows(configuration);
        services.AddExternalServices(configuration);
        services.AddScoped<DataSeeder>();

        services.AddSingleton(jwtSettings);
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
