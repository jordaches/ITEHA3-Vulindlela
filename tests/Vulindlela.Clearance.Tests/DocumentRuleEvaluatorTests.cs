using Vulindlela.Clearance.Api.Business;
using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Clearance.Api.Data;
using Vulindlela.Clearance.Api.Data.Entities;
using Vulindlela.Clearance.Api.Data.Repositories;

namespace Vulindlela.Clearance.Tests;

public class DocumentRuleEvaluatorTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    private sealed class FakeClock(DateTime now) : IClock { public DateTime UtcNow => now; }

    private static DocumentRuleEvaluator Build(ClearanceDbContext ctx) =>
        new(new DocumentRuleRepository(ctx), new FakeClock(Now));

    private static void SeedRules(ClearanceDbContext ctx, params DocumentRule[] rules)
    {
        ctx.DocumentRules.AddRange(rules);
        ctx.SaveChanges();
    }

    private static SubmittedDocument Doc(string type, string file = "x.pdf", DateTime? certifiedOnUtc = null) =>
        new() { DocumentType = type, FileName = file, CertifiedOnUtc = certifiedOnUtc };

    [Fact]
    public async Task Missing_mandatory_document_is_reported_as_Required()
    {
        using var db = new SqliteTestDatabase();
        await using var ctx = db.CreateContext();
        SeedRules(ctx, new DocumentRule
        {
            ServiceType = "PASSPORT",
            DocumentType = "Certified ID copy",
            IsMandatory = true,
            MaxCertificationAgeDays = 90,
            AcceptedFormats = "pdf,jpg"
        });

        var missing = await Build(ctx).EvaluateAsync("PASSPORT", Array.Empty<SubmittedDocument>(), default);

        Assert.Single(missing);
        Assert.Equal("Certified ID copy", missing[0].DocumentType);
        Assert.Equal("Required", missing[0].Reason);
    }

    [Fact]
    public async Task Certification_older_than_the_window_is_reported_as_too_old()
    {
        using var db = new SqliteTestDatabase();
        await using var ctx = db.CreateContext();
        SeedRules(ctx, new DocumentRule
        {
            ServiceType = "PASSPORT",
            DocumentType = "Certified ID copy",
            IsMandatory = true,
            MaxCertificationAgeDays = 90,
            AcceptedFormats = "pdf"
        });

        var old = Now.AddDays(-200);
        var missing = await Build(ctx).EvaluateAsync(
            "PASSPORT", new[] { Doc("Certified ID copy", certifiedOnUtc: old) }, default);

        Assert.Single(missing);
        Assert.Contains("too long ago", missing[0].Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Everything_present_and_fresh_returns_no_missing_documents()
    {
        using var db = new SqliteTestDatabase();
        await using var ctx = db.CreateContext();
        SeedRules(ctx,
            new DocumentRule
            {
                ServiceType = "PASSPORT",
                DocumentType = "Certified ID copy",
                IsMandatory = true,
                MaxCertificationAgeDays = 90,
                AcceptedFormats = "pdf"
            },
            new DocumentRule
            {
                ServiceType = "PASSPORT",
                DocumentType = "Passport photographs",
                IsMandatory = true,
                MaxCertificationAgeDays = null,
                AcceptedFormats = "jpg"
            });

        var docs = new[]
        {
            Doc("Certified ID copy", "id.pdf", certifiedOnUtc: Now.AddDays(-10)),
            Doc("Passport photographs", "photo.jpg")
        };

        var missing = await Build(ctx).EvaluateAsync("PASSPORT", docs, default);

        Assert.Empty(missing);
    }

    [Fact]
    public async Task Wrong_file_extension_is_reported()
    {
        using var db = new SqliteTestDatabase();
        await using var ctx = db.CreateContext();
        SeedRules(ctx, new DocumentRule
        {
            ServiceType = "PASSPORT",
            DocumentType = "Passport photographs",
            IsMandatory = true,
            MaxCertificationAgeDays = null,
            AcceptedFormats = "jpg"
        });

        var missing = await Build(ctx).EvaluateAsync(
            "PASSPORT", new[] { Doc("Passport photographs", "photo.png") }, default);

        Assert.Single(missing);
        Assert.Contains("Format", missing[0].Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A_service_type_with_no_rules_returns_empty_not_error()
    {
        using var db = new SqliteTestDatabase();
        await using var ctx = db.CreateContext();
        // No rules seeded — this is the ID_COLLECTION case.

        var missing = await Build(ctx).EvaluateAsync("ID_COLLECTION", Array.Empty<SubmittedDocument>(), default);

        Assert.Empty(missing);
    }
}