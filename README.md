# Vulindlela — Phase 1
## The Problem
Every weekday morning, citizens queue outside National Civic Services Agency branches from 03:00. One in three is turned away at the counter because of a missing or expired document. The legacy system (BranchDesk 2011) runs on isolated, nightly-synced branch databases, meaning real-time capacity is invisible and document checks happen only after hours of waiting.

Vulindlela Phase 1 solves this by moving the two things that cause the queue—the booking and the document check—online before the citizen travels.

## Architecture
Phase 1 consists of three independently runnable ASP.NET Core projects and two isolated SQLite databases. It deliberately does not replace BranchDesk; it runs alongside it.

The Portal composes data from two completely separate services at read time. If the Clearance service goes offline, the Booking service and the Portal continue to function (graceful degradation).

```text
       [ Citizen ]
            │
            ▼ (HTTPS, cookie auth)
   ┌────────────────────────┐
   │  Vulindlela.Portal     │
   │  ASP.NET Core MVC      │
   │  Port 9001             │
   │  (No business database)│
   └────┬──────────────┬────┘
        │              │
  HTTP+JSON      HTTP+JSON
  (Headers)      (Headers)
        │              │
        ▼              ▼
┌───────────────┐  ┌─────────────────────────┐
│ Booking.Api   │  │ Clearance.Api           │
│ Port 9002     │  │ Port 9003               │
│               │  │                         │
│ - Slots       │  │ - Channel<T> Queue      │
│ - Appts       │  │ - BackgroundService     │
│ - Auth        │  │ - Document Verification │
│ - Access Log  │  │ - Access Log            │
└──────┬────────┘  └────────────┬────────────┘
       │                        │
       ▼                        ▼
┌───────────────┐  ┌─────────────────────────┐
│ booking.db    │  │ clearance.db            │
│ (SQLite)      │  │ (SQLite)                │
└───────────────┘  └─────────────────────────┘
```

## How to Run
You need the .NET 8 SDK (or later) and a terminal. No Docker, SQL Server, or external services are required.

1. Clone the repository and navigate to the root folder.

2. Configure the Internal Key:
The services trust each other via a shared key. Read docs/dev-setup.md. In short, 
create appsettings.Development.json in Booking.Api, Clearance.Api, and Portal with:

```json
{
  "Security": { "InternalKey": "dev-local-key-do-not-commit-1234567890" }
}
```
3. Run the three projects in three separate terminal windows:
```powershell
#Powershell
dotnet run --project src/Vulindlela.Booking.Api     # Starts on http://localhost:9002
dotnet run --project src/Vulindlela.Clearance.Api   # Starts on http://localhost:9003
dotnet run --project src/Vulindlela.Portal          # Starts on http://localhost:9001
```
4. Open http://localhost:9001 in your browser.

## Seeded Demonstration Accounts
The databases are created and seeded automatically on the first run.

| Name                    | ID/Login                | Number          | Password            | Role
|-------------------------|-------------------------|-----------------|---------------------|------
| Queen Moroka            | 8901015800081           | 0821110851      | 1P@ssword           | Citizen
| Jack Mabaso             | 8802025800082           | 0821110002      | 1P@ssword           | Citizen
| Sibusiso Dlomo          | 8303035800083           | 0821133003      | 1P@ssword           | Citizen
| Kenny Mashaba           | 8703035800084           | 0821110293      | 1P@ssword           | Citizen
| Georgie Zamdela         | 9203035800085           | 0821110003      | Official123!        | Official
| Rajesh Kumar            | 9203035802285           | 0821111203      | Official123!        | Official
| Agnes Matabane          | 9203035809886           | 0821110163      | Official123!        | Official
| Demo Official           | 7001015800080           | 0821110099      | Official123!        | Official


## Things to Try
Here is how to see the interesting architectural behaviour in action:

### 1. The Double-Booking Guarantee:
Open two different browsers (or one normal, one incognito). Sign in as Queen in one and Jack in the other. 
Search for the exact same available slot and click "Book" at the same time. One will succeed; the other will be 
rejected by the database's unique constraint.

### 2. Asynchronous Background Work:
Book a slot. Go to "My Appointments". You will see the Clearance status starts as Not registered (or PendingVerification). 
If you wait 3–5 seconds and refresh, the BackgroundService will have finished its simulated OCR/document check and 
updated the status to AwaitingDocuments or MissingDocuments.

### 3. Fault Isolation (Graceful Degradation):
While the system is running, stop the Clearance service (Ctrl+C in its terminal window). Go back to the Portal 
and click "My Appointments". The page will still load, show the booking, and display a yellow warning banner. It will not crash.

### 4. Policy-Based Authorisation & The Audit Log:
Sign in as Queen, view your appointments, and check "My Access Log". You will see yourself. Sign out and sign in as the Official. 
View Queen's appointments using her ID number. Sign back in as Queen and look at your 
Access Log again—you will see that the Official looked at your file, exactly when, and which file.

## Known Limitations
Here is what Phase 1 does not do:

- No actual file storage: The Clearance service verifies metadata (document type, file extension, certification date) 
to prove the asynchronous workflow. It does not accept or store actual file streams, nor does it perform OCR.

- Certification rules are hardcoded: The 90-day limit for certified documents is currently seeded in the database. 
A prodution system should integrate with a rules engine.

- SQLite Limitations: SQLite handles one writer at a time. While the unique index guarantees no double-booking, 
it would not handle the write volume of the entire 218-branch estate in production.
Phase 2 would move to PostgreSQL, MSSQL Server or MYSQL which have proven horizontal scaling strategies.

- The shared internal key: The API-to-API trust is established via a shared internal key. In production,
this should be replaced by Mutual TLS (mTLS) or a proper OAuth2/OIDC token exchange.

- No legacy data migration: This phase runs beside BranchDesk 2011. 
It does not import historical citizen records or migrate the nightly CSV exports.
