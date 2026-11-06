using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Business;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Data.Entities;
using Vulindlela.Booking.Api.Data.Repositories;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Tests;

public class BookingServiceTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 5, 8, 0, 0, DateTimeKind.Utc);

    // ---------- helpers ----------

    private static BookingService Build(BookingDbContext ctx) => new(
        new SlotRepository(ctx),
        new AppointmentRepository(ctx),
        new CitizenAccountRepository(ctx),
        new FeeCalculator(),
        new FakeClock(FixedNow));

    private sealed class FakeClock(DateTime now) : IClock { public DateTime UtcNow => now; }

    private static async Task<(int slotId, int citizenId)> SeedOneSlotAsync(
        SqliteTestDatabase db, string serviceType = "PASSPORT", DateTime? when = null)
    {
        await using var ctx = db.CreateContext();
        var branch = new Branch { Code = "JHB-C", Name = "Johannesburg Central", City = "Johannesburg", Address = "1 Main Rd" };
        ctx.Branches.Add(branch);
        await ctx.SaveChangesAsync();

        var slot = new Slot { BranchId = branch.Id, StartsAtUtc = when ?? FixedNow.AddDays(2), ServiceType = serviceType };
        ctx.Slots.Add(slot);

        var citizen = new CitizenAccount { FullName = "Test Citizen", IdNumber = "9001015800081", CellNumber = "0821110001" };
        ctx.Citizens.Add(citizen);
        await ctx.SaveChangesAsync();

        return (slot.Id, citizen.Id);
    }

    // ---------- booking rules ----------

    [Fact]
    public async Task Booking_a_free_slot_succeeds_and_persists_exactly_one_appointment()
    {
        using var db = new SqliteTestDatabase();
        var (slotId, citizenId) = await SeedOneSlotAsync(db);

        await using (var ctx = db.CreateContext())
        {
            var result = await Build(ctx).BookAsync(
                new BookAppointmentRequest(slotId, citizenId, "PASSPORT"), default);

            Assert.Equal(BookingOutcome.Success, result.Outcome);
            Assert.Equal(465m, result.Appointment!.Fee);        // 400 + 65 surcharge
            Assert.StartsWith("VL-", result.Appointment.Reference);
        }

        await using var verify = db.CreateContext();
        Assert.Equal(1, await verify.Appointments.CountAsync());
    }

    [Fact]
    public async Task Booking_a_slot_in_the_past_is_rejected()
    {
        using var db = new SqliteTestDatabase();
        var (slotId, citizenId) = await SeedOneSlotAsync(db, when: FixedNow.AddDays(-1));

        await using var ctx = db.CreateContext();
        var result = await Build(ctx).BookAsync(
            new BookAppointmentRequest(slotId, citizenId, "PASSPORT"), default);

        Assert.Equal(BookingOutcome.Rejected, result.Outcome);
        Assert.Contains("past", result.Message!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Booking_with_a_service_type_that_does_not_match_the_slot_is_rejected()
    {
        using var db = new SqliteTestDatabase();
        var (slotId, citizenId) = await SeedOneSlotAsync(db, serviceType: "PASSPORT");

        await using var ctx = db.CreateContext();
        var result = await Build(ctx).BookAsync(
            new BookAppointmentRequest(slotId, citizenId, "BIRTH_CERT"), default);

        Assert.Equal(BookingOutcome.Rejected, result.Outcome);
    }

    [Fact]
    public async Task Booking_an_unknown_slot_returns_not_found()
    {
        using var db = new SqliteTestDatabase();
        var (_, citizenId) = await SeedOneSlotAsync(db);

        await using var ctx = db.CreateContext();
        var result = await Build(ctx).BookAsync(
            new BookAppointmentRequest( 9999, citizenId, "PASSPORT"), default);

        Assert.Equal(BookingOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task A_citizen_cannot_hold_two_appointments_on_the_same_day()
    {
        using var db = new SqliteTestDatabase();
        var (firstSlotId, citizenId) = await SeedOneSlotAsync(db);

        await using (var ctx = db.CreateContext())
        {
            var first = await Build(ctx).BookAsync(
                new BookAppointmentRequest(firstSlotId, citizenId, "PASSPORT"), default);
            Assert.Equal(BookingOutcome.Success, first.Outcome);
        }

        // Second slot, same day, different time.
        int secondSlotId;
        await using (var ctx = db.CreateContext())
        {
            var second = new Slot
            {
                BranchId = ctx.Slots.Single().BranchId,
                StartsAtUtc = FixedNow.AddDays(2).AddHours(2),
                ServiceType = "PASSPORT"
            };
            ctx.Slots.Add(second);
            await ctx.SaveChangesAsync();
            secondSlotId = second.Id;
        }

        await using var check = db.CreateContext();
        var result = await Build(check).BookAsync(
            new BookAppointmentRequest(secondSlotId, citizenId, "PASSPORT"), default);

        Assert.Equal(BookingOutcome.Rejected, result.Outcome);
        Assert.Contains("already", result.Message!, StringComparison.OrdinalIgnoreCase);
    }

    // ---------- the one that matters ----------

    [Fact]
    public async Task Twenty_concurrent_requests_for_one_slot_produce_exactly_one_appointment()
    {
        using var db = new SqliteTestDatabase();

        int slotId;
        int[] citizenIds;

        await using (var seedCtx = db.CreateContext())
        {
            var branch = new Branch { Code = "JHB-C", Name = "Johannesburg Central", City = "Johannesburg", Address = "1 Main Rd" };
            seedCtx.Branches.Add(branch);
            await seedCtx.SaveChangesAsync();

            var slot = new Slot { BranchId = branch.Id, StartsAtUtc = FixedNow.AddDays(2), ServiceType = "PASSPORT" };
            seedCtx.Slots.Add(slot);
            await seedCtx.SaveChangesAsync();
            slotId = slot.Id;

            var citizens = Enumerable.Range(0, 20).Select(i => new CitizenAccount
            {
                FullName = $"Citizen {i}",
                IdNumber = (9001015800000L + i).ToString(),
                CellNumber = $"0821110{i:D3}"
            }).ToList();
            seedCtx.Citizens.AddRange(citizens);
            await seedCtx.SaveChangesAsync();
            citizenIds = citizens.Select(c => c.Id).ToArray();
        }

        // Fire simultaneously. Task.Run matters: without it, async continuations may
        // run sequentially on the test thread and the race never happens.
        var results = await Task.WhenAll(citizenIds.Select(citizenId => Task.Run(async () =>
        {
            await using var ctx = db.CreateContext();
            return await Build(ctx).BookAsync(
                new BookAppointmentRequest(slotId, citizenId, "PASSPORT"), default);
        })));

        Assert.Equal(1, results.Count(r => r.Outcome == BookingOutcome.Success));
        Assert.Equal(19, results.Count(r => r.Outcome == BookingOutcome.Conflict));

        await using var verify = db.CreateContext();
        Assert.Equal(1, await verify.Appointments.CountAsync(a => a.SlotId == slotId));
    }
}

public class FeeCalculatorTests
{
    [Theory]
    [InlineData("ID_COLLECTION", 0)]
    [InlineData("ID_APPLICATION", 140)]
    [InlineData("PASSPORT", 465)]          // 400 + 65 surcharge
    [InlineData("PASSPORT_URGENT", 865)]   // 800 + 65 surcharge
    [InlineData("BIRTH_CERT", 75)]
    [InlineData("MARRIAGE_CERT", 75)]
    [InlineData("DEATH_CERT", 75)]
    public void Known_service_types_return_the_expected_fee(string serviceType, int expected)
    {
        var calculator = new FeeCalculator();
        Assert.True(calculator.TryCalculate(serviceType, out var fee));
        Assert.Equal(expected, fee);
    }

    [Fact]
    public void Unknown_service_type_is_rejected_rather_than_priced_at_zero()
    {
        var calculator = new FeeCalculator();
        Assert.False(calculator.TryCalculate("MAGIC_TRICK", out _));
    }
}