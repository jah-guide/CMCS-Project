# CMCS — Requirements Traceability Matrix

Links functional requirements (FR) and key non-functional requirements (NFR) to use cases (UC), user stories (US), and acceptance tests (AT).

| Req ID | Use case(s) | User story | Acceptance test(s) | Implementation hint |
|--------|-------------|------------|----------------------|---------------------|
| FR-01 | UC-01 | — | AT-01, AT-02 | Identity areas, registration |
| FR-02 | UC-01 | — | AT-03 | `ApplicationUser` profile |
| FR-03 | UC-02 | US-02 | AT-04, AT-05, AT-06 | `ClaimsController.Create` |
| FR-04 | UC-02 | US-02 | AT-07, UT-01 | HourlyRate × hours |
| FR-05 | UC-02 | US-02 | AT-04 | `CurrentStatusID = 1` |
| FR-06 | UC-03 | US-02 | AT-08 | Multipart upload on Create |
| FR-07 | UC-03 | US-02 | AT-09, AT-10 | `FileUploadService.IsValidFile` |
| FR-08 | UC-03 | — | AT-08 | `UploadFileAsync` |
| FR-09 | UC-02 | US-AUD-01 | AT-11 | Status history on submit |
| FR-10 | UC-04 | US-05 | AT-12 | `CoordinatorDashboard` |
| FR-11 | UC-04 | US-11 | AT-13 | `ClaimAutomationService` |
| FR-12 | UC-05 | US-05 | AT-14 | `UpdateStatus` → status 2 |
| FR-13 | UC-06 | US-06 | AT-15 | `UpdateStatus` → status 4 |
| FR-14 | UC-11 | US-11 | AT-16 | `ProcessBatchApproval` |
| FR-15 | UC-07 | US-08 | AT-17 | `ManagerDashboard` |
| FR-16 | UC-08 | US-08 | AT-18 | `UpdateStatus` → status 3 |
| FR-17 | UC-09 | US-08 | AT-19 | Manager reject |
| FR-18 | UC-05, UC-08, UC-06, UC-09 | US-AUD-01 | AT-11, AT-14–AT-19 | `ClaimStatusHistory` |
| FR-19 | UC-10 | US-10 | AT-20 | `Claims/Details` |
| FR-20 | UC-02 | US-SEC-01 | AT-21 | `[Authorize(Roles = "Lecturer")]` |
| FR-21 | UC-04–UC-06 | US-SEC-01 | AT-22 | Coordinator role attributes |
| FR-22 | UC-07–UC-09 | US-SEC-01 | AT-23 | Manager role attributes |
| FR-23 | — | — | AT-24, UT-04 | Seed statuses in DbContext/SeedData |
| FR-24 | — | — | AT-25 | `SeedData.Initialize` |
| FR-25 | UC-05, UC-08 | — | AT-14, AT-18 | JSON `UpdateStatus` |
| FR-26 | UC-12 | US-12 | AT-26 | `GenerateManagerReport` |
| NFR-01 | All | US-SEC-01 | AT-27 | `[Authorize]` on ClaimsController |
| NFR-02 | All | US-SEC-01 | AT-21–AT-23 | Role-based actions |
| NFR-03 | UC-02 | US-SEC-02 | AT-28 | Antiforgery on POST Create |
| NFR-05 | — | — | UT-03 | EF relationships |
| NFR-06 | — | — | UT-01 | Decimal precision |
| NFR-11 | — | — | UT-01–UT-04 | xUnit project |
| NFR-12 | — | — | AT-25 | Startup seed + migrate |

## Coverage summary

| Artefact | Count referenced |
|----------|------------------|
| Functional requirements | FR-01 – FR-26 |
| Non-functional (sampled) | NFR-01, NFR-02, NFR-03, NFR-05, NFR-06, NFR-11, NFR-12 |
| Use cases | UC-01 – UC-12 |
| Automated unit tests | UT-01 – UT-04 (see test project) |

## Gaps / manual-only tests

| Gap | Reason |
|-----|--------|
| Full browser AJAX flows | Covered by manual AT-14–AT-19; optional UI automation backlog |
| Paid status transition | FR status exists; finance integration not built (OI-01) |
| Draft persistence | Demo API only (OI-02) |
