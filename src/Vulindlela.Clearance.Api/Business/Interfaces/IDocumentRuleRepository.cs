using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Business.Interfaces;

public interface IDocumentRuleRepository
{
    Task<IReadOnlyList<DocumentRule>> GetForServiceTypeAsync(string serviceType, CancellationToken ct);
    Task<IReadOnlyList<DocumentRule>> GetAllAsync(CancellationToken ct);
}