namespace Vulindlela.Clearance.Api.Data.Entities
{
    public class ClearanceStatus
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public ClearanceState State { get; set; } = ClearanceState.AwaitingDocuments;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? VerifiedAtUtc { get; set; }
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public ICollection<MissingDocument> MissingDocuments { get; set; } = new List<MissingDocument>();
    }
}
