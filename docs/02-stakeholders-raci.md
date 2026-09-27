# CMCS — Stakeholders & RACI

## Stakeholder register

| ID | Stakeholder | Role in organisation | Interest | Influence |
|----|-------------|----------------------|----------|-----------|
| ST-01 | Independent contractor lecturer | Submits monthly claims | High — payment and transparency | Medium |
| ST-02 | Programme coordinator | First-line academic review | High — workload and compliance | High |
| ST-03 | Academic manager | Final academic sign-off | High — budget and policy | High |
| ST-04 | Finance / payroll | Consumes approved claims (future) | Medium — accurate totals | Medium |
| ST-05 | IT operations | Hosts and secures the application | Medium — availability, backups | Medium |
| ST-06 | Institutional compliance | Audit and POPIA alignment | Medium — evidence trail | High |

## Personas (summary)

### Lecturer (Primary user)

- **Goal:** Submit accurate claims quickly and see where they stand.
- **Frustrations:** Chasing email, re-sending attachments, unclear rejection reasons.
- **System touchpoints:** Register/login, create claim, upload documents, view details/history.

### Programme coordinator

- **Goal:** Review submitted claims fairly and efficiently.
- **Frustrations:** Unprioritised inbox, missing documents, inconsistent amounts.
- **System touchpoints:** Coordinator dashboard, approve/reject, batch tools, claim analysis.

### Academic manager

- **Goal:** Approve coordinator-vetted claims with confidence before payment run.
- **Frustrations:** Lack of context from first review, manual reporting.
- **System touchpoints:** Manager dashboard, final approve/reject, reports and batch high-confidence approval.

## RACI matrix

**R** = Responsible · **A** = Accountable · **C** = Consulted · **I** = Informed

| Activity | Lecturer | Coordinator | Academic Manager | Finance | IT |
|----------|:--------:|:-----------:|:----------------:|:-------:|:--:|
| Register account & maintain hourly rate | R/A | I | I | I | C |
| Submit monthly claim + documents | R/A | I | I | I | I |
| Validate completeness (hours, docs) | C | R | I | I | I |
| First approval / rejection | I | R/A | C | I | I |
| Final approval / rejection | I | C | R/A | I | I |
| Mark claim paid (future integration) | I | I | C | R/A | I |
| User access & role assignment | I | C | C | I | R/A |
| System availability & backups | I | I | I | I | R/A |

## Communication plan (analysis view)

| Event | Audience | Channel in CMCS | Future enhancement |
|-------|----------|-----------------|-------------------|
| Claim submitted | Coordinator | Dashboard queue | Email notification |
| Coordinator decision | Lecturer, Manager | Status + history | Email to lecturer |
| Manager decision | Lecturer, Finance | Status + history | Export to payroll |
| Rejection | Lecturer | History notes | Templated email with reason |

## Decision rights

| Decision | Owner | Notes |
|----------|-------|-------|
| Approve/reject at coordinator stage | Programme coordinator | Cannot skip lecturer submission |
| Approve/reject at manager stage | Academic manager | Only claims in `ApprovedByCoordinator` |
| Policy on max hours per claim | Academic manager + compliance | Implemented as FR validation (1–200) |
| Allowed document types | IT + compliance | Enforced in upload service |

## Mapping to implementation

| Stakeholder | ASP.NET Identity role | Primary controllers/views |
|-------------|----------------------|---------------------------|
| Lecturer | `Lecturer` | `Claims/Create`, `Claims/Details`, Home |
| Programme coordinator | `Coordinator` | `Claims/CoordinatorDashboard` |
| Academic manager | `Manager` | `Claims/ManagerDashboard` |

See [seed-accounts.md](./seed-accounts.md) for local demo identities only.
