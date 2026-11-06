using Vulindlela.Clearance.Api.Business.Interfaces;

namespace Vulindlela.Clearance.Api.Business;

public sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }