using Vulindlela.Booking.Api.Business.Interfaces;

namespace Vulindlela.Booking.Api.Business
{
    public sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }
}
