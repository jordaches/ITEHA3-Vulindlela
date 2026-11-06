using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Business;

public enum BookingOutcome { Success, NotFound, Rejected, Conflict }

public sealed class BookingResult
{
    public BookingOutcome Outcome { get; }
    public string? Message { get; }
    public AppointmentDto? Appointment { get; }

    private BookingResult(BookingOutcome outcome, string? message, AppointmentDto? appointment)
    {
        Outcome = outcome; Message = message; Appointment = appointment;
    }

    public static BookingResult Success(AppointmentDto a) => new(BookingOutcome.Success, null, a);
    public static BookingResult NotFound(string m) => new(BookingOutcome.NotFound, m, null);
    public static BookingResult Rejected(string m) => new(BookingOutcome.Rejected, m, null);
    public static BookingResult Conflict(string m) => new(BookingOutcome.Conflict, m, null);
}