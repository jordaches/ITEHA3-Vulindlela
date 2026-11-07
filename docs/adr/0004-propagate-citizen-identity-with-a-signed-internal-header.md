# ADR 0004: Propagate citizen identity from the Portal to the APIs with a signed internal header

- **Status:** Accepted
- **Date:** Phase 1, week 3

## Context

Three processes run in Phase 1: the Portal (:9001), the Booking API (:9002), and the
Clearance API (:9003). A citizen signs in once, at the Portal. Both APIs then need
to know who is calling, because three requirements depend on it:

- a citizen may view only their own appointments and their own access log (D4.2);
- an official may view any citizen's record (D4.2);
- every read of a citizen's record is logged with the reading party's identity (D4.3).

The APIs are bound to `localhost` in Phase 1 and the Portal is their only intended
caller, but nothing in code enforces that. The question is how the citizen's
identity crosses the two HTTP boundaries without turning the APIs into
session-aware web applications.

The brief prescribes cookie authentication or ASP.NET Core Identity for the
Portal's own sign-in, permits no secret as a literal in a `.cs` file, requires a
policy that compares the identity in the request against the identity on the
session, and requires the solution to run from a clean clone with no external
identity provider.

## Decision

**Portal side.** The Portal authenticates the citizen with cookie authentication.
On successful sign-in it writes two claims to the cookie: `ClaimTypes.NameIdentifier`
(the citizen's `CitizenAccountId`) and `ClaimTypes.Role` (`"Citizen"` or `"Official"`).

**Portal to API.** Every server-side HttpClient call from the Portal to Booking or
Clearance passes three headers, added by a `DelegatingHandler` that reads the
current `HttpContext.User`:

    X-Citizen-Id:     <numeric citizen id>
    X-Citizen-Role:   Citizen | Official
    X-Internal-Key:   <shared key, loaded from configuration>

**API side.** Booking and Clearance each register a custom authentication scheme
(`"PortalHeader"`). Its handler:

1. reads `X-Internal-Key` and compares it to the configured key using
   `CryptographicOperations.FixedTimeEquals`; if missing or wrong, no identity is
   set and the request fails any `[Authorize]` endpoint with 401;
2. parses `X-Citizen-Id` and `X-Citizen-Role`, and produces a `ClaimsPrincipal`
   with `NameIdentifier` and `Role` claims.

That scheme is the default authentication scheme in both APIs. A standard
authorization policy — `CitizenCanAccessOwnRecord` — is registered in both, with
an `IAuthorizationRequirement` and a handler that either matches the request's
`NameIdentifier` claim against the resource's owner id, **or** accepts
`Role == "Official"`.

Routes are annotated accordingly. `[AllowAnonymous]` on sign-in, branch listing,
slot listing, and the service-type listing. `[Authorize(Policy = "CitizenCanAccessOwnRecord")]`
on anything that reads a citizen's own data. `[Authorize(Roles = "Official")]` on
the official-view routes.

## Alternatives considered

**Cookie-sharing with a shared Data Protection key ring.** All three processes
configure the same `SetApplicationName` and `PersistKeysToFileSystem`, so one
sign-in cookie at `localhost:9001` is presented and accepted at `localhost:9002`
and `localhost:9003`. The Portal's HttpClient forwards the cookie.

Rejected. The APIs are stateless resource servers, not sessions. Carrying the
cookie to them forces the APIs to participate in sliding expiry, SameSite and
antiforgery, and a state-changing `POST` accepted on a cookie-authenticated API
requires CSRF protection — a real security concern that the header approach does
not introduce. The key ring also becomes a shared secret on disk that two
developers must sync, which makes "clone and run" harder, not easier.

**A real OAuth 2.0 / OpenID Connect provider (Keycloak, IdentityServer, Auth0).**
Rejected. It is the clean production answer and it is the right Phase 2 direction.
For Phase 1 it is a fourth running service with its own database, which the brief's
technology stack and constraints ("no database server, a message broker, a cloud
subscription or a credit card") explicitly exclude. It would dominate six weeks.

**JWT issued by the Portal, validated by the APIs.** The Portal signs a short-lived
JWT after sign-in with a shared HMAC key from configuration; Booking and Clearance
validate the signature and read the claims from the token. This is the strongest of
the rejected options and would be the choice for a longer project. Rejected for
Phase 1 because it requires `Microsoft.AspNetCore.Authentication.JwtBearer` or
`System.IdentityModel.Tokens.Jwt`, neither of which is prescribed by the brief nor
taught in the module, and it introduces token lifetime, clock skew and key rotation
as topics to defend. For three processes on one machine, where the Portal is the
only caller, the shared key gives the same trust boundary as the JWT signature.
This is the option to revisit in Phase 2 if the system leaves `localhost`.

## Consequences

**What we gain.** The APIs stay stateless: no cookie machinery, no CSRF surface, no
sliding expiry, no session store. The identity arrives in one header bundle, so
every access-log write is a single line that reads `HttpContext.User`. The policy
is a standard `AuthorizationHandler` reading `HttpContext.User`, not a homegrown
middleware. The shared key is one configuration value, rotated independently of
the code, and read via `IConfiguration` — never a literal in a `.cs` file.

**What we accept.**

- **The APIs trust the Portal.** Anyone who can reach port 9002 or 9003 *and*
  knows the shared key can impersonate any citizen. In Phase 1 both APIs bind to
  `localhost` and the key is loaded from the environment, so the attack surface is
  the developer's machine. In production this design requires either mutual TLS
  between the Portal and the APIs, or the JWT option above. This is stated in the
  README's Known Limitations.
- **The audit log records what the Portal said, not what is cryptographically
  true.** When the citizen opens "who looked at my record", the entries show the
  citizen id that the Portal asserted — not a signed token that proves it. The
  Tallinn comparison is therefore weaker than it is with JWTs, and the reflection
  says so.
- **Configuration carries a secret.** The key is read from 
  IConfiguration["Security:InternalKey"]. ASP.NET Core's configuration pipeline 
  resolves this from environment variables first, then appsettings.json, then 
  appsettings.Development.json. In Development, the developer supplies it by 
  creating appsettings.Development.json (gitignored) with a Security:InternalKey 
  value. In any other environment, the VULINDLELA_INTERNAL_KEY environment variable 
  is the only allowed source. The process fails fast at start-up if neither source 
  supplies a value. This makes "clone and run" require one documented file for the 
  lecturer, while making production deployment fail loudly rather than silently 
  trusting callers.. It is never written to source, never logged, and the README 
  documents the two-line PowerShell to set it before each session. A misconfigured 
  deployment fails loudly rather than silently trusting callers.
- **The Portal's `DelegatingHandler` depends on `IHttpContextAccessor`,** so it is
  only usable from request-handling code. A background service in the Portal
  could not use the same client. That is not a Phase 1 requirement; if it becomes
  one, the background code must resolve the identity differently.

## Evidence

- Phase D: the Portal renders its My Appointments page only for the signed-in
  citizen; calling `GET /api/appointments/citizen/{otherId}` directly with a
  different `X-Citizen-Id` returns the other citizen's data, proving the policy is
  on the API, not the Portal.
- Phase E: a request with the correct citizen header but no `X-Internal-Key`
  returns 401, and a request with the wrong key returns 401.
- Phase E: the access log shows `CallerCitizenId` and `CallerRole` for every read,
  and the citizen's "who looked at my record" page renders exactly those rows.