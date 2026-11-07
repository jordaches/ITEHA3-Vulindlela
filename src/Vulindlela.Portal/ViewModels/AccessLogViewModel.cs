using Vulindlela.Shared.Contracts;

namespace Vulindlela.Portal.ViewModels;

public sealed record AccessLogViewModel(
    int CitizenId,
    IReadOnlyList<AccessLogEntryDto> Entries,
    string? Error);