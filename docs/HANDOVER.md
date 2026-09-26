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

- Stage: Survey, with slices 2 and 3 in progress.
- Branch: `adopt-work-protocol`, from `main` at `ecbc05d`, pushed and open as a
  pull request.
- Last gate: 2026-09-26, `./tools/Test.ps1` passed (700 tests, documentation
  check passed).

## Unfinished

None.

## Blockers

None known.

## Next

1. Survey exit: a tooling batch that exports the function inventory of
   `DSUN.EXE` to `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`, and a check that
   every screen `SRC-MANUAL-1994` mentions has a screen entry.
2. `Q-CONFIG-001` and `Q-CONFIG-002`, for `SCR-UI-007`.
3. `Q-PARTY-001`, `Q-PARTY-005` and `Q-PARTY-009`.
4. `Q-EXPLORE-001` to `Q-EXPLORE-006` and `Q-ACTOR-001`.
5. Rows with evidence already in the spec: `RULE-EXPLORE-002`,
   `RULE-EXPLORE-003`, `RULE-EXPLORE-004` and `RULE-INPUT-002`.
