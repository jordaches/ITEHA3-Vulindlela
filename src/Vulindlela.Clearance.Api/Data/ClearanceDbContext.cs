using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Data;

public class ClearanceDbContext : DbContext
{
    public ClearanceDbContext(DbContextOptions<ClearanceDbContext> options) : base(options) { }

    public DbSet<DocumentRule> DocumentRules => Set<DocumentRule>();
    public DbSet<ClearanceStatus> ClearanceStatuses => Set<ClearanceStatus>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<SubmittedDocument> SubmittedDocuments => Set<SubmittedDocument>();
    public DbSet<MissingDocument> MissingDocuments => Set<MissingDocument>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<DocumentRule>(e =>
        {
            e.Property(x => x.ServiceType).HasMaxLength(32).IsRequired();
            e.Property(x => x.DocumentType).HasMaxLength(64).IsRequired();
            e.Property(x => x.AcceptedFormats).HasMaxLength(64).IsRequired();
            e.HasIndex(x => new { x.ServiceType, x.DocumentType }).IsUnique();
        });

        b.Entity<ClearanceStatus>(e =>
        {
            e.Property(x => x.ServiceType).HasMaxLength(32).IsRequired();
            e.HasIndex(x => x.AppointmentId).IsUnique();
            e.HasIndex(x => x.CitizenAccountId);        // for the authorisation lookup
        });

        b.Entity<Submission>(e =>
        {
            e.HasOne(x => x.ClearanceStatus).WithMany(x => x.Submissions)
             .HasForeignKey(x => x.ClearanceStatusId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<SubmittedDocument>(e =>
        {
            e.Property(x => x.DocumentType).HasMaxLength(64).IsRequired();
            e.Property(x => x.FileName).HasMaxLength(256).IsRequired();
            e.HasOne(x => x.Submission).WithMany(x => x.Documents)
             .HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<MissingDocument>(e =>
        {
            e.Property(x => x.DocumentType).HasMaxLength(64).IsRequired();
            e.Property(x => x.Reason).HasMaxLength(256).IsRequired();
            e.HasOne(x => x.ClearanceStatus).WithMany(x => x.MissingDocuments)
             .HasForeignKey(x => x.ClearanceStatusId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}