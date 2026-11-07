namespace Vulindlela.Shared.Contracts;

public sealed record AccessLogEntryDto(
    string Service,
    DateTime AccessedAtUtc,
    string AccessedByRole,
    int? AccessedByCitizenId,
    string Action,
    string? ResourceId);