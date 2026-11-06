namespace Vulindlela.Shared.Contracts;

public sealed record BranchDto(int Id, string Code, string Name, string City, string Address);

public sealed record SlotDto(
    int Id,
    int BranchId,
    DateTime StartsAtUtc,
    string ServiceType,
    bool IsAvailable);

public sealed record CitizenDto(int Id, string FullName, string IdNumber, string CellNumber);

public sealed record BookAppointmentRequest(
    int SlotId,
    int CitizenAccountId,
    string ServiceType);

public sealed record AppointmentDto(
    int Id,
    int SlotId,
    int BranchId,
    string BranchName,
    DateTime StartsAtUtc,
    string ServiceType,
    string Reference,
    decimal Fee,
    DateTime CreatedAtUtc);

/// <summary>Uniform error shape. One shape for every failure keeps the Portal simple.</summary>
public sealed record ProblemDto(string Code, string Message);