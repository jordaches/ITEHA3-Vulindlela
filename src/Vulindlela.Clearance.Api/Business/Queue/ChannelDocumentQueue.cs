using System.Threading.Channels;
using Vulindlela.Clearance.Api.Business.Interfaces;

namespace Vulindlela.Clearance.Api.Business.Queue
{
    public sealed class ChannelDocumentQueue : IDocumentQueue
    {
        private readonly Channel<int> _channel;

        public ChannelDocumentQueue()
        {
            _channel = Channel.CreateBounded<int>(new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
        }

        public ValueTask EnqueueAsync(int submissionId, CancellationToken ct) =>
            _channel.Writer.WriteAsync(submissionId, ct);

        public IAsyncEnumerable<int> ReadAllAsync(CancellationToken ct) =>
            _channel.Reader.ReadAllAsync(ct);
    }
}
