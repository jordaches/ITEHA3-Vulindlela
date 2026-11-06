using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Business.Interfaces
{
    public interface IDocumentRuleEvaluator
    {
        Task<IReadOnlyList<MissingDocument>> EvaluateAsync(
        string serviceType,
        IReadOnlyCollection<SubmittedDocument> documents,
        CancellationToken ct);
    }
}
