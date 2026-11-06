using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Business.Interfaces
{
    public interface IClearanceStatusRepository
    {
        Task<ClearanceStatus?> GetByAppointmentIdAsync(int appointmentId, CancellationToken ct);
        Task<ClearanceStatus?> GetByIdAsync(int id, CancellationToken ct);
        void Add(ClearanceStatus status);
    }
}
