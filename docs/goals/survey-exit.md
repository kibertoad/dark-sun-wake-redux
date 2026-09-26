# survey-exit

## Condition

The Survey exit in `docs/BOOTSTRAP-CHECKLIST.md` holds: every manifest `data`
file except CD audio tracks has a format entry, each analyzed executable has
a compliant function inventory under `coverage/`, every screen mentioned by
`SRC-MANUAL-1994` has a screen entry, and each spec area has a queue file. The
documentation check passes on the last commit and each batch ended with a
status block; or stop after 40 turns.

## Scope

Areas: UI, EXPLORE, CONFIG, EXE and SOUND. Batches: planning, research and
tooling only. The inventory covers `BLD-GOG-EN-1.1`.

## Must not touch

`src/` gameplay code or parity implementation status. Original content in Git.

## Dead ends

None known.

## Handover

- Stage: Survey, with slices 2 and 3 in progress.
- Branch: `main`, three commits ahead of `origin/main`; push awaits owner approval.
- Last gate: 2026-09-27, `./tools/Test.ps1` passed (700 tests; documentation check passed).
- Unfinished: Survey screen coverage for `SRC-MANUAL-1994`. The `BLD-GOG-EN-1.1/DSUN.EXE` function inventory is in `coverage/`.
- Blockers: none known.
- Next: add the missing screen entries cited by `SRC-MANUAL-1994`, their parity rows and queue items; then check the Survey exit.
