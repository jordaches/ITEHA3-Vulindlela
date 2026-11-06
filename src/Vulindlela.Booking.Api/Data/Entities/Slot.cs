namespace Vulindlela.Booking.Api.Data.Entities
{
    public class Slot
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
        public DateTime StartsAtUtc { get; set; }
        public string ServiceType { get; set; } = string.Empty;

        // Navigation only. Existence of an Appointment row IS the booking state.
        public Appointment? Appointment { get; set; }
    }
}
