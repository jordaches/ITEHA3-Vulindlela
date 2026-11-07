namespace Vulindlela.Booking.Api.Data.Entities;

public class BookingAccessLog
{
    public int Id { get; set; }
    public int CitizenAccountId { get; set; }      // Whose record was accessed
    public int? AccessedByCitizenId { get; set; }  // Who accessed it

    public string AccessedByName { get; set; } = string.Empty;
    public string AccessedByRole { get; set; } = string.Empty;
    public DateTime AccessedAtUtc { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
}