using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Business.Security;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Data.Entities;
using Vulindlela.Booking.Api.Data.Seed;

namespace Vulindlela.Booking.Tests;

public class AuthTests
{
    private static readonly Pbkdf2PasswordHasher Hasher = new();

    [Fact]
    public void Hasher_produces_a_verifiable_hash_with_a_per_user_salt()
    {
        var (hash1, salt1) = Hasher.Create("1P@ssword");
        var (hash2, salt2) = Hasher.Create("1P@ssword");

        Assert.NotEqual(salt1, salt2);                     // different salt per user
        Assert.NotEqual(hash1, hash2);                     // and therefore different hashes
        Assert.True(Hasher.Verify("1P@ssword", hash1, salt1));
        Assert.False(Hasher.Verify("1P@ssword?", hash1, salt1));
    }

    [Fact]
    public async Task Seeded_citizen_verifies_with_the_documented_password()
    {
        using var db = new SqliteTestDatabase();

        await using (var ctx = db.CreateContext())
        {
            BookingSeeder.Seed(ctx, Hasher);
        }

        await using var check = db.CreateContext();
        var citizen = await check.Citizens.SingleAsync(c => c.IdNumber == "8901015800081");

        Assert.Equal("Citizen", citizen.Role);
        Assert.NotNull(citizen.PasswordHash);
        Assert.NotNull(citizen.PasswordSalt);
        Assert.True(Hasher.Verify("1P@ssword", citizen.PasswordHash!, citizen.PasswordSalt!));
        Assert.False(Hasher.Verify("wrong", citizen.PasswordHash!, citizen.PasswordSalt!));
    }

    [Fact]
    public async Task Seeded_official_has_the_official_role_and_its_own_password()
    {
        using var db = new SqliteTestDatabase();
        await using (var ctx = db.CreateContext()) BookingSeeder.Seed(ctx, Hasher);

        await using var check = db.CreateContext();
        var official = await check.Citizens.SingleAsync(c => c.IdNumber == "7001015800080");

        Assert.Equal("Official", official.Role);
        Assert.True(Hasher.Verify("Official123!", official.PasswordHash!, official.PasswordSalt!));
    }
}