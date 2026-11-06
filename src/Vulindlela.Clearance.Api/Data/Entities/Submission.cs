namespace Vulindlela.Clearance.Api.Data.Entities
{
    public class Submission
    {
        public int Id { get; set; }
        public int ClearanceStatusId { get; set; }
        public ClearanceStatus ClearanceStatus { get; set; } = null!;
        public DateTime SubmittedAtUtc { get; set; }
        public ICollection<SubmittedDocument> Documents { get; set; } = new List<SubmittedDocument>();
    }
}
