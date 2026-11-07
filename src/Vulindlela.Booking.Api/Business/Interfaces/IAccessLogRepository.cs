using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Business.Interfaces;

public interface IAccessLogRepository
{
    Task AddAsync(BookingAccessLog log, CancellationToken ct);
    Task<IReadOnlyList<BookingAccessLog>> GetForCitizenAsync(int citizenId, CancellationToken ct);
}