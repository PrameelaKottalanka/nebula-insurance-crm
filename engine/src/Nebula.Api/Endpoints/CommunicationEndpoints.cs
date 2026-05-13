using FluentValidation;
using Nebula.Api.Helpers;
using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;
using Nebula.Application.Services;

namespace Nebula.Api.Endpoints;

public static class CommunicationEndpoints
{
    public static IEndpointRouteBuilder MapCommunicationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/communications")
            .WithTags("Communications")
            .RequireAuthorization()
            .RequireRateLimiting("authenticated");

        group.MapPost("/", LogCommunication);
        group.MapGet("/", ListCommunications);
        group.MapGet("/{communicationId:guid}", GetCommunication);
        group.MapPut("/{communicationId:guid}", EditCommunication);
        group.MapPost("/{communicationId:guid}/redact", RedactCommunication);
        group.MapPost("/{communicationId:guid}/follow-up", CreateFollowUp);

        return app;
    }

    private static async Task<IResult> LogCommunication(
        CommunicationLogRequest req,
        IValidator<CommunicationLogRequest> validator,
        CommunicationEventService svc,
        ICurrentUserService user,
        IAuthorizationService authz,
        CancellationToken ct)
    {
        var authorized = false;
        foreach (var role in user.Roles)
        {
            if (await authz.AuthorizeAsync(role, "communication", "create"))
            { authorized = true; break; }
        }
        if (!authorized) return ProblemDetailsHelper.PolicyDenied();

        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return ProblemDetailsHelper.ValidationError(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var (dto, error) = await svc.LogAsync(req, user, ct);
        return error switch
        {
            "entity_not_found" => ProblemDetailsHelper.NotFound("Entity", req.PrimaryEntityId),
            _ => Results.Created($"/communications/{dto!.Id}", dto),
        };
    }

    private static async Task<IResult> ListCommunications(
        string entityType,
        Guid entityId,
        int? page,
        int? pageSize,
        CommunicationEventService svc,
        ICurrentUserService user,
        IAuthorizationService authz,
        CancellationToken ct)
    {
        var authorized = false;
        foreach (var role in user.Roles)
        {
            if (await authz.AuthorizeAsync(role, "communication", "read"))
            { authorized = true; break; }
        }
        if (!authorized) return ProblemDetailsHelper.PolicyDenied();

        var (dto, error) = await svc.ListByEntityAsync(
            entityType, entityId,
            page ?? 1, pageSize ?? 25,
            user, ct);

        return error switch
        {
            "entity_not_found" => ProblemDetailsHelper.NotFound("Entity", entityId),
            _ => Results.Ok(dto),
        };
    }

    private static async Task<IResult> GetCommunication(
        Guid communicationId,
        CommunicationEventService svc,
        ICurrentUserService user,
        IAuthorizationService authz,
        CancellationToken ct)
    {
        var authorized = false;
        foreach (var role in user.Roles)
        {
            if (await authz.AuthorizeAsync(role, "communication", "read"))
            { authorized = true; break; }
        }
        if (!authorized) return ProblemDetailsHelper.PolicyDenied();

        var (dto, error) = await svc.GetByIdAsync(communicationId, user, ct);
        return error switch
        {
            "not_found" => ProblemDetailsHelper.NotFound("CommunicationEvent", communicationId),
            _ => Results.Ok(dto),
        };
    }

    private static async Task<IResult> EditCommunication(
        Guid communicationId,
        CommunicationEditRequest req,
        IValidator<CommunicationEditRequest> validator,
        CommunicationEventService svc,
        ICurrentUserService user,
        IAuthorizationService authz,
        CancellationToken ct)
    {
        var authorized = false;
        foreach (var role in user.Roles)
        {
            if (await authz.AuthorizeAsync(role, "communication", "edit"))
            { authorized = true; break; }
        }
        if (!authorized) return ProblemDetailsHelper.PolicyDenied();

        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return ProblemDetailsHelper.ValidationError(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var (dto, error) = await svc.EditAsync(communicationId, req, user, ct);
        return error switch
        {
            "not_found"           => ProblemDetailsHelper.NotFound("CommunicationEvent", communicationId),
            "event_redacted"      => ProblemDetailsHelper.EventRedacted(),
            "not_author"          => ProblemDetailsHelper.NotAuthor(),
            "edit_window_expired" => ProblemDetailsHelper.EditWindowExpired(),
            "no_changes"          => ProblemDetailsHelper.NoChanges(),
            _ => Results.Ok(dto),
        };
    }

    private static async Task<IResult> RedactCommunication(
        Guid communicationId,
        CommunicationRedactRequest req,
        IValidator<CommunicationRedactRequest> validator,
        CommunicationEventService svc,
        ICurrentUserService user,
        IAuthorizationService authz,
        CancellationToken ct)
    {
        var authorized = false;
        foreach (var role in user.Roles)
        {
            if (await authz.AuthorizeAsync(role, "communication", "redact"))
            { authorized = true; break; }
        }
        if (!authorized) return ProblemDetailsHelper.PolicyDenied();

        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return ProblemDetailsHelper.ValidationError(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var (dto, error) = await svc.RedactAsync(communicationId, req, user, ct);
        return error switch
        {
            "not_found"       => ProblemDetailsHelper.NotFound("CommunicationEvent", communicationId),
            "already_redacted" => ProblemDetailsHelper.AlreadyRedacted(),
            _ => Results.Ok(dto),
        };
    }

    private static async Task<IResult> CreateFollowUp(
        Guid communicationId,
        CommunicationFollowUpRequest req,
        IValidator<CommunicationFollowUpRequest> validator,
        CommunicationEventService svc,
        ICurrentUserService user,
        IAuthorizationService authz,
        CancellationToken ct)
    {
        var authorized = false;
        foreach (var role in user.Roles)
        {
            if (await authz.AuthorizeAsync(role, "communication", "create"))
            { authorized = true; break; }
        }
        if (!authorized) return ProblemDetailsHelper.PolicyDenied();

        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return ProblemDetailsHelper.ValidationError(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var (dto, error) = await svc.CreateFollowUpAsync(communicationId, req, user, ct);
        return error switch
        {
            "not_found"       => ProblemDetailsHelper.NotFound("CommunicationEvent", communicationId),
            "follow_up_exists" => ProblemDetailsHelper.FollowUpExists(),
            _ => Results.Created($"/tasks/{dto!.Id}", dto),
        };
    }
}
