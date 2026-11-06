# ADR 0001: Separate Booking and Clearance services, each with its own database

- **Status:** Accepted
- **Date:** Phase 1, week 1

## Context

The Agency's Phase 1 has two capabilities: a citizen can see real availability
and book a counter slot, and a citizen can submit documents in advance and be
told before travelling whether anything is missing. The legacy BranchDesk puts
booking, document checks, SMS, and the head-office export in one method and one
database per branch, and the March incident showed that one failing component
can take everything down.

The Agency has set three non-negotiable constraints: booking and document
clearance are separate services with separate databases and neither may read the
other's tables; a Clearance failure must not stop a citizen booking; every read
of a citizen's record must be logged and visible to the citizen.

## Decision

Build two independently runnable ASP.NET Core Web APIs.

**Booking** owns branches, slots, appointments, citizen accounts, and the
booking-side access log in `booking.db`. It enforces the one-slot-one-citizen
guarantee with a unique index in its own database.

**Clearance** owns document rules, submissions, verification outcomes, clearance
status, and the clearance-side access log in `clearance.db`. It drains a
`Channel<T>` on a `BackgroundService`.

They communicate over HTTP using DTOs from `Vulindlela.Shared`. Booking notifies
Clearance when an appointment is created. The Portal calls both to compose
views. Neither service holds the other's connection string, and Booking never
queries Clearance's database.

## Alternatives considered

**One service with one database.** Rejected. It is simpler to build and would
satisfy the booking feature alone, but it reintroduces the March incident: a slow
OCR transaction or a locked table in the document path would block booking. It
also makes the Agency's "separate databases" constraint impossible to
demonstrate, and D3.1 explicitly tests that Booking never queries Clearance's
data.

**Two services sharing one database with separate schemas.** Rejected. It keeps
the codebases apart but the failure domain, backup domain, and migration domain
together. A database-level lock or a schema migration affects both. "One .db
file per service makes separate ownership physical" is the point; a shared schema
is a boundary on paper only.

**Two services communicating through a message broker (RabbitMQ or Azure Service
Bus).** Rejected for Phase 1. It is the right long-term choice for the
notification, and D3.2's `Channel<T>` behind an interface is deliberately shaped
so the swap is one class. But a broker requires an install which is out of scope
and the Phase 1 notification is one-way and low-volume.

**Three services, with a separate Auth service.** Rejected for Phase 1.
Authentication is out of scope for Deliverable 1 and a third database adds 
operational cost with no Phase 1 requirement. Auth lives with Booking because 
Booking owns the citizen's appointments and therefore the identity that books 
them. Phase 2 could split it.

## Consequences

**What we gain.** Failure isolation is physical: stopping Clearance leaves
Booking and the Portal working (D3.4). The unique index that prevents double
booking lives in Booking's database alone, uncontended by document work. Each
database can be backed up, migrated, and deleted independently. The boundary is
visible as two files on disk.

**What we accept.** More moving parts than one service. The Portal must compose
two calls and degrade gracefully when one is down (D3.5). The notification is not
durable without an outbox: if Booking crashes between the appointment insert and
the HTTP call, Clearance never hears. The accepted mitigation is an outbox row
written in the same transaction as the appointment, drained by a background
worker that retries the notification. Cross-service queries are impossible by
design, so any report needing both facts must be composed at read time. Local
development runs two processes, not one.

## Evidence

Stop Clearance, book a slot, screenshot the
confirmation, and screenshot the My Appointments page showing the degradation
message rather than an error page.