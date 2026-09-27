# CMCS — Acceptance Tests

## Test conventions

| Prefix | Meaning |
|--------|---------|
| AT-xx | Manual or end-to-end acceptance test |
| UT-xx | Automated unit test in `ContractMonthlyClaimSystem.Tests` |

**Environment:** Local development with SQL LocalDB, HTTPS profile, seed data applied on startup.

**Demo accounts:** See [seed-accounts.md](./seed-accounts.md) — local only.

---

## Identity & registration

### AT-01 — Lecturer registration

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Browse to Register | Registration form displayed |
| 2 | Complete required fields including hourly rate | Account created |
| 3 | Sign in | Home/dashboard accessible |

### AT-02 — Unauthenticated access

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Open `/Claims/Create` while logged out | Redirect to login |

### AT-03 — Profile hourly rate used

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Register with hourly rate 250 | Rate stored on user |
| 2 | Submit 10 hours | Total amount R 2,500.00 |

---

## Claim submission (Lecturer)

### AT-04 — Happy path submit

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Login as Lecturer | Create claim available |
| 2 | Enter 20 hours, note "March lectures" | Form accepts input |
| 3 | Submit | Success message; claim status Submitted |

### AT-05 — Invalid zero hours

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Submit 0 hours | Error; no new claim |

### AT-06 — Hours above maximum

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Attempt 201 hours | Validation failure (range 1–200) |

### AT-07 — Amount calculation

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Rate 200, hours 10 | Total 2000 |

**Automated:** UT-01 `Claim_TotalAmount_Calculation_Correct`

### AT-08 — Document upload success

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Attach valid PDF under 5 MB | File linked on claim details |

### AT-09 — Reject oversize file

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Attach file > 5 MB | File not stored |

### AT-10 — Reject disallowed extension

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Attach `.exe` or `.zip` | File rejected by validation |

### AT-11 — History on submit

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | After submit, open Details | History contains Submitted entry with lecturer as actor |

---

## Coordinator workflow

### AT-12 — Dashboard queue

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Login as Coordinator | Coordinator dashboard loads |
| 2 | With pending claims | Only Submitted claims listed |

### AT-13 — Prioritisation visible

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | View dashboard | Score/priority/recommendation columns or analysis available |

### AT-14 — Approve claim

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Approve pending claim | Status ApprovedByCoordinator; history updated |

### AT-15 — Reject claim

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Reject with note "Missing timesheet" | Status Rejected; note in history |

### AT-16 — Batch approval

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Select multiple IDs, run batch | JSON response with approved/review counts |

---

## Manager workflow

### AT-17 — Manager queue filter

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Login as Manager | Dashboard shows coordinator-approved only |

### AT-18 — Final approval

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Approve claim from queue | Status ApprovedByManager |

### AT-19 — Manager rejection

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Reject with reason | Status Rejected |

### AT-26 — Manager report

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Generate report | Downloadable summary with totals |

---

## View & security

### AT-20 — Claim details

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Open Details for claim | Documents + ordered history displayed |

### AT-21 — Lecturer cannot create without role

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | User without Lecturer role hits Create | Access denied |

### AT-22 — Non-coordinator blocked from coordinator dashboard

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Lecturer opens coordinator URL | Forbidden or redirect |

### AT-23 — Non-manager blocked from manager dashboard

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Coordinator opens manager URL | Forbidden or redirect |

### AT-27 — Authenticated controller

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Anonymous access to Claims actions | Login required |

### AT-28 — CSRF on create

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | POST create without antiforgery token | Request rejected |

---

## Seed & reference data

### AT-24 — Status reference

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Inspect database or UI labels | Five statuses present per spec |

**Automated:** UT-04 `Status_Initialization_Works`

### AT-25 — Seed on startup

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Fresh run application | Migrations applied; roles and demo users exist |

---

## Automated unit test catalogue

| UT-ID | Test name | Validates |
|-------|-----------|-----------|
| UT-01 | `Claim_TotalAmount_Calculation_Correct` | FR-04 |
| UT-02 | `SupportingDocument_Properties_Set_Correctly` | FR-06 model |
| UT-03 | `Claim_Can_Be_Saved_To_InMemory_Database` | FR-03 persistence |
| UT-04 | `Status_Initialization_Works` | FR-23 |

Run:

```bash
dotnet test ContractMonthlyClaimSystem.sln
```

---

## Sign-off checklist

| Role | Criteria | Pass (Y/N) | Date |
|------|----------|------------|------|
| Business owner | AT-04, AT-14, AT-18 complete | | |
| Coordinator rep | AT-12–AT-16 complete | | |
| Academic manager | AT-17–AT-19, AT-26 complete | | |
| QA | UT-01–UT-04 green | | |
