using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Identity;
using TripCraft.Application.Identity.Services;
using TripCraft.Application.Quotations;
using TripCraft.Application.Resources.Services;
using TripCraft.Application.Trips.Services;
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

        // Component A — Trip Requests & Itinerary
        services.AddScoped<ITripRequestService, TripRequestService>();
        services.AddScoped<ITripPlanningService, TripPlanningService>();
        services.AddScoped<IAttractionService, AttractionService>();
        services.AddScoped<IPassportPhotoService, PassportPhotoService>();

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

        // Agent workflow integration and the approval gate
        services.AddSingleton<ProposalValidator>();
        services.AddScoped<IWorkflowStepService, WorkflowStepService>();
        services.AddScoped<IWorkflowProposalService, WorkflowProposalService>();
        services.AddScoped<IWorkflowQueryService, WorkflowQueryService>();
        services.AddScoped<IQuotationApprovalService, QuotationApprovalService>();

        return services;
    }
}
