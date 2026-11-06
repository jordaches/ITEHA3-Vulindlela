using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Data.Entities;
using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Clearance.Api.Data;
public sealed class DocumentRuleRepository : IDocumentRuleRepository
{
    private readonly ClearanceDbContext _db;
    public DocumentRuleRepository(ClearanceDbContext db) => _db = db;

    public async Task<IReadOnlyList<DocumentRule>> GetForServiceTypeAsync(
        string serviceType, CancellationToken ct) =>
        await _db.DocumentRules
                 .Where(r => r.ServiceType == serviceType)
                 .OrderBy(r => r.DocumentType)
                 .ToListAsync(ct);

    public async Task<IReadOnlyList<DocumentRule>> GetAllAsync(CancellationToken ct) =>
        await _db.DocumentRules.OrderBy(r => r.ServiceType).ThenBy(r => r.DocumentType).ToListAsync(ct);
}


namespace Vulindlela.Clearance.Api.Data.Repositories
{
    public sealed class DocumentRuleRepository : IDocumentRuleRepository
    {
        private readonly ClearanceDbContext _db;
        public DocumentRuleRepository(ClearanceDbContext db) => _db = db;

        public async Task<IReadOnlyList<DocumentRule>> GetForServiceTypeAsync(
            string serviceType, CancellationToken ct) =>
            await _db.DocumentRules
                     .Where(r => r.ServiceType == serviceType)
                     .OrderBy(r => r.DocumentType)
                     .ToListAsync(ct);

        public async Task<IReadOnlyList<DocumentRule>> GetAllAsync(CancellationToken ct) =>
            await _db.DocumentRules.OrderBy(r => r.ServiceType).ThenBy(r => r.DocumentType).ToListAsync(ct);
    }

}
