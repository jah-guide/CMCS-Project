# CMCS — Use Cases & User Stories

## At a glance

| UC | Name | Actor |
|----|------|-------|
| UC-01 | Register | Lecturer |
| UC-02 | Submit monthly claim | Lecturer |
| UC-03 | Upload documents | Lecturer |
| UC-04–06 | Review / approve / reject | Coordinator |
| UC-07–09 | Final review / approve / reject | Academic Manager |
| UC-10 | View claim detail & history | Authenticated user |
| UC-11 | Batch process claims | Coordinator |
| UC-12 | Generate manager report | Academic Manager |

---

## UC-02 — Submit monthly claim

**Goal:** Record hours and start the approval workflow.

**Happy path:** Open **Create Claim** → enter hours and notes → attach files → system validates, calculates total, sets **Submitted**, writes history → confirmation.

**User story US-02:** As a lecturer, I want one form for my monthly hours so I am not chasing email replies.

| # | Acceptance criterion |
|---|----------------------|
| 1 | Valid hours create a Submitted claim; total = hours × hourly rate |
| 2 | Zero or non-numeric hours show validation errors |
| 3 | Hours above 200 are rejected server-side |
| 4 | Valid PDF within size limits is stored on the claim |
| 5 | Oversized files are rejected per upload rules |

---

## UC-05 — Coordinator approve

**User story US-05:** As a coordinator, I approve compliant claims so managers only see vetted work.

| # | Acceptance criterion |
|---|----------------------|
| 1 | Approve moves Submitted → ApprovedByCoordinator with history |
| 2 | Non-coordinators cannot call approve actions |
| 3 | Auto-approve (when enabled) follows automation rules and logs a note |

---

## UC-08 — Manager approve

**User story US-08:** As an academic manager, I finalise coordinator-approved claims for payment.

| # | Acceptance criterion |
|---|----------------------|
| 1 | Approve moves ApprovedByCoordinator → ApprovedByManager |
| 2 | Submitted claims do not appear on the manager queue |
| 3 | Reject with reason sets Rejected and shows notes in history |

---

## UC-10 — Claim detail & history

**User story US-10:** As a stakeholder, I view documents and status changes for audit queries.

| # | Acceptance criterion |
|---|----------------------|
| 1 | Details lists all linked documents |
| 2 | History is ordered with status, actor, date, notes |
| 3 | Anonymous users are redirected to login |

---

## Other stories (short form)

| ID | Summary |
|----|---------|
| US-06 | Coordinator reject with notes; documents retained |
| US-11 | Batch returns counts of auto-advanced vs manual review |
| US-12 | Manager report download with totals for the period |
| US-SEC-01 | Lecturer cannot open coordinator/manager dashboards |
| US-SEC-02 | POST without antiforgery token fails |
| US-AUD-01 | Each transition creates one history row with actor |

Full test mapping: [08-traceability-matrix.md](./08-traceability-matrix.md).
