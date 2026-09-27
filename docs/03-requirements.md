# CMCS — Requirements Specification

## Document control

| Field | Value |
|-------|-------|
| System | Contract Monthly Claim System (CMCS) |
| Version | 1.0 (Systems Analyst showcase) |
| Status | Baseline aligned to ASP.NET Core 8 implementation |

## Functional requirements

| ID | Requirement | Priority | Role(s) |
|----|-------------|----------|---------|
| FR-01 | The system shall allow a lecturer to register and sign in using ASP.NET Core Identity. | Must | Lecturer |
| FR-02 | The system shall store lecturer profile fields including first name, last name, and hourly rate. | Must | Lecturer |
| FR-03 | The system shall allow a lecturer to create a claim with hours worked (1–200) and optional notes. | Must | Lecturer |
| FR-04 | The system shall calculate claim total amount as `hourly rate × hours worked` at submission time. | Must | Lecturer |
| FR-05 | The system shall set new claims to status **Submitted** (`CurrentStatusID = 1`). | Must | Lecturer |
| FR-06 | The system shall allow lecturers to attach one or more supporting documents on submission. | Must | Lecturer |
| FR-07 | The system shall reject uploads that exceed size limits or use disallowed file extensions. | Must | Lecturer |
| FR-08 | The system shall persist uploaded files under `wwwroot/uploads` with unique file names. | Must | System |
| FR-09 | The system shall record an entry in claim status history when a claim is submitted. | Must | System |
| FR-10 | The system shall provide a coordinator dashboard listing claims in **Submitted** status. | Must | Coordinator |
| FR-11 | The system shall prioritise pending claims using an automation scoring service (hours, amount, documents, history). | Should | Coordinator |
| FR-12 | The system shall allow the coordinator to approve a claim (transition to **ApprovedByCoordinator**). | Must | Coordinator |
| FR-13 | The system shall allow the coordinator to reject a claim (transition to **Rejected**). | Must | Coordinator |
| FR-14 | The system shall allow coordinator batch approval with optional auto-approval for high-confidence claims. | Should | Coordinator |
| FR-15 | The system shall provide a manager dashboard listing **ApprovedByCoordinator** claims. | Must | Academic Manager |
| FR-16 | The system shall allow the academic manager to approve a claim (transition to **ApprovedByManager**). | Must | Academic Manager |
| FR-17 | The system shall allow the academic manager to reject a claim at final stage. | Must | Academic Manager |
| FR-18 | The system shall append status history for every approval/rejection including actor and notes. | Must | Coordinator, Manager |
| FR-19 | The system shall allow authorised users to view claim details including documents and full status history. | Must | All authenticated |
| FR-20 | The system shall restrict claim creation to users in the **Lecturer** role. | Must | System |
| FR-21 | The system shall restrict coordinator actions to the **Coordinator** role. | Must | System |
| FR-22 | The system shall restrict manager final approval actions to the **Manager** role. | Must | System |
| FR-23 | The system shall seed reference statuses: Submitted, ApprovedByCoordinator, ApprovedByManager, Rejected, Paid. | Must | System |
| FR-24 | The system shall seed demo coordinator and manager accounts on first run (development/demo). | Should | System |
| FR-25 | The system shall expose AJAX endpoints for status updates used by approval dashboards. | Must | Coordinator, Manager |
| FR-26 | The system shall allow the manager to generate a summary report of recently approved claims. | Could | Academic Manager |

## Non-functional requirements

| ID | Category | Requirement | Target / note |
|----|----------|-------------|---------------|
| NFR-01 | Security | Authentication required for all claim workflows except Identity public pages. | `[Authorize]` on controllers |
| NFR-02 | Security | Role-based authorization on create, dashboards, and status APIs. | Role attributes per action |
| NFR-03 | Security | Anti-forgery tokens on form POST for claim submission. | `ValidateAntiForgeryToken` |
| NFR-04 | Security | Demo credentials documented only in [seed-accounts.md](./seed-accounts.md), not in public README. | Portfolio hygiene |
| NFR-05 | Data integrity | Foreign keys restrict orphan claims; user delete restricted on claims. | EF `DeleteBehavior.Restrict` |
| NFR-06 | Data integrity | Monetary fields use decimal(18,2) precision. | EF configuration |
| NFR-07 | Usability | Bootstrap-based responsive UI for dashboards and forms. | Bootstrap 5 |
| NFR-07a | Usability | Shared status badges, flash messages, empty states, and client-side claim list filters on lecturer/coordinator dashboards. | See [10-ui-ux-features.md](./10-ui-ux-features.md) |
| NFR-08 | Performance | Coordinator dashboard shall load pending claims in a single query with includes. | EF `Include` patterns |
| NFR-09 | Reliability | Coordinator dashboard falls back to basic list if automation service fails. | Implemented fallback |
| NFR-10 | Maintainability | Business logic for uploads and automation isolated in service classes. | `IFileUploadService`, `IClaimAutomationService` |
| NFR-11 | Testability | Unit tests cover amount calculation, document model, in-memory persistence. | xUnit + InMemory DB |
| NFR-12 | Operability | Application applies EF migrations and seed data on startup. | `SeedData.Initialize` |
| NFR-13 | Compliance | Status history provides audit trail (who, when, status, notes). | `ClaimStatusHistory` |
| NFR-14 | Availability | Target suitable for single-instance institutional deployment; no HA spec in v1. | Future ops doc |

## Business rules

| BR-ID | Rule |
|-------|------|
| BR-01 | Hours worked must be between 1 and 200 inclusive. |
| BR-02 | Total amount is derived from the lecturer's hourly rate at submission; not manually overridden on create. |
| BR-03 | Coordinator may act only on **Submitted** claims (dashboard filter). |
| BR-04 | Manager acts on **ApprovedByCoordinator** claims unless explicitly rejected earlier. |
| BR-05 | Rejected claims remain in **Rejected** until a new claim is submitted (no auto-resubmit in v1). |

## Dependencies

- SQL Server or LocalDB connection string in `appsettings.json`.
- .NET 6 SDK for build and run.
- Browser with JavaScript enabled for AJAX approval actions.

## Open issues / backlog

| Issue | Description |
|-------|-------------|
| OI-01 | Paid status exists in reference data but payment integration is not implemented. |
| OI-02 | Draft save API returns success without durable storage — document as demo-only. |
| OI-03 | HR role seeded in code; HR workflows not in scope for this showcase. |
