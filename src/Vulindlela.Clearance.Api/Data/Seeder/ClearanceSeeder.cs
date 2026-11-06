using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Data.Seed;

public static class ClearanceSeeder
{
    public static void Seed(ClearanceDbContext db)
    {
        if (db.DocumentRules.Any()) return;

        var rules = new List<DocumentRule>();

        void Add(string serviceType, string documentType, int? maxCertificationAgeDays, string formats = "pdf,jpg")
            => rules.Add(new DocumentRule
            {
                ServiceType = serviceType,
                DocumentType = documentType,
                IsMandatory = true,
                MaxCertificationAgeDays = maxCertificationAgeDays,
                AcceptedFormats = formats
            });

        // ID_APPLICATION
        Add("ID_APPLICATION", "Certified identity document", 90);
        Add("ID_APPLICATION", "Proof of residence", 90);

        // PASSPORT
        Add("PASSPORT", "Certified ID copy", 90);
        Add("PASSPORT", "Passport photographs", null, "jpg");
        Add("PASSPORT", "Proof of residence", 90);

        // PASSPORT_URGENT — same document set; fee differs (not the documents)
        Add("PASSPORT_URGENT", "Certified ID copy", 90);
        Add("PASSPORT_URGENT", "Passport photographs", null, "jpg");
        Add("PASSPORT_URGENT", "Proof of residence", 90);

        // BIRTH_CERT
        Add("BIRTH_CERT", "Parent ID copy", 90);
        Add("BIRTH_CERT", "Proof of birth", null);

        // MARRIAGE_CERT
        Add("MARRIAGE_CERT", "Both parties' IDs", 90);
        Add("MARRIAGE_CERT", "Witnesses' IDs", 90);
        Add("MARRIAGE_CERT", "Marriage officer details", null);

        // DEATH_CERT
        Add("DEATH_CERT", "Deceased's ID", 90);
        Add("DEATH_CERT", "Informant's ID", 90);
        Add("DEATH_CERT", "Medical certificate of cause of death", null);
        Add("DEATH_CERT", "Proof of residence", 90);

        // ID_COLLECTION — intentionally no rules. Registration becomes NotRequired.

        db.DocumentRules.AddRange(rules);
        db.SaveChanges();
    }
}