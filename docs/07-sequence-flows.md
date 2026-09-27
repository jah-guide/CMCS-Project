# CMCS — Sequence Flows

## Flow 1 — Lecturer submits claim

End-to-end path from form submission through persistence and initial history.

```mermaid
sequenceDiagram
    actor L as Lecturer
    participant UI as Claims/Create View
    participant C as ClaimsController
    participant UM as UserManager
    participant DB as ApplicationDbContext
    participant FS as FileUploadService

    L->>UI: Enter hours, notes, files
    UI->>C: POST Create (antiforgery token)
    C->>UM: GetUserAsync
    UM-->>C: ApplicationUser + HourlyRate
    C->>C: Validate hours, compute TotalAmount
    C->>DB: Add Claim (Status Submitted)
    DB-->>C: ClaimID
    loop Each valid file
        C->>FS: IsValidFile / UploadFileAsync
        FS-->>C: FilePath
        C->>DB: Add SupportingDocument
    end
    C->>DB: Add ClaimStatusHistory (Submitted)
    C->>DB: SaveChanges
    C-->>L: Redirect + success message
```

---

## Flow 2 — Coordinator reviews and approves

Coordinator dashboard loads prioritised **Submitted** claims; approval updates status via AJAX.

```mermaid
sequenceDiagram
    actor CO as Coordinator
    participant UI as CoordinatorDashboard
    participant C as ClaimsController
    participant AS as ClaimAutomationService
    participant DB as ApplicationDbContext

    CO->>UI: Open dashboard
    UI->>C: GET CoordinatorDashboard
    C->>AS: GetPrioritizedClaimsAsync
    AS->>DB: Query StatusID = 1 + includes
    AS-->>C: List ClaimWithScore
    C-->>UI: Render prioritised table

    CO->>UI: Approve claim (AJAX)
    UI->>C: POST UpdateStatus(claimId, 2, notes)
    C->>DB: Update Claim.CurrentStatusID
    C->>DB: Insert ClaimStatusHistory
    C->>DB: SaveChanges
    C-->>UI: JSON success
    UI-->>CO: Refresh row / message
```

**Optional path:** Coordinator invokes **AutoApproveClaim**; controller calls `ProcessAutomatedApprovalAsync` and only advances status when automation rules pass.

---

## Flow 3 — Manager final approval

Manager works on claims already in **ApprovedByCoordinator** status.

```mermaid
sequenceDiagram
    actor M as Academic Manager
    participant UI as ManagerDashboard
    participant C as ClaimsController
    participant AS as ClaimAutomationService
    participant DB as ApplicationDbContext

    M->>UI: Open manager dashboard
    UI->>C: GET ManagerDashboard
    C->>DB: Query CurrentStatusID = 2
    C-->>UI: Approved-by-coordinator list

    M->>UI: View analysis (optional)
    UI->>C: GET GetManagerAnalysis(claimId)
    C->>AS: CalculateClaimScoreAsync
    AS-->>C: ClaimScore + HTML fragment
    C-->>UI: Analysis panel

    M->>UI: Final approve (AJAX)
    UI->>C: POST UpdateStatus(claimId, 3, notes)
    C->>DB: Update status + history
    C-->>UI: JSON success

    opt Batch high confidence
        M->>UI: BatchApproveHighConfidence
        UI->>C: POST BatchApproveHighConfidence
        C->>AS: Score each claim
        C->>DB: Bulk update to ApprovedByManager
    end
```

---

## Flow 4 — Rejection (coordinator or manager)

Same `UpdateStatus` endpoint with `newStatusId = 4` (Rejected). Notes should capture reason for lecturer visibility on **Details**.

```mermaid
sequenceDiagram
    actor R as Reviewer
    participant UI as Dashboard
    participant C as ClaimsController
    participant DB as ApplicationDbContext

    R->>UI: Reject with reason
    UI->>C: POST UpdateStatus(claimId, 4, notes)
    C->>DB: CurrentStatusID = Rejected
    C->>DB: Add history row
    C-->>UI: success
```

---

## Authorization checkpoints

| Step | Required role |
|------|----------------|
| Create claim | Lecturer |
| CoordinatorDashboard | Coordinator |
| ManagerDashboard | Manager |
| UpdateStatus | Coordinator or Manager |
| BatchApproveHighConfidence | Manager |

---

## Error handling notes

- If automation fails on coordinator dashboard load, controller falls back to a plain pending list (NFR-09).
- Invalid files are skipped during upload loop; lecturer may receive success with partial attachments unless all files invalid.

See [09-acceptance-tests.md](./09-acceptance-tests.md) for executable test mapping.
