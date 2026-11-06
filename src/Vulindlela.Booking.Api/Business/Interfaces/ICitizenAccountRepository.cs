using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Business.Interfaces
{
    public interface ICitizenAccountRepository
    {
        Task<CitizenAccount?> GetByIdAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<CitizenAccount>> GetAllAsync(CancellationToken ct);
    }
}
