using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Slot> Slots => Set<Slot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<CitizenAccount> Citizens => Set<CitizenAccount>();

    public DbSet<NotificationOutbox> Outbox => Set<NotificationOutbox>();

    public DbSet<BookingAccessLog> AccessLogs => Set<BookingAccessLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Branch>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        b.Entity<Slot>(e =>
        {
            e.Property(x => x.ServiceType).HasMaxLength(32).IsRequired();
            e.HasIndex(x => new { x.BranchId, x.StartsAtUtc, x.ServiceType }).IsUnique();
            e.HasOne(x => x.Branch).WithMany(x => x.Slots)
             .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Appointment>(e =>
        {
            e.Property(x => x.Reference).HasMaxLength(16).IsRequired();
            e.Property(x => x.Fee).HasColumnType("decimal(10,2)");
            e.HasIndex(x => x.Reference).IsUnique();

            //  THE GUARANTEE. A slot can hold at most one appointment.
            //  Enforced by the database. Not by application code.
            //  Two concurrent inserts both reach SQLite; exactly one wins.

            e.HasIndex(x => x.SlotId).IsUnique();

            e.HasOne(x => x.Slot).WithOne(x => x.Appointment)
             .HasForeignKey<Appointment>(x => x.SlotId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Citizen).WithMany(x => x.Appointments)
             .HasForeignKey(x => x.CitizenAccountId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CitizenAccount>(e =>
        {
            e.Property(x => x.IdNumber).HasMaxLength(13).IsRequired();
            e.Property(x => x.Role).HasMaxLength(16).IsRequired();
            e.HasIndex(x => x.IdNumber).IsUnique();
        });

        b.Entity<NotificationOutbox>(e =>
        {
            e.Property(x => x.ServiceType).HasMaxLength(32).IsRequired();
            e.Property(x => x.LastError).HasMaxLength(1024);

            e.HasOne(x => x.Appointment).WithMany()
             .HasForeignKey(x => x.AppointmentId)
             .OnDelete(DeleteBehavior.Cascade);

            // Indexes for the dispatcher's polling query.
            e.HasIndex(x => new { x.SentAtUtc, x.NextAttemptAtUtc });
        });

        b.Entity<BookingAccessLog>(e =>
        {
            e.Property(x => x.AccessedByRole).HasMaxLength(16).IsRequired();
            e.Property(x => x.Action).HasMaxLength(64).IsRequired();
            e.Property(x => x.ResourceId).HasMaxLength(64);
            e.Property(x => x.AccessedByName).HasMaxLength(128).IsRequired();
            e.HasIndex(x => x.CitizenAccountId);
        });
    }
}