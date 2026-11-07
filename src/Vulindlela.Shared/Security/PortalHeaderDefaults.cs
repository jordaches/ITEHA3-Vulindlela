namespace Vulindlela.Shared.Security;

public static class PortalHeaderDefaults
{
    public const string Scheme = "PortalHeader";
    public const string CitizenCanAccessOwnRecordPolicy = "CitizenCanAccessOwnRecord";

    public const string InternalKeyHeader = "X-Internal-Key";
    public const string CitizenIdHeader = "X-Citizen-Id";
    public const string RoleHeader = "X-Citizen-Role";

    public const string CitizenRole = "Citizen";
    public const string OfficialRole = "Official";
    public const string SystemRole = "System";   // the Portal when no citizen is signed in
}