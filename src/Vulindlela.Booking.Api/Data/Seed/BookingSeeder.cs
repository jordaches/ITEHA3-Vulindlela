using Vulindlela.Booking.Api.Business.Security;
using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Data.Seed;

public static class BookingSeeder
{
    private static readonly string[] ServiceTypes =
    {
        "ID_COLLECTION", "ID_APPLICATION", "PASSPORT",
        "PASSPORT_URGENT", "BIRTH_CERT", "MARRIAGE_CERT", "DEATH_CERT"
    };

    private static readonly TimeSpan[] DailyTimes =
    {
        new(9, 0, 0), new(11, 0, 0), new(14, 0, 0)
    };

    public static void Seed(BookingDbContext db, IPasswordHasher hasher)
    {
        if (db.Branches.Any()) return;

        var branches = new List<Branch>
        {
            new() { Code = "JHB-CENTRAL", Name = "Johannesburg Central", City = "Johannesburg", Address = "1 Rissik Street" },
            new() { Code = "PTA-CBD",     Name = "Pretoria CBD",        City = "Pretoria",     Address = "22 Church Street" },
            new() { Code = "CPT-CBD",     Name = "Cape Town CBD",       City = "Cape Town",    Address = "40 Barrack Street" }
        };
        db.Branches.AddRange(branches);
        db.SaveChanges();

        var nextMonday = NextWeekday(DateTime.UtcNow.Date, DayOfWeek.Monday);
        var slots = new List<Slot>();
        foreach (var branch in branches)
        {
            for (var day = 0; day < 14; day++)
            {
                var date = nextMonday.AddDays(day);
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
                foreach (var serviceType in ServiceTypes)
                    foreach (var time in DailyTimes)
                        slots.Add(new Slot
                        {
                            BranchId = branch.Id,
                            StartsAtUtc = date.Add(time),
                            ServiceType = serviceType
                        });
            }
        }
        db.Slots.AddRange(slots);

        db.Citizens.AddRange(
            Account(hasher, "Thandi Mokoena", "9001015800081", "0821110001", "1P@ssword", "Citizen"),
            Account(hasher, "Sipho Ndlovu", "8802025800082", "0821110002", "1P@ssword", "Citizen"),
            Account(hasher, "Anele Dlamini", "9203035800083", "0821110003", "1P@ssword", "Citizen"),
            Account(hasher, "Demo Official", "7001015800080", "0821110099", "Official123!", "Official"));

        db.SaveChanges();
    }

    private static CitizenAccount Account(
        IPasswordHasher hasher, string name, string id, string cell, string password, string role)
    {
        var (hash, salt) = hasher.Create(password);
        return new CitizenAccount
        {
            FullName = name,
            IdNumber = id,
            CellNumber = cell,
            Role = role,
            PasswordHash = hash,
            PasswordSalt = salt
        };
    }

    private static DateTime NextWeekday(DateTime from, DayOfWeek target)
    {
        var delta = ((int)target - (int)from.DayOfWeek + 7) % 7;
        return delta == 0 ? from : from.AddDays(delta);
    }
}