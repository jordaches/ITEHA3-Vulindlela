namespace Vulindlela.Clearance.Api.Data.Entities
{
    public enum ClearanceState
    {
        AwaitingDocuments = 0,
        PendingVerification = 1,
        Verified = 2,
        MissingDocuments = 3,
        Failed = 4,
        NotRequired = 5
    }
}
