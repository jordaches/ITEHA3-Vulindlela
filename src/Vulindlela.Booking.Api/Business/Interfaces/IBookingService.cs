using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Business.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResult> BookAsync(BookAppointmentRequest request, CancellationToken ct);
        Task<AppointmentDto?> GetAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<AppointmentDto>> GetForCitizenAsync(int citizenAccountId, CancellationToken ct);
    }
}
