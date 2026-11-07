namespace Vulindlela.Shared.Contracts;

public sealed record VerifyPasswordRequest(string IdNumber, string Password);

public sealed record VerifyPasswordResponse(
    int CitizenId,
    string FullName,
    string Role,
    string IdNumber);