namespace Vulindlela.Booking.Api.Data.Entities
{
    public enum AppointmentStatus { Booked = 0, Completed = 1 }

    public class Appointment
    {
        public int Id { get; set; }
        public int SlotId { get; set; }
        public Slot Slot { get; set; } = null!;
        public int CitizenAccountId { get; set; }
        public CitizenAccount Citizen { get; set; } = null!;
        public string ServiceType { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
        public DateTime CreatedAtUtc { get; set; }
    }

}
