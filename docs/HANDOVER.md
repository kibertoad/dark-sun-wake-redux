# Handover

## Current state

- Stage: Slices. Full research Audit remains unmet. The owner cleared the
  full-game goal on 2026-11-09 and requested batch completion, documentation,
  push to main and stop. The goal is dropped, not achieved; this does not
  accept any unresolved evidence as out of reach.
- The final tooling batch prepares `setup_parser.py --chooser-bound-rows`.
  Synthetic controls pass, but no original attempt was made before wrap-up.
  Its native wrapper/source/row/vector capability remains unverified.
  Use docs/implementation-plans/ADV-SURVEY.md and docs/RUNTIME.md.
- Last acceptance: 2026-11-09, `./tools/Test.ps1 -Integration -NoRestore`
  passed the full Node checks, build and .NET tests, with the existing
  documented argument-count, Kaitai and scheduled-generation skips.
  The final tooling gate also passed with `-ChangedSince 17db43d8 -NoRestore`.
  docs/PROGRESS.md is the refreshed script-generated snapshot.
- Set EVIDENCE_PYTHON to the checkout's
  artifacts/evidence-python/Scripts/python.exe; the inherited override belongs
  to another project. Keep hooks enabled and use supported child-process access.
- Runtime pins remain session 1.0.0 and rust-dos v1.8.1. Follow the package's
  owned-process/run-lock contract and default muted host output. Never operate
  an unowned emulator. All owned probes are closed; no owned run lock remains.
  Reusable MSBuild workers are intentionally preserved.

## Unfinished work and limitations

- No unfinished batch. The prepared wrapped-row profile needs a fresh bounded
  research capability attempt; it is not validated by the older generic-entry
  observations. No gameplay implementation was added.
- Toolkit issue511 remains the shared transport reference. The follow-up
  diagnostic was not posted: automatic approval review rejected external
  disclosure without specific approval. The local draft and approval request
  remain pending; this does not block independent preservation work.
- Controlled memory profiles remain unavailable under the pinned API;
  toolkit issue552 tracks typed support. Never replace the package-owned config.
- Keep implementation separate from research. Original-derived reports and
  payloads remain in GAME_DIR, never Git. Existing queue items and parity
  limitations remain authoritative.

## Next research work

1. Q-SOUND-010: actual chooser wrapper/source/count/text, wrapped-frame and
   row/vector admission, second-event completion and later lifetime. Reuse
   FND-SOUND-097..109, FND-EXE-483..489 and FND-CONFIG-257..269; do not repeat
   answered notice construction or first-event delivery. Preserve BIOS SI/DI/ES,
   port-progress, scratch/backing and live-allocation obligations. Other SOUND
   items reuse SOUND-049..083; Q-CONFIG-005 retains constructor/source/driver
   admission. No hypothetical source is assigned from register values alone.
2. Q-CONFIG-014 and Q-CONFIG-002: remaining Preferences readback, initialization,
   writer/frame history, alternate/reentrant parent and non-sentinel driver
   admission. Reuse FND-CONFIG-235..257, SOUND-082..096 and the existing
   Preferences helpers. FMT-CONFIG-006 predecessor review checkpoint is e0f597ab;
   reuse CONFIG-257 and its supersession links rather than repeating that review.
3. Q-EXE-033..037 and FMT-EXE-003: remaining entry mechanisms, return/alias,
   buffer-domain and callback obligations. Reuse EXE-644/645/647..655,
   CONFIG-225/226/227/230/231/234 and PARTY-339/342/343. Preserve unassigned/shared
   functions, contested transfers, walk gaps, missing inventories and exclusions.
   Consult docs/RESEARCH-CLOSURE.md; checklists alone promote nothing.
4. Q-PARTY-137/138/136: first-handoff backing/frame/native preservation, other
   roots/index changes/nested drains and shutdown admission. Reuse PARTY-288..296,
   314..331,337..343 and RNG-081/082/093/095. Retain Q-PARTY-134/135/124/121/120/116
   separately. checkpoint_rng.py admits sound/script-52 only; no untraced runner.
5. Other open work stays in queue/: Q-RNG-040/029, Q-SCRIPT-019/024/025,
   Q-CONFIG-008/013 and Q-SAVE-007 reuse SCRIPT-053..065, CONFIG-224..237,
   SAVE-027..044 and RNG-096..098. Q-COMBAT-059 owns post-error work. Earlier
   item/script contracts and captures remain indexed by their findings.

## Retained local references

- GAME_DIR/captures/rust-dos-setup-parser/: v181-second-select-20261109-b/
  and sibling c/ are complete; a/ is the host diagnostic (SOUND-109).
  SOUND-100..108 name earlier paired routes and retained partial diagnostics.
  Use da2f94a1's read-only transport recovery; mutations are never replayed.
- Static project: GAME_DIR/analysis/config-block-construction-20261108/SETUP-blocks.gpr.
  Reuse the controlled reports and exact regeneration commands named by findings.
- Additional retained stores: GAME_DIR/analysis/descriptor-pair-consumers-20261108/,
  preferences-root-contract-20261108/, music-getter-entry-census-20261108/ and
  header-pointer-reach-report-20261106.json. Earlier references remain in the
  17db43d8 goal checkpoint and the relevant findings/runtime records.
- Current-pin play-ready checkpoint:
  GAME_DIR/captures/rust-dos-regenerated/v181-q046-play-ready-20261104/.
  Saved-map checkpoint:
  GAME_DIR/captures/rust-dos-native-save/v181-q063-native-save-input-20261104/.
