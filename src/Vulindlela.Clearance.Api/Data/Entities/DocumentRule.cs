namespace Vulindlela.Clearance.Api.Data.Entities;

public class DocumentRule
{
    public int Id { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public int? MaxCertificationAgeDays { get; set; }
    public string AcceptedFormats { get; set; } = "pdf,jpg";
}
