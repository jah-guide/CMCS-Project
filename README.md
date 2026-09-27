# Contract Monthly Claim System (CMCS)

**Systems Analyst case study · ASP.NET Core reference implementation**

CMCS replaces a fragile **paper-and-email** monthly claim process for **independent contractor lecturers** with a structured web workflow: submit hours and evidence, coordinator review, academic manager sign-off, and a durable audit trail.

> **Start with the analysis pack:** full business analysis artefacts live in **[`docs/`](./docs/)** — context, requirements, use cases, process models, data design, sequence flows, traceability, and acceptance tests.

---

## The problem

Lecturers historically emailed spreadsheets and scanned attachments to coordinators, who forwarded threads to academic managers. Payment teams reconstructed totals manually. The result was slow cycle times, lost files, and weak proof of approval.

## Stakeholders & outcomes

| Stakeholder | Outcome enabled by CMCS |
|-------------|-------------------------|
| Lecturer | One place to submit hours, upload documents, and track status |
| Programme coordinator | Prioritised queue of **Submitted** claims with approve/reject and notes |
| Academic manager | Queue of coordinator-approved claims and summary reporting |
| Compliance / audit | `ClaimStatusHistory` records who changed status and when |

Role mapping in the app: **Lecturer**, **Coordinator**, **Manager** (Academic Manager).

---

## Documentation index

| Document | Contents |
|----------|----------|
| [docs/README.md](./docs/README.md) | **Start here** — analysis pack index |
| [docs/01-context.md](./docs/01-context.md) | Business context, scope, success measures |
| [docs/02-stakeholders-raci.md](./docs/02-stakeholders-raci.md) | Stakeholders, personas, RACI |
| [docs/03-requirements.md](./docs/03-requirements.md) | FR-01…FR-26, NFRs, business rules |
| [docs/04-use-cases-stories.md](./docs/04-use-cases-stories.md) | Use cases and acceptance criteria |
| [docs/05-process-as-is-to-be.md](./docs/05-process-as-is-to-be.md) | As-is vs to-be (Mermaid) |
| [docs/06-data-model.md](./docs/06-data-model.md) | ERD: Claim, Document, User, Status |
| [docs/07-sequence-flows.md](./docs/07-sequence-flows.md) | Submit → coordinator → manager |
| [docs/08-traceability-matrix.md](./docs/08-traceability-matrix.md) | Requirements → tests |
| [docs/10-ui-ux-features.md](./docs/10-ui-ux-features.md) | Shared UI partials, filters, empty states |
| [docs/09-acceptance-tests.md](./docs/09-acceptance-tests.md) | Manual & automated acceptance |
| [docs/seed-accounts.md](./docs/seed-accounts.md) | **Local demo only** — seed credentials |

---

## Technology stack

| Layer | Choice |
|-------|--------|
| Application | ASP.NET Core **8** MVC |
| Data | Entity Framework Core, SQL Server (LocalDB in dev) |
| Security | ASP.NET Core Identity, role-based authorization |
| UI | Bootstrap 5, jQuery, AJAX approval actions |
| Testing | xUnit, EF Core InMemory provider |
| Services | File upload validation, claim automation/scoring, reporting |

---

## Solution layout

```
CMCS-Project/
├── Controllers/          # Claims workflow, Home
├── Models/               # Claim, Status, documents, view models
├── Views/                # Lecturer & approval dashboards
├── Data/                 # ApplicationDbContext, SeedData
├── Services/             # Upload, automation, reports
├── Areas/                # Identity UI
├── ClaimServiceTests.cs  # Unit tests (xUnit)
└── docs/                 # Systems Analyst deliverables
```

---

## Run locally

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), SQL Server LocalDB (typical Visual Studio install).

1. **Clone** the repository and open a terminal in the project root.

2. **Configure database** (default LocalDB connection in `appsettings.json`):

   ```json
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CMCS_Database;Trusted_Connection=true;MultipleActiveResultSets=true"
   ```

3. **Restore and run:**

   ```bash
   dotnet restore ContractMonthlyClaimSystem.sln
   dotnet run --project ContractMonthlyClaimSystem.csproj
   ```

   On first run, migrations and **seed data** apply automatically (`SeedData.Initialize`).

4. **Optional — EF CLI** (if you add migrations in development):

   ```bash
   dotnet ef database update
   ```

5. Open the HTTPS URL shown in the console (trust the dev certificate if prompted).

6. **Test accounts:** use [docs/seed-accounts.md](./docs/seed-accounts.md) for coordinator/manager demo logins. Register a **Lecturer** via the app to test submission.

7. **Unit tests:**

   ```bash
   dotnet test ContractMonthlyClaimSystem.sln
   ```

---

## Features (implementation snapshot)

- Lecturer claim submission with automatic amount calculation  
- Supporting document upload (type/size validated)  
- Coordinator dashboard with prioritisation and batch/auto-approval aids  
- Academic manager dashboard, analysis views, and report generation  
- Status history on every transition  
- Unit tests for core domain calculations and persistence  

---

## Portfolio note

This repository is curated as a **Systems Analyst showcase** for [jah-guide](https://github.com/jah-guide): analysis-first documentation under `docs/`, with this codebase as the traceable solution increment (PROG6212 Programming 2B — Contract Monthly Claim System).

**Recent refresh:** simplified Mermaid and use-case docs, enterprise Bootstrap layout (`wwwroot/css/site.css`), and role-aware navigation. After clone, run `dotnet tool restore` (if configured) and `libman restore` to pull client libraries into `wwwroot/lib/`.

**License:** see [LICENSE](./LICENSE).
