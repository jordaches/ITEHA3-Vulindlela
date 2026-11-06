namespace Vulindlela.Clearance.Api.Data.Entities
{
    public class SubmittedDocument
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public Submission Submission { get; set; } = null!;
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime? CertifiedOnUtc { get; set; }
    }
}