# latest-upstream-migration

## Condition

Template, toolkit, standard and protocol match the latest delivered upstream capabilities, current upstream versions are independently checked, capability dispositions are recorded, and canonical validation passes. Commit the completed migration and its handover; do not push.

## Scope

Areas: repository infrastructure freshness verification and adoption documentation. Tooling batches only. This goal does not take research queue items or claim the reporter acceptance work owned by upstream-gap-resolution.

## Must not touch

Game behavior, spec claims, parity statuses, proprietary content, original runtime, other goals, or the pre-existing scoped-memory candidate plan.

## Dead ends

Sandbox network access cannot establish freshness. Use authorized upstream queries. The installed PowerShell 7 host is artifacts/pwsh7/runtime/pwsh.exe.

## Handover

- Stage: Slices; migration is tooling only.
- Last gate: 2026-10-02, tools/Invoke-Validation.ps1 passed, including tools/Test.ps1, Release build and assetless publish/smoke.
- Unfinished: latest checker action pin and current acceptance records.
- Blockers: none.
- Next: reconcile latest action pin, record capability dispositions, rerun affected checks and commit.
