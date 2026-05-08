using FluentValidation;
using Nebula.Application.DTOs;

namespace Nebula.Application.Validators;

public class CommunicationLogRequestValidator : AbstractValidator<CommunicationLogRequest>
{
    private static readonly string[] ValidEventTypes = ["Note", "Call", "Meeting"];
    private static readonly string[] ValidEntityTypes = ["Broker", "Account", "Submission", "Policy", "Renewal"];
    private static readonly string[] CallDirections = ["Inbound", "Outbound"];
    private static readonly string[] AllDirections = ["Inbound", "Outbound", "Internal"];

    public CommunicationLogRequestValidator()
    {
        RuleFor(x => x.EventType)
            .NotEmpty()
            .Must(t => ValidEventTypes.Contains(t))
            .WithMessage($"EventType must be one of: {string.Join(", ", ValidEventTypes)}.");

        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Subject).MaximumLength(200).When(x => x.Subject is not null);
        RuleFor(x => x.Outcome).MaximumLength(1000).When(x => x.Outcome is not null);

        RuleFor(x => x.PrimaryEntityType)
            .NotEmpty()
            .Must(t => ValidEntityTypes.Contains(t))
            .WithMessage($"PrimaryEntityType must be one of: {string.Join(", ", ValidEntityTypes)}.");

        RuleFor(x => x.PrimaryEntityId).NotEmpty();

        RuleFor(x => x.OccurredAt)
            .Must(d => d!.Value <= DateTime.UtcNow)
            .When(x => x.OccurredAt.HasValue)
            .WithMessage("OccurredAt must not be in the future.");

        // Note: Direction and DurationMinutes must be absent
        When(x => x.EventType == "Note", () =>
        {
            RuleFor(x => x.Direction)
                .Null()
                .WithMessage("Direction must not be provided for Note events.");
            RuleFor(x => x.DurationMinutes)
                .Null()
                .WithMessage("DurationMinutes must not be provided for Note events.");
        });

        // Call: Direction required (Inbound|Outbound only), DurationMinutes required
        When(x => x.EventType == "Call", () =>
        {
            RuleFor(x => x.Direction)
                .NotNull()
                .WithMessage("Direction is required for Call events.")
                .Must(d => CallDirections.Contains(d))
                .When(x => x.Direction is not null)
                .WithMessage("Direction for Call must be Inbound or Outbound.");
            RuleFor(x => x.DurationMinutes)
                .NotNull()
                .WithMessage("DurationMinutes is required for Call events.")
                .InclusiveBetween(1, 600)
                .When(x => x.DurationMinutes is not null);
        });

        // Meeting: Direction required (all three), DurationMinutes required
        When(x => x.EventType == "Meeting", () =>
        {
            RuleFor(x => x.Direction)
                .NotNull()
                .WithMessage("Direction is required for Meeting events.")
                .Must(d => AllDirections.Contains(d))
                .When(x => x.Direction is not null)
                .WithMessage("Direction for Meeting must be Inbound, Outbound, or Internal.");
            RuleFor(x => x.DurationMinutes)
                .NotNull()
                .WithMessage("DurationMinutes is required for Meeting events.")
                .InclusiveBetween(1, 600)
                .When(x => x.DurationMinutes is not null);
        });
    }
}

public class CommunicationEditRequestValidator : AbstractValidator<CommunicationEditRequest>
{
    public CommunicationEditRequestValidator()
    {
        RuleFor(x => x.Body).MinimumLength(1).MaximumLength(4000).When(x => x.Body is not null);
        RuleFor(x => x.Subject).MinimumLength(1).MaximumLength(200).When(x => x.Subject is not null);
        RuleFor(x => x.Outcome).MinimumLength(1).MaximumLength(1000).When(x => x.Outcome is not null);
        RuleFor(x => x.OccurredAt)
            .Must(d => d!.Value <= DateTime.UtcNow)
            .When(x => x.OccurredAt.HasValue)
            .WithMessage("OccurredAt must not be in the future.");
    }
}

public class CommunicationRedactRequestValidator : AbstractValidator<CommunicationRedactRequest>
{
    public CommunicationRedactRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class CommunicationFollowUpRequestValidator : AbstractValidator<CommunicationFollowUpRequest>
{
    private static readonly string[] ValidPriorities = ["Low", "Normal", "High", "Urgent"];

    public CommunicationFollowUpRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Priority)
            .Must(p => ValidPriorities.Contains(p!))
            .When(x => x.Priority is not null)
            .WithMessage($"Priority must be one of: {string.Join(", ", ValidPriorities)}.");
    }
}
