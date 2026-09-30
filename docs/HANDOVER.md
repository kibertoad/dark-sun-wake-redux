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
- Last gate: 2026-09-30, `./tools/Test.ps1` passed with repository-local
  PowerShell 7 (92 Python tests, 41 Node tests, 700 .NET tests; documentation check: 615
  entries, 158 parity rows, 5 deviations). The full solution builds with no
  warnings or errors.
- Latest website, toolkit and template main revisions are fully adopted with exact source pins; see `docs/TEMPLATE-ADOPTION.md`. The case acceptance audit is in `docs/REPORTER-CASE-AUDIT.md`; `gaps.md` has 37 remaining requests. PE32 and segmented synthetic suites run in the canonical gate.
- Probe is none. Native DOSBox access remains owner-only; no draw probe or
  RNG hook has been implemented by this update.
- Offline checker and section-link verification pass. Snapshot/configuration
  tests require the repository-local PowerShell 7 runtime on PATH.

## Unfinished

None.

## Blockers

None known.

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
