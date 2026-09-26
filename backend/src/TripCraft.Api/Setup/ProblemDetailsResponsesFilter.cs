using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TripCraft.Api.Controllers.Internal;

namespace TripCraft.Api.Setup;

/// <summary>
/// Documents the RFC 7807 error responses every operation can return, so Swagger shows them without an
/// attribute on each action. Rules (they mirror ExceptionHandlingMiddleware and the auth setup):
/// 500 always; 400 when the operation takes input; 401 when it needs a JWT or the internal key;
/// 403 when it has a role requirement; 404 when the route has an {id}. 409/429/503 are declared on the
/// actions that can return them.
/// </summary>
public class ProblemDetailsResponsesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var internalKey = metadata.OfType<TypeFilterAttribute>().Any(f => f.ImplementationType == typeof(InternalKeyAuthFilter));
        var anonymous = metadata.OfType<IAllowAnonymous>().Any() && !internalKey;
        var hasRoles = metadata.OfType<AuthorizeAttribute>().Any(a => a.Roles is not null || a.Policy is not null);
        var hasInput = context.ApiDescription.ParameterDescriptions.Count > 0;
        var hasId = context.ApiDescription.RelativePath?.Contains("{id") == true;

        if (hasInput) Add(operation, context, "400", "Validation failed");
        if (!anonymous) Add(operation, context, "401", internalKey ? "Missing or wrong X-Internal-Key" : "Missing, expired or invalid JWT");
        if (hasRoles) Add(operation, context, "403", "The caller's role or ownership does not allow this");
        if (hasId) Add(operation, context, "404", "Not found");
        Add(operation, context, "500", "Unexpected error (logged; no details returned)");

        // Every error the API returns is application/problem+json (ExceptionHandlingMiddleware, UseStatusCodePages),
        // including the 409/429/503 declared on actions.
        foreach (var (status, response) in operation.Responses)
        {
            if (status[0] is '4' or '5')
                response.Content = ProblemContent(context);
        }

        if (internalKey)
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "InternalKey" } }] = []
                }
            ];
        }
    }

    private static void Add(OpenApiOperation operation, OperationFilterContext context, string status, string description)
    {
        operation.Responses.TryAdd(status, new OpenApiResponse { Description = description });
    }

    private static Dictionary<string, OpenApiMediaType> ProblemContent(OperationFilterContext context) => new()
    {
        ["application/problem+json"] = new OpenApiMediaType
        {
            Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
        }
    };
}
