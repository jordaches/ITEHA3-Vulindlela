namespace Vulindlela.Clearance.Api.Data.Entities;

public class ClearanceAccessLog
{
    public int Id { get; set; }
    public int CitizenAccountId { get; set; }
    public int? AccessedByCitizenId { get; set; }
    public string AccessedByRole { get; set; } = string.Empty;
    public DateTime AccessedAtUtc { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
}