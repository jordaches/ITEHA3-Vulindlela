namespace Vulindlela.Shared.Contracts;

public sealed record RegisterAppointmentRequest(
    int AppointmentId,
    int CitizenAccountId,
    string ServiceType);

public sealed record SubmittedDocumentDto(
    string DocumentType,
    string FileName,
    DateTime? CertifiedOnUtc);

public sealed record SubmitDocumentsRequest(
    int AppointmentId,
    int CitizenAccountId,
    string ServiceType,
    IReadOnlyList<SubmittedDocumentDto> Documents);

public sealed record SubmissionAcceptedDto(int SubmissionId, string State);

public sealed record MissingDocumentDto(string DocumentType, string Reason);

public sealed record ClearanceStatusDto(
    int AppointmentId,
    string ServiceType,
    string State,
    IReadOnlyList<MissingDocumentDto> MissingDocuments,
    DateTime? VerifiedAtUtc,
    DateTime UpdatedAtUtc);