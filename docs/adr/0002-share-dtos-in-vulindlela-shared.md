# ADR 0002: Share cross-boundary DTOs in Vulindlela.Shared rather than duplicate them

- **Status:** Accepted
- **Date:** Phase 1, week 1

## Context

The Portal calls both services, and Booking calls Clearance. Each call needs a
request and response shape: a slot DTO, an appointment DTO, a document
submission DTO, a clearance status DTO. `SETUP.md` creates `Vulindlela.Shared`
and notes that sharing DTOs is tidier while duplicating them is more honest about
the boundary, and that either choice is worth an ADR. The risk of sharing is
coupling: a change to a shared DTO forces a coordinated release of both services,
which is the opposite of independent deployability.

## Decision

Put the cross-boundary DTOs in `Vulindlela.Shared` as plain records. Each service
and the Portal reference the shared library. The DTOs are contracts, not
entities: no EF attributes, no behaviour, no navigation properties. Each
service's EF entities stay private to that service.

## Alternatives considered

**Duplicate the DTOs in each service.** Rejected. It is more honest about the
boundary and lets each service evolve its wire format independently, but it
doubles the maintenance and invites drift: a field renamed in Booking's copy but
not Clearance's copy compiles fine and fails at runtime. For a six-week Phase 1
with one team, the coordination cost of a shared library is lower than the drift
cost of duplication.

**Share the EF entities.** Rejected outright. It leaks each service's schema
across the boundary, reintroduces the shared-database coupling the boundary
exists to prevent, and makes it impossible to change a column without a
coordinated release. It also violates D3.1's rule that Booking must never query
Clearance's database.

**Generate clients from OpenAPI (NSwag or Refit).** Rejected for Phase 1. It is
the production-grade answer and would keep each service's contract independent.
It adds a code-generation step and a tool the marker would have to install, and
Phase 1's contracts are small enough to hand-write.

## Consequences

**What we gain.** One definition of each contract. The compiler catches a field
mismatch at build time rather than at runtime. The Portal and both services agree
on the shape of a slot, an appointment, and a clearance status without copying
code.

**What we accept.** A change to a shared DTO is a change to all three projects at
once. This is a real reduction in independent deployability. It is acceptable in
Phase 1 because one team owns all three and the DTOs are small; it would be
unacceptable across organisational boundaries. The shared library must stay free
of behaviour and EF attributes, or it becomes a shared entity model by stealth.
That is a discipline cost, not a technical one.

## Evidence

All three projects build against one definition of each DTO; no DTO is
redeclared in a service project.