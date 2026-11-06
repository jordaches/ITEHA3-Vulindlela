using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Booking.Api.Data.Entities;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Business
{
    public sealed class BookingService : IBookingService
    {
        private readonly ISlotRepository _slots;
        private readonly IAppointmentRepository _appointments;
        private readonly ICitizenAccountRepository _citizens;
        private readonly IFeeCalculator _fees;
        private readonly IClock _clock;

        public BookingService(
            ISlotRepository slots,
            IAppointmentRepository appointments,
            ICitizenAccountRepository citizens,
            IFeeCalculator fees,
            IClock clock)
        {
            _slots = slots;
            _appointments = appointments;
            _citizens = citizens;
            _fees = fees;
            _clock = clock;
        }

        public async Task<BookingResult> BookAsync(BookAppointmentRequest request, CancellationToken ct)
        {
            // ---- Rules. No SQL, no EF types, no HttpContext in this method. ----

            if (request.SlotId <= 0)
                return BookingResult.Rejected("A slot must be selected.");

            var slot = await _slots.GetByIdAsync(request.SlotId, ct);
            if (slot is null)
                return BookingResult.NotFound("That slot does not exist.");

            if (slot.StartsAtUtc <= _clock.UtcNow)
                return BookingResult.Rejected("That slot is in the past.");

            if (!slot.ServiceType.Equals(request.ServiceType, StringComparison.OrdinalIgnoreCase))
                return BookingResult.Rejected(
                    $"That slot is for {slot.ServiceType}, not {request.ServiceType}.");

            var citizen = await _citizens.GetByIdAsync(request.CitizenAccountId, ct);
            if (citizen is null)
                return BookingResult.NotFound("Citizen account does not exist.");

            if (!_fees.TryCalculate(slot.ServiceType, out var fee))
                return BookingResult.Rejected($"Unknown service type '{slot.ServiceType}'.");

            // Best-effort business rule, NOT an atomic guarantee. See ADR 0003.
            if (await _appointments.ExistsForCitizenOnDateAsync(citizen.Id, slot.StartsAtUtc, ct))
                return BookingResult.Rejected("You already hold an appointment on that day.");

            var appointment = new Appointment
            {
                SlotId = slot.Id,
                CitizenAccountId = citizen.Id,
                ServiceType = slot.ServiceType,
                Reference = NewReference(),
                Fee = fee,
                Status = AppointmentStatus.Booked,
                CreatedAtUtc = _clock.UtcNow
            };

            // ---- The act. No "is it taken?" check. The database decides. ----
            try
            {
                await _appointments.AddAsync(appointment, ct);
            }
            catch (DuplicateSlotException)
            {
                return BookingResult.Conflict(
                    "That slot was taken moments ago. Please choose another.");
            }

            return BookingResult.Success(ToDto(appointment, slot));
        }

        public async Task<AppointmentDto?> GetAsync(int id, CancellationToken ct)
        {
            var a = await _appointments.GetByIdAsync(id, ct);
            return a is null ? null : ToDto(a, a.Slot);
        }

        public async Task<IReadOnlyList<AppointmentDto>> GetForCitizenAsync(
            int citizenAccountId, CancellationToken ct)
        {
            var list = await _appointments.GetForCitizenAsync(citizenAccountId, ct);
            return list.Select(a => ToDto(a, a.Slot)).ToList();
        }

        private static AppointmentDto ToDto(Appointment a, Slot slot) => new(
            a.Id, a.SlotId, slot.BranchId, slot.Branch?.Name ?? string.Empty,
            slot.StartsAtUtc, a.ServiceType, a.Reference, a.Fee, a.CreatedAtUtc);

        private static string NewReference() =>
            "VL-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
    }
}
