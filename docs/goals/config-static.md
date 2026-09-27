# config-static

## Condition

Close the Static items in `queue/CONFIG.md` that block slice 3, or move each
to the evidence section that can settle it with its attempted readings recorded;
the documentation check passes on the last commit and each batch ends with a
status block; or stop after 20 turns. This bounded goal file tracks the current
research campaign under the continuing thread goal.

## Scope

Areas: CONFIG. Batches: research only. Queue sections: Static.

## Must not touch

Other areas' entries, queue files and parity rows. `src/` and `tests/`.

## Dead ends

- Mapped Ghidra reference queries to the overlay 171 difficulty writer and
  its resident trampoline found no recognized callers; follow indirect
  dispatch or callback data for Q-CONFIG-002.
- Q-CONFIG-007's window setup calls enter resident heap routines; inspect
  their result contract before inferring live message-delay outcomes.

## Handover

- Stage: Slices, with slices 2 and 3 in progress.
- Branch: `main`, nineteen commits ahead of `origin/main` including this
  handover; not pushed.
- Last gate: 2026-09-27, `./tools/Test.ps1` passed (700 tests);
  documentation check passed (408 entries, 158 parity rows).
- Unfinished: none.
- Blockers: none known.
- Next: Q-CONFIG-005 through the remaining SOUND.CFG consumers and setup
  writer; then Q-CONFIG-007 through new-game initialization; then
  Q-CONFIG-002 through PREF data-site users or indirect dispatch.
