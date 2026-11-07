using Vulindlela.Shared.Contracts;

namespace Vulindlela.Portal.ViewModels;

public sealed record BrowseSlotsViewModel(
    int BranchId,
    string BranchName,
    string Date,
    string? ServiceType,
    IReadOnlyList<SlotDto> Slots);