# CMCS — Sequence Flows

Three flows cover the happy path and rejection. All POST actions use antiforgery tokens; status changes go through `ClaimsController.UpdateStatus` unless noted.

## 1 — Lecturer submits a claim

```mermaid
sequenceDiagram
    actor L as Lecturer
    participant C as ClaimsController
    participant DB as Database

    L->>C: POST Create (hours, notes, files)
    C->>C: Validate + compute total
    C->>DB: Save Claim (Submitted)
    C->>DB: Save documents + history
    C-->>L: Redirect with success
```

## 2 — Coordinator approves

```mermaid
sequenceDiagram
    actor CO as Coordinator
    participant C as ClaimsController
    participant DB as Database

    CO->>C: GET CoordinatorDashboard
    C->>DB: Load Submitted claims
    CO->>C: POST UpdateStatus → ApprovedByCoordinator
    C->>DB: Update claim + history
    C-->>CO: JSON success (AJAX)
```

Optional: **AutoApproveClaim** runs automation rules before advancing status.

## 3 — Manager final approval

```mermaid
sequenceDiagram
    actor M as Manager
    participant C as ClaimsController
    participant DB as Database

    M->>C: GET ManagerDashboard
    C->>DB: Load ApprovedByCoordinator claims
    M->>C: POST UpdateStatus → ApprovedByManager
    C->>DB: Update claim + history
    C-->>M: JSON success
```

Batch **BatchApproveHighConfidence** may approve multiple high-scoring claims in one request.

## Rejection (coordinator or manager)

Same endpoint with status **Rejected (4)** and notes stored on history for the lecturer to read on **Details**.

## Who can do what

| Action | Role |
|--------|------|
| Create claim | Lecturer |
| Coordinator dashboard | Coordinator |
| Manager dashboard | Manager |
| UpdateStatus | Coordinator or Manager |

## Resilience

If automation fails when loading the coordinator dashboard, the controller falls back to a plain pending list (see NFR-09 in requirements).

Test mapping: [08-traceability-matrix.md](./08-traceability-matrix.md).
