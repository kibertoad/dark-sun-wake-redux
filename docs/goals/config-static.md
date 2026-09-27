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
- Q-CONFIG-002's resident `PREF` byte match is a menu-label substring
  (FND-CONFIG-025); follow indirect resource dispatch or settings writers.
- Three apparent speech-gate offset tables lie in FBOV fixup payloads
  (FND-CONFIG-027); follow indirect initialization or a block copy.
- The Start Game button branch and local setup helper have no direct
  settings writes (FND-CONFIG-028); its raw far-call segments are FBOV
  descriptor encodings, now resolved to resident and overlay targets.
- Literal music-level request writers are limited to Load Game and the
  Preferences-entry getter (FND-CONFIG-029); follow indirect writes.

## Handover

- Stage: Slices, with slices 2 and 3 in progress.
- Branch: `main`, thirty-nine commits ahead of `origin/main` including this
  handover; not pushed.
- Last gate: 2026-09-27, `./tools/Test.ps1` passed (700 tests);
  documentation check passed (417 entries, 158 parity rows).
- Unfinished: none.
- Blockers: none known.
- Next: Q-CONFIG-002 through resolved Start Game setup callees and indirect writes;
  then Q-CONFIG-007 through the same initialization path; then Q-CONFIG-005
  through the setup input record and remaining sound-library branches.
