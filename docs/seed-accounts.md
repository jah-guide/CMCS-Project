# CMCS — Seed & Demo Accounts

> **WARNING — LOCAL DEMO ONLY**
>
> The credentials below are created automatically by `SeedData` for **local development and portfolio demonstration**. They are **not** secrets suitable for production. **Do not** deploy these passwords to any shared or production environment. Change or disable seed users before go-live.

## When these accounts are created

On application startup, `Program.cs` invokes `SeedData.Initialize`, which:

1. Applies Entity Framework migrations.
2. Creates Identity roles: `Lecturer`, `Coordinator`, `Manager`, `HR`.
3. Seeds reference statuses if the table is empty.
4. Creates demo users if they do not already exist.

## Pre-seeded accounts

| Role | Email (username) | Password | Notes |
|------|------------------|----------|-------|
| Programme coordinator | `coordinator@cmcs.com` | `Coordinator123!` | First approval queue |
| Academic manager | `manager@cmcs.com` | `Manager123!` | Final approval queue |
| HR (extension/demo) | `hr@cmcs.com` | `HR123!` | Role seeded; HR UI out of scope for v1 |

## Lecturer accounts

Lecturers are **not** pre-seeded with shared passwords. Register a new user via the Identity **Register** page. During registration, set an hourly rate so claim totals calculate correctly. Assign the **Lecturer** role if your local registration flow does not do so automatically (production would use an admin provisioning step).

## Security reminders

- Rotate or remove demo users before publishing to the internet.
- Prefer user secrets or environment variables for any future admin accounts — never commit production passwords.
- The public [README](../README.md) intentionally omits plaintext passwords; use this file only on trusted machines.

## Quick local test path

1. Run the application (see README).
2. Sign in as coordinator → approve a submitted claim.
3. Sign in as manager → finalise the claim.
4. Register a separate lecturer account to test submission end-to-end.
