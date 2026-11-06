# ADR 0003: Prevent double booking with a unique index on Appointment.SlotId

- **Status:** Accepted
- **Date:** Phase 1, week 2

## Context

A slot has capacity of exactly one. When booking opens, many citizens request the
same popular slot in the same second — the "thundering herd". The obvious
implementation is wrong:

    if (!await _repository.SlotIsTaken(slotId))   // check
    {
        await _repository.Add(appointment);       // act
    }

Two requests can both pass the check before either performs the act. The window
is small; it is not zero, and under load it is hit routinely. Phase 1 must hold a
guarantee, not a probability.

## Decision

Enforce the invariant in the database, with a unique index on
`Appointment.SlotId`, and let the losing request fail:

    b.Entity<Appointment>().HasIndex(x => x.SlotId).IsUnique();

The business layer performs no "is it taken?" check. It attempts the insert and
interprets the answer. The repository catches the SQLite constraint violation
(`SqliteErrorCode == 19`, SQLITE_CONSTRAINT) and translates it into a domain
exception, `DuplicateSlotException`, so that no EF Core or Sqlite type crosses
into the business layer. `BookingService` maps that to `BookingOutcome.Conflict`,
and the controller returns HTTP 409.

The existence of the appointment row IS the booking state. There is no
`Slot.IsBooked` flag, because a flag is a second source of truth that can drift.

## Alternatives considered

**Application-level lock (`SemaphoreSlim` or `lock` around the check-and-insert).**
Rejected. It only works inside one process. The target architecture runs several
instances of the Booking service, and a lock in instance A does not stop instance
B. It also provides no protection after a restart, and it serialises every booking
in the process rather than only the contended ones.

**Optimistic concurrency with a version column on `Slot`.** Rejected as the
primary mechanism. It is a real technique, but SQLite has no native `rowversion`;
implementing it means a hand-maintained version column and a retry loop in the
business layer. It also still relies on the application to compare and retry
correctly, whereas the unique index is enforced no matter what the application
does — including a future code path that forgets the retry. The unique index is
strictly simpler and strictly stronger here.

**Serializable transaction with `BEGIN IMMEDIATE` followed by a check and an
insert.** Rejected. It works, but it takes a database-wide write lock, so every
booking anywhere serialises behind every other booking. It also expresses no
invariant: a later developer who opens a transaction and forgets `IMMEDIATE`
reintroduces the bug silently, whereas a unique index cannot be bypassed without
dropping it.

**Conditional update / compare-and-swap: `UPDATE Slots SET IsBooked = 1 WHERE Id =
@id AND IsBooked = 0`, then check rows affected.** Rejected as the primary
mechanism for the same reason as the flag above — it introduces mutable derived
state that can drift from `Appointments`. It is a legitimate design; the unique
index was chosen because the invariant is a property of the appointments table
alone.

**A queue that serialises all bookings through one consumer.** Rejected for Phase
1. It solves contention by removing concurrency, which costs latency and adds a
component that must itself be available — reintroducing the single point of
failure the March incident was about. It also does not prevent duplicates if the
consumer ever scales out.

## Consequences

**What we gain.** A guarantee that holds across processes, machines, restarts, and
future code paths, because it is enforced by the storage engine. The losing
request fails fast with a clear 409 and a message the Portal can show. The test in
`BookingConcurrencyTests` proves it with 20 simultaneous writers and asserts
exactly one appointment exists afterwards.

**What we accept.** The business layer must handle a persistence failure mode,
which is a small leak of the database into the domain; the repository contains
that leak in one method. Under heavy contention many requests get a 409 and the
citizen must pick another slot — we accept that over the complexity of
queueing and backoff in Phase 1. **Cancellation is not supported**: a plain unique
index on `SlotId` means a cancelled appointment still occupies the slot forever.
Supporting cancellation would require a partial unique index (`WHERE Status =
'Booked'`) or a separate booking table; that is Phase 2 work. The
one-appointment-per-citizen-per-day rule remains a best-effort application check
and is **not** atomically guaranteed — two concurrent requests by the same citizen
for two different slots on the same day can both succeed. That rule is a
convenience, not the invariant this ADR is about, and making it atomic would
require denormalising the appointment date into its own uniquely indexed column.

## Evidence

`BookingServiceTests.Twenty_concurrent_requests_for_one_slot_produce_exactly_one_appointment`
— 20 concurrent bookings, 1 success, 19 conflicts, 1 row in `Appointments`.