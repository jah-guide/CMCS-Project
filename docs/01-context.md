# CMCS — Business Context

## Purpose of this document

Establishes the business problem, scope, and success criteria for the **Contract Monthly Claim System (CMCS)**. It anchors all downstream requirements, use cases, and acceptance tests.

## Executive summary

Independent contractor lecturers at a higher-education institution submit **monthly claims** for hours worked. Before CMCS, claims moved through informal paper trails and email chains, causing delays, lost attachments, and weak audit evidence. CMCS digitises submission, supporting documents, and a **two-stage approval workflow** (Programme Coordinator → Academic Manager) with transparent status history.

## Problem statement

| Pain point | Impact |
|------------|--------|
| Paper/email submissions | Incomplete packs, version confusion, no single source of truth |
| Manual routing | Coordinators and managers cannot see a unified queue |
| Weak audit trail | Disputes over who approved what and when |
| Delayed payment | Lecturers chase status; finance lacks timely approved totals |

## Business objectives

1. **Reduce cycle time** from submission to manager approval for standard claims.
2. **Improve compliance** by requiring supporting documents and recording every status change.
3. **Increase transparency** so lecturers can view claim status and history.
4. **Support governance** through role-based access aligned to academic hierarchy.

## Scope

### In scope

- Lecturer self-registration and profile (including hourly rate used for amount calculation).
- Claim creation: hours worked, auto-calculated total amount, optional notes.
- Supporting document upload (validated type and size).
- Coordinator review of **Submitted** claims (including prioritisation/scoring aids).
- Academic Manager review of **Coordinator-approved** claims.
- Rejection at either approval stage with notes captured in history.
- Status tracking and claim detail views with full history.
- Seed data for demo roles on local development only.

### Out of scope (current release)

- Integration with payroll/ERP systems.
- Production email/SMS notifications (logged/simulated only).
- Multi-campus policy engines beyond basic validation rules.
- HR role workflows beyond seeded demo account (role exists in codebase for extension).

## Assumptions and constraints

- Users authenticate via **ASP.NET Core Identity**; lecturers register through the app UI.
- Database is **SQL Server** (LocalDB in development).
- Currency and amounts use **ZAR (R)** formatting in the UI.
- Hours worked must fall within **1–200** per claim (enforced on the `Claim` model).
- File uploads: max **5 MB**, extensions **PDF, DOCX, XLSX, JPG, PNG**.

## Success measures

| Measure | Target (pilot) |
|---------|----------------|
| Lecturers can submit a claim with documents in one session | ≥ 95% of test scenarios pass |
| Every approval/rejection writes to status history | 100% in acceptance tests |
| Role isolation (lecturer cannot approve) | Enforced on all protected actions |
| Automated unit tests for core calculations | Green on CI/local `dotnet test` |

## Related artefacts

| Document | Description |
|----------|-------------|
| [02-stakeholders-raci.md](./02-stakeholders-raci.md) | Stakeholders and RACI |
| [03-requirements.md](./03-requirements.md) | Functional and non-functional requirements |
| [05-process-as-is-to-be.md](./05-process-as-is-to-be.md) | Process comparison |
| [seed-accounts.md](./seed-accounts.md) | Local demo accounts (not for production) |

## Glossary

| Term | Definition |
|------|------------|
| Claim | Monthly record of hours worked and payable amount for one lecturer |
| Supporting document | Evidence file attached to a claim (timesheet, invoice scan, etc.) |
| Coordinator | Programme-level approver (Identity role: `Coordinator`) |
| Academic Manager | Final academic approver (Identity role: `Manager`) |
| Status | Lifecycle state of a claim (Submitted → approvals → Paid/Rejected) |
