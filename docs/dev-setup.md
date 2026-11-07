# Development setup

## Internal service key

The Booking API, the Clearance API, and the Portal trust each other by a shared
key carried in an `X-Internal-Key` header. The key is never in source. Read
`docs/adr/0004-propagate-citizen-identity-with-a-signed-internal-header.md` for
the reasoning and the accepted cost.

### What to do

1. Generate a key (PowerShell):

   ```powershell
   [guid]::NewGuid().ToString("N") + [guid]::NewGuid().ToString("N")

2. Copy the value once. It is the same for all three services

3. Create three files with identical contents:

    src/Vulindlela.Booking.Api/appsettings.Development.json

    src/Vulindlela.Clearance.Api/appsettings.Development.json

    src/Vulindlela.Portal/appsettings.Development.json

    In each: 
    {
        "Security": { "InternalKey": "<paste the value here>" }
    }

These files are gitignored. If you skip this step, each service fails at
start-up with a clear message naming the missing configuration key.

#### Non-Development environments
Set the VULINDLELA_INTERNAL_KEY environment variable. It overrides
appsettings.Development.json.

# Demo accounts (seeded by Booking on first run)
Account	ID Number	Password	Role
Thandi Mokoena	9001015800081	Passw0rd!	Citizen
Sipho Ndlovu	8802025800082	Passw0rd!	Citizen
Anele Dlamini	9203035800083	Passw0rd!	Citizen
Demo Official	7001015800080	Official123!	Official

### `appsettings.Development.json.example` (committed at repo root)
```json
{
  "Security": {
    "InternalKey": "<generate a new GUID, see docs/dev-setup.md>"
  }
}