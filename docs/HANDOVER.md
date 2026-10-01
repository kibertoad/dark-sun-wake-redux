# Handover

Where the work outside any goal stands now. Rewrite this file at the end of
every session that works under no goal; do not append to it. A session under a
goal writes the same sections in its goal file's Handover instead, so two
sessions running at once never write the same file. It is at most 200 lines.
History is in git, open research questions are in `queue/`, the plan is in
`docs/IMPLEMENTATION-PLAN.md`, the goals running are the files in
`docs/goals/`, and the open live session requests are the files in
`docs/live-sessions/`. Name items and entries by ID; what research found or
tried belongs in the spec and the queue, never here.
See the [work protocol](../vendor/upstream/work-protocol.md#working-files) (lines 10-30).

## State

- Stage: Slices, with slices 2 and 3 in progress. The refreshed Survey
  file-denominator exit needs the Q-EXE-003 re-audit.
- Last gate: 2026-10-01, `./tools/Test.ps1` passed with repository-local
  PowerShell 7; output is in `artifacts/migration-final-acceptance.log`.
  Full solution build and assetless Release publish/smoke pass.
- The adopted website, toolkit and template revisions have exact source pins;
  see `docs/TEMPLATE-ADOPTION.md` and `docs/REPORTER-CASE-AUDIT.md`.
  CI includes all adopted regression suites, research tracking and local policy.
  Hosted verification and every installer pass in
  [CI run 36799584294](https://github.com/kibertoad/dark-sun-wake-redux/actions/runs/36799584294).
- Probe is none. Native DOSBox access remains owner-only; no draw probe or
  RNG hook has been implemented by this update.
- Offline checker and section-link verification pass. Snapshot/configuration
  tests require the repository-local PowerShell 7 runtime on PATH.

## Unfinished

No unfinished migration changes. Research follow-up remains in `queue/` and
`gaps.md`.

## Blockers

No migration blocker. High/mixed-DPI original-window capture remains
unverified and requires owner validation; native runtime stays owner-only.

## Next

1. `Q-EXE-003`, the revised Survey listing/exclusions exit; then
   `Q-CONFIG-008`, `Q-CONFIG-007` and `Q-CONFIG-002`, for `RULE-CONFIG-005`
   and `FMT-CONFIG-003`.
2. `Q-UI-005` and `Q-SAVE-001`, for `SCR-UI-013` and `SCR-UI-014`;
   `Q-UI-002`, for `SCR-UI-007`.
3. `Q-CONFIG-001`, the requested Preferences live session.
4. `Q-PARTY-001`, `Q-PARTY-005` and `Q-PARTY-009`.
5. Rows with evidence already in the spec: `RULE-EXPLORE-002`,
   `RULE-EXPLORE-003`, `RULE-EXPLORE-004` and `RULE-INPUT-002`.
