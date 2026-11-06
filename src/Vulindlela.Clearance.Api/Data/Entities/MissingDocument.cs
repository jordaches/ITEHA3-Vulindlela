namespace Vulindlela.Clearance.Api.Data.Entities
{
    public class MissingDocument
    {
        public int Id { get; set; }
        public int ClearanceStatusId { get; set; }
        public ClearanceStatus ClearanceStatus { get; set; } = null!;
        public string DocumentType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
