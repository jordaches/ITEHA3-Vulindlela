namespace Vulindlela.Booking.Api.Business;


/// Raised when the database refuses a second appointment for the same slot.
/// Deliberately a business-layer type: the business layer must not know about
/// SqliteException or EF Core. The repository translates.

public sealed class DuplicateSlotException : Exception
{
    public int SlotId { get; }
    public DuplicateSlotException(int slotId)
        : base($"Slot {slotId} has already been taken.") => SlotId = slotId;
}