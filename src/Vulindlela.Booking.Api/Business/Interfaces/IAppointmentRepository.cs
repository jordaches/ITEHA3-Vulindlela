using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Business.Interfaces
{
    public interface IAppointmentRepository
    {
        /// <summary>Throws <see cref="DuplicateSlotException"/> if the slot is already taken.</summary>
        Task AddAsync(Appointment appointment, CancellationToken ct);
        Task<Appointment?> GetByIdAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<Appointment>> GetForCitizenAsync(int citizenAccountId, CancellationToken ct);
        Task<bool> ExistsForCitizenOnDateAsync(int citizenAccountId, DateTime dateUtc, CancellationToken ct);
    }
}
