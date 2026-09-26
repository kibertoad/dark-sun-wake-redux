# survey-exit

## Condition

The Survey exit in `docs/IMPLEMENTATION-PLAN.md` holds: the approved
`DSUN.EXE` has a compliant function inventory under `coverage/`, every screen
mentioned by `SRC-MANUAL-1994` has a screen entry, the documentation check
passes on the last commit, and each batch ended with a status block; or stop
after 40 turns.

## Scope

Areas: UI and any other area that needs a screen entry from
`SRC-MANUAL-1994`. Batches: research and tooling only. The inventory covers
`BLD-GOG-EN-1.1`.

## Must not touch

`src/` gameplay code or parity implementation status. Original content in Git.

## Dead ends

None known.

## Handover

- Stage: Survey, with slices 2 and 3 in progress.
- Branch: `main`, clean at `00ec491` before this goal file.
- Last gate: 2026-09-26, documentation check passed (375 entries, 140 parity rows, 5 deviations); `./tools/Test.ps1` last passed per `docs/HANDOVER.md` (700 tests).
- Unfinished: Survey exit: function inventory and manual screen coverage.
- Blockers: none known.
- Next: export `DSUN.EXE` function inventory; check `SRC-MANUAL-1994` screen coverage.
