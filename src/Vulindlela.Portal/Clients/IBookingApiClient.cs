using Vulindlela.Shared.Contracts;

namespace Vulindlela.Portal.Clients;

public interface IBookingApiClient
{
    Task<VerifyPasswordResponse?> VerifyPasswordAsync(string idNumber, string password, CancellationToken ct);
    Task<IReadOnlyList<BranchDto>> GetBranchesAsync(CancellationToken ct);
    Task<IReadOnlyList<SlotDto>> GetSlotsAsync(int branchId, string date, string? serviceType, CancellationToken ct);
    Task<(AppointmentDto? Appointment, string? Error)> BookAsync(
    BookAppointmentRequest request, CancellationToken ct);
    Task<IReadOnlyList<AppointmentDto>> GetAppointmentsForCitizenAsync(int citizenId, CancellationToken ct);

    Task<IReadOnlyList<AccessLogEntryDto>> GetAccessLogAsync(int citizenId, CancellationToken ct);
}