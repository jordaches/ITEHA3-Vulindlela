using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Business;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Data.Repositories
{

    public sealed class AppointmentRepository : IAppointmentRepository
    {
        private readonly BookingDbContext _db;
        public AppointmentRepository(BookingDbContext db) => _db = db;

        public async Task AddAsync(Appointment appointment, CancellationToken ct)
        {
            _db.Appointments.Add(appointment);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Leave the context usable for the caller's next attempt.
                _db.Entry(appointment).State = EntityState.Detached;
                throw new DuplicateSlotException(appointment.SlotId);
            }
        }

        public Task<Appointment?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.Appointments
               .Include(a => a.Slot).ThenInclude(s => s.Branch)
               .FirstOrDefaultAsync(a => a.Id == id, ct);

        public async Task<IReadOnlyList<Appointment>> GetForCitizenAsync(int citizenAccountId, CancellationToken ct) =>
            await _db.Appointments
                     .Include(a => a.Slot).ThenInclude(s => s.Branch)
                     .Where(a => a.CitizenAccountId == citizenAccountId)
                     .OrderBy(a => a.Slot.StartsAtUtc)
                     .ToListAsync(ct);

        public Task<bool> ExistsForCitizenOnDateAsync(int citizenAccountId, DateTime dateUtc, CancellationToken ct)
        {
            var start = dateUtc.Date;
            var end = start.AddDays(1);
            return _db.Appointments.AnyAsync(a =>
                a.CitizenAccountId == citizenAccountId &&
                a.Slot.StartsAtUtc >= start &&
                a.Slot.StartsAtUtc < end, ct);
        }

        // SQLITE_CONSTRAINT = 19. Also covers the Reference
        // collision; both mean "this insert would break a unique index".
        private static bool IsUniqueViolation(DbUpdateException ex) =>
            ex.InnerException is SqliteException { SqliteErrorCode: 19 };

        public async Task AddWithNotificationAsync(
            Appointment appointment,
            NotificationOutbox notification,
            CancellationToken ct)
        {
            _db.Appointments.Add(appointment);
            _db.Outbox.Add(notification);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _db.Entry(appointment).State = EntityState.Detached;
                _db.Entry(notification).State = EntityState.Detached;
                throw new DuplicateSlotException(appointment.SlotId);
            }
        }
    }

}
