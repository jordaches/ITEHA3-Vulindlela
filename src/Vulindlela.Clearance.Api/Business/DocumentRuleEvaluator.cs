using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Business
{
    public sealed class DocumentRuleEvaluator : IDocumentRuleEvaluator
    {
        private readonly IDocumentRuleRepository _rules;
        private readonly IClock _clock;

        public DocumentRuleEvaluator(IDocumentRuleRepository rules, IClock clock)
        {
            _rules = rules;
            _clock = clock;
        }

        public async Task<IReadOnlyList<MissingDocument>> EvaluateAsync(
            string serviceType,
            IReadOnlyCollection<SubmittedDocument> documents,
            CancellationToken ct)
        {
            var rules = await _rules.GetForServiceTypeAsync(serviceType, ct);
            if (rules.Count == 0) return Array.Empty<MissingDocument>();   // e.g. ID_COLLECTION

            var missing = new List<MissingDocument>();
            var now = _clock.UtcNow;

            foreach (var rule in rules.Where(r => r.IsMandatory))
            {
                var provided = documents.FirstOrDefault(d =>
                    string.Equals(d.DocumentType, rule.DocumentType, StringComparison.OrdinalIgnoreCase));

                if (provided is null)
                {
                    missing.Add(new MissingDocument { DocumentType = rule.DocumentType, Reason = "Required" });
                    continue;
                }

                if (rule.MaxCertificationAgeDays is int maxDays)
                {
                    if (provided.CertifiedOnUtc is null)
                    {
                        missing.Add(new MissingDocument
                        {
                            DocumentType = rule.DocumentType,
                            Reason = "Certification date is required"
                        });
                        continue;
                    }

                    if ((now - provided.CertifiedOnUtc.Value).TotalDays > maxDays)
                    {
                        missing.Add(new MissingDocument
                        {
                            DocumentType = rule.DocumentType,
                            Reason = $"Certified too long ago (older than {maxDays} days)"
                        });
                        continue;
                    }
                }

                var accepted = rule.AcceptedFormats
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => s.TrimStart('.').ToLowerInvariant())
                    .ToHashSet();

                var ext = (Path.GetExtension(provided.FileName) ?? string.Empty)
                          .TrimStart('.').ToLowerInvariant();

                if (accepted.Count > 0 && !accepted.Contains(ext))
                {
                    missing.Add(new MissingDocument
                    {
                        DocumentType = rule.DocumentType,
                        Reason = $"Format '{ext}' not accepted; use {string.Join(" or ", accepted)}"
                    });
                }
            }

            return missing;
        }
    }
}
