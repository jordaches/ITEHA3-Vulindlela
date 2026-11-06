namespace Vulindlela.Clearance.Api.Business.Interfaces
{
    public interface IDocumentQueue
    {
        ValueTask EnqueueAsync(int submissionId, CancellationToken ct = default);
        IAsyncEnumerable<int> ReadAllAsync(CancellationToken ct = default);
    }
}
