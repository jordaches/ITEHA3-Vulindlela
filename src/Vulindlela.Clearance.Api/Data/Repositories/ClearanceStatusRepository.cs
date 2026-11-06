using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Data.Entities;
using Vulindlela.Clearance.Api.Business.Interfaces;

namespace Vulindlela.Clearance.Api.Data.Repositories
{
    public sealed class ClearanceStatusRepository : IClearanceStatusRepository
    {
        private readonly ClearanceDbContext _db;
        public ClearanceStatusRepository(ClearanceDbContext db) => _db = db;

        public Task<ClearanceStatus?> GetByAppointmentIdAsync(int appointmentId, CancellationToken ct) =>
            _db.ClearanceStatuses
               .Include(s => s.MissingDocuments)
               .FirstOrDefaultAsync(s => s.AppointmentId == appointmentId, ct);

        public Task<ClearanceStatus?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.ClearanceStatuses
               .Include(s => s.MissingDocuments)
               .FirstOrDefaultAsync(s => s.Id == id, ct);

        public void Add(ClearanceStatus status) => _db.ClearanceStatuses.Add(status);
    }
}
