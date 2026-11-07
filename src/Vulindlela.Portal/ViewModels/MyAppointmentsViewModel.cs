using Vulindlela.Shared.Contracts;

namespace Vulindlela.Portal.ViewModels;

public sealed record MyAppointmentsViewModel(
    string OwnerName,
    IReadOnlyList<AppointmentDto> Appointments,
    IReadOnlyDictionary<int, ClearanceStatusDto> Clearance,
    string? ClearanceError,
    string? BookingError = null);