namespace Vulindlela.Booking.Api.Clients;

public interface IClearanceClient
{
    Task NotifyAppointmentCreatedAsync(
        int appointmentId,
        int citizenAccountId,
        string serviceType,
        CancellationToken ct);
}