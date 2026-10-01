using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Identity;
using TripCraft.Application.Identity.Services;
using TripCraft.Application.Quotations;
using TripCraft.Application.Quotations.Dashboard;
using TripCraft.Application.Quotations.Documents;
using TripCraft.Application.Quotations.Reports;
using TripCraft.Application.Quotations.Services;
using TripCraft.Application.Resources.Services;
using TripCraft.Application.Trips.Services;
using TripCraft.Application.Trips.Templates;
using TripCraft.Application.Vouchers;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Services;

namespace TripCraft.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();

        // v1.1: in-app notifications and the email outbox
        services.AddScoped<INotifier, Notifier>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailDispatcher, EmailDispatcher>();
        services.AddScoped<ISettingsService, SettingsService>();

        // Component A — Trip Requests & Itinerary
        services.AddScoped<ITripRequestService, TripRequestService>();
        services.AddScoped<IItineraryEditService, ItineraryEditService>();
        services.AddScoped<ITripPlanningService, TripPlanningService>();
        services.AddScoped<IAttractionService, AttractionService>();
        services.AddScoped<IPassportPhotoService, PassportPhotoService>();
        services.AddScoped<ITripTemplateService, TripTemplateService>();

        // Component B — Resource Management. ResourceCatalog and ResourceHoldService are also registered as the
        // workflow ports (IResourceCatalog, IResourceHoldService) in Infrastructure/Workflows/WorkflowsSetup.
        services.AddScoped<ResourceCatalog>();
        services.AddScoped<ResourceHoldService>();
        services.AddScoped<IGuideService, GuideService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IResourceHoldAdminService, ResourceHoldAdminService>();
        services.AddScoped<IGuideScheduleService, GuideScheduleService>();
        services.AddScoped<IGuideChangeService, GuideChangeService>();
        services.AddScoped<ITripAssignmentService, TripAssignmentService>();
        services.AddScoped<IAvailabilityGridService, AvailabilityGridService>();

        // Agent workflow integration and the approval gate
        services.AddSingleton<ProposalValidator>();
        services.AddScoped<ProposalFactsLoader>();
        services.AddScoped<IProposalEditService, ProposalEditService>();
        services.AddScoped<IPlanExplanationService, PlanExplanationService>();
        services.AddScoped<IWorkflowStepService, WorkflowStepService>();
        services.AddScoped<IWorkflowProposalService, WorkflowProposalService>();
        services.AddScoped<IWorkflowQueryService, WorkflowQueryService>();
        services.AddScoped<IQuotationApprovalService, QuotationApprovalService>();
        services.AddScoped<IQuotationClientService, QuotationClientService>();
        services.AddScoped<ITripConfirmationService, TripConfirmationService>();
        services.AddScoped<IVoucherService, VoucherService>();

        // Component C — Quotation, Approval & Reporting. QuotationStore is also the IQuotationStore workflow port
        // (registered in Infrastructure/Workflows/WorkflowsSetup).
        services.AddScoped<QuotationStore>();
        services.AddScoped<IQuotationService, QuotationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITripDocumentService, TripDocumentService>();

        return services;
    }
}
