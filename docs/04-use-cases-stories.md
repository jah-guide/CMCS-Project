# CMCS — Use Cases & User Stories

## Use case catalogue

| UC-ID | Name | Primary actor | Preconditions |
|-------|------|---------------|---------------|
| UC-01 | Register lecturer account | Lecturer | Identity registration enabled |
| UC-02 | Submit monthly claim | Lecturer | Signed in as Lecturer; hourly rate set |
| UC-03 | Upload supporting documents | Lecturer | Claim form in progress or create POST |
| UC-04 | Review submitted claims | Coordinator | Signed in as Coordinator |
| UC-05 | Approve claim (coordinator) | Coordinator | Claim status = Submitted |
| UC-06 | Reject claim (coordinator) | Coordinator | Claim status = Submitted |
| UC-07 | Review coordinator-approved claims | Academic Manager | Signed in as Manager |
| UC-08 | Approve claim (manager) | Academic Manager | Claim status = ApprovedByCoordinator |
| UC-09 | Reject claim (manager) | Academic Manager | Claim status = ApprovedByCoordinator |
| UC-10 | View claim detail & history | Authenticated user | Claim exists; user authorised to view |
| UC-11 | Batch process claims | Coordinator | Pending claims selected |
| UC-12 | Generate manager report | Academic Manager | Approved claims exist |

---

## UC-02 — Submit monthly claim

**Primary actor:** Lecturer  
**Goal:** Record hours for the month and initiate approval workflow.

**Main success scenario**

1. Lecturer opens **Create Claim**.
2. System displays profile context (hourly rate).
3. Lecturer enters hours worked and optional notes.
4. Lecturer attaches supporting documents (optional but recommended).
5. System validates hours and files.
6. System calculates total amount, persists claim as **Submitted**, stores documents, writes history.
7. System confirms success and redirects to home.

**Extensions**

- 3a. Invalid hours → error message, form redisplayed.
- 4a. Invalid file type/size → file skipped or submission fails with message.

### User story US-02

> **As a** lecturer, **I want to** submit my monthly hours in one form **so that** I am paid on time without email follow-ups.

**Acceptance criteria**

1. Given I am logged in as a Lecturer, when I submit 40 hours with valid data, then a claim is created with status Submitted and total = 40 × my hourly rate.
2. Given I enter 0 or non-numeric hours, when I submit, then I see a validation error and no claim is created.
3. Given I enter 201 hours, when I submit, then server-side validation prevents save (model range 1–200).
4. Given I upload a 2 MB PDF, when I submit, then the file is stored and linked to the claim.
5. Given I upload a 10 MB file, when I submit, then the file is not accepted per upload rules.

---

## UC-05 — Approve claim (coordinator)

**Primary actor:** Programme Coordinator  
**Goal:** Move a valid claim to the manager queue.

### User story US-05

> **As a** coordinator, **I want to** approve compliant claims **so that** managers only see vetted submissions.

**Acceptance criteria**

1. Given a claim in Submitted status, when I approve it, then status becomes ApprovedByCoordinator and history records my user id and timestamp.
2. Given I am not in the Coordinator role, when I call the approve API, then access is denied.
3. Given the automation score is high and rules match, when I use auto-approve, then the claim moves to ApprovedByCoordinator with an auto-approval note.

---

## UC-08 — Approve claim (manager)

**Primary actor:** Academic Manager  
**Goal:** Provide final academic approval before payment processing.

### User story US-08

> **As an** academic manager, **I want to** finalise coordinator-approved claims **so that** finance can pay lecturers confidently.

**Acceptance criteria**

1. Given a claim in ApprovedByCoordinator status, when I approve, then status becomes ApprovedByManager.
2. Given a claim still in Submitted status, when I open the manager dashboard, then it does not appear in the default queue.
3. Given I reject with a reason, when I save, then status is Rejected and notes appear in history.

---

## UC-10 — View claim detail & history

### User story US-10

> **As a** stakeholder, **I want to** see documents and status changes **so that** I can answer audit or payment queries.

**Acceptance criteria**

1. Given a claim with two documents, when I open Details, then both file names and upload dates are listed.
2. Given multiple status changes, when I view history, then entries are ordered with status name, actor, date, and notes.
3. Given I am unauthenticated, when I request Details, then I am redirected to login.

---

## UC-06 — Reject claim (coordinator)

### User story US-06

> **As a** coordinator, **I want to** reject incorrect claims with notes **so that** lecturers know what to fix.

**Acceptance criteria**

1. When I reject with notes, then status is Rejected and lecturer can see the reason in history on Details.
2. Rejection does not delete uploaded documents.

---

## UC-11 — Batch process claims

### User story US-11

> **As a** coordinator, **I want to** process multiple claims **so that** peak-month intake is manageable.

**Acceptance criteria**

1. When I submit a batch of claim IDs, then the system returns counts of approved vs needs manual review.
2. High-confidence claims may be auto-advanced per automation rules; others remain for manual action.

---

## UC-12 — Generate manager report

### User story US-12

> **As an** academic manager, **I want a** summary of approved claims **so that** I can reconcile monthly totals.

**Acceptance criteria**

1. When I generate a report, then I receive a downloadable text summary with lecturer names, amounts, and aggregate total for the selected period logic in the controller.

---

## Cross-cutting stories

| US-ID | Story | Acceptance criteria (summary) |
|-------|-------|-------------------------------|
| US-SEC-01 | Role enforcement | Lecturer cannot access coordinator/manager dashboards (403 or redirect). |
| US-SEC-02 | CSRF protection | Form POST without token fails antiforgery validation. |
| US-AUD-01 | Audit trail | Every status transition creates exactly one `ClaimStatusHistory` row with actor. |

## Traceability

Detailed mapping to tests: [08-traceability-matrix.md](./08-traceability-matrix.md).
