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
See the [work protocol](https://dinorefurb.com/work-protocol/#working-files).

## State

- Stage: Slices, with slices 2 and 3 in progress. The Survey exit is checked.
- Branch: `main`, fifty-one commits ahead of `origin/main`; not pushed.
- Last gate: 2026-09-27, `./tools/Test.ps1` passed (700 tests;
  documentation check passed with 422 entries and 158 parity rows).

## Unfinished

None.

## Blockers

None known.

## Next

1. `Q-CONFIG-002` and `Q-CONFIG-007`, for `FMT-CONFIG-003` and `RULE-CONFIG-005`.
2. `Q-UI-005` and `Q-SAVE-001`, for `SCR-UI-013` and `SCR-UI-014`;
   `Q-UI-002`, for `SCR-UI-007`.
3. `Q-CONFIG-001`, the requested Preferences live session.
4. `Q-PARTY-001`, `Q-PARTY-005` and `Q-PARTY-009`.
5. Rows with evidence already in the spec: `RULE-EXPLORE-002`,
   `RULE-EXPLORE-003`, `RULE-EXPLORE-004` and `RULE-INPUT-002`.
