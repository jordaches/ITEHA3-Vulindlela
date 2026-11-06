using Vulindlela.Clearance.Api.Business.Interfaces;

namespace Vulindlela.Clearance.Api.Data.Repositories
{
    public sealed class ClearanceUnitOfWork : IUnitOfWork
    {
        private readonly ClearanceDbContext _db;
        public ClearanceUnitOfWork(ClearanceDbContext db) => _db = db;
        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
