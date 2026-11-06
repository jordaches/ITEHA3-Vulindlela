namespace Vulindlela.Booking.Api.Data.Entities;

public enum NotificationKind
{
    AppointmentCreated = 1
}

public class NotificationOutbox
{
    public int Id { get; set; }
    public NotificationKind Kind { get; set; }

    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public int CitizenAccountId { get; set; }
    public string ServiceType { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; }
    public string? LastError { get; set; }
}