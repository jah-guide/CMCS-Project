# CMCS — Process: As-Is vs To-Be

## As-is process (paper and email)

Lecturers complete a paper or spreadsheet timesheet, scan supporting evidence, and email the package to a programme coordinator. The coordinator forwards approved packs to an academic manager via email. Finance receives ad-hoc summaries. Status is inferred from reply chains; attachments are duplicated across inboxes.

```mermaid
flowchart TD
    subgraph Lecturer
        A[Complete paper/spreadsheet timesheet]
        B[Scan / collect PDFs]
        C[Email to coordinator]
    end
    subgraph Coordinator
        D[Search inbox for claim]
        E{Complete?}
        F[Email back for fixes]
        G[Forward email to manager]
    end
    subgraph AcademicManager
        H[Review email thread]
        I{Approve?}
        J[Reply approve / reject]
    end
    subgraph Finance
        K[Manual spreadsheet totals]
        L[Payment run]
    end
    A --> B --> C --> D --> E
    E -->|No| F --> C
    E -->|Yes| G --> H --> I
    I -->|No| F
    I -->|Yes| J --> K --> L
```

### As-is pain points

- No single queue; coordinators triage personal inboxes.
- Version control on attachments is poor.
- Approval authority is implicit in email text, not structured data.
- Reporting requires manual copy/paste.

---

## To-be process (CMCS)

All actors use the web application. Identity roles control access. Status transitions are explicit and logged. Documents live with the claim record.

```mermaid
flowchart TD
    subgraph CMCS_Lecturer[Lecturer — CMCS]
        L1[Register / Sign in]
        L2[Create claim: hours + notes]
        L3[Upload supporting documents]
        L4[Submit → Status: Submitted]
    end
    subgraph CMCS_Coord[Coordinator — CMCS]
        C1[Open coordinator dashboard]
        C2[Prioritised queue Submitted]
        C3{Decision}
        C4[Approve → ApprovedByCoordinator]
        C5[Reject → Rejected + notes]
    end
    subgraph CMCS_Mgr[Academic Manager — CMCS]
        M1[Open manager dashboard]
        M2[Queue ApprovedByCoordinator]
        M3{Decision}
        M4[Approve → ApprovedByManager]
        M5[Reject → Rejected + notes]
        M6[Generate report optional]
    end
    subgraph CMCS_Future[Finance — future]
        F1[Import approved claims]
        F2[Mark Paid]
    end
    L1 --> L2 --> L3 --> L4 --> C1 --> C2 --> C3
    C3 -->|Approve| C4 --> M1 --> M2 --> M3
    C3 -->|Reject| C5
    M3 -->|Approve| M4 --> M6 --> F1 --> F2
    M3 -->|Reject| M5
```

---

## Process comparison

| Dimension | As-is | To-be (CMCS) |
|-----------|-------|--------------|
| Submission channel | Email | Web form |
| Evidence storage | Mail attachments | Linked `SupportingDocument` rows + file store |
| Work queues | Inbox search | Role dashboards filtered by status |
| Approval evidence | Email headers | `ClaimStatusHistory` with user id and timestamp |
| Rejection feedback | Reply email | History notes visible on claim detail |
| Reporting | Manual | Manager report endpoint (text export) |
| Access control | Informal | ASP.NET Identity roles |

## Status alignment

| Step | Business status | StatusID |
|------|-----------------|----------|
| Lecturer submits | Submitted | 1 |
| Coordinator approves | ApprovedByCoordinator | 2 |
| Manager approves | ApprovedByManager | 3 |
| Either rejects | Rejected | 4 |
| Finance completes (future) | Paid | 5 |

## Change impact

| Group | Change | Mitigation |
|-------|--------|------------|
| Lecturers | New login and form | Short guide; hourly rate on registration |
| Coordinators | Dashboard instead of inbox | Training on prioritisation scores |
| Managers | Second queue in system | Report export for finance handoff |
| IT | Host SQL + web app | Standard ASP.NET Core deployment |
