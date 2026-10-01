# Upstream gap resolution

## Condition

Fully resolve all 37 remaining requests recorded in gaps.md at goal intake. Each numbered request must have delivered behavior satisfying its entire acceptance contract and a recorded passing Dark Sun case where applicable. Remove only proven addressed requests, retain stable IDs, and open upstream PRs for any additional shared standard/protocol/tool limitation found. Upstream proposals alone do not prove adopted completion. Canonical validation and exact-source integrity must pass after each finished batch. No smaller completion condition or fixed turn limit.

## Scope

Areas: shared tooling, upstream rules/adoption and tooling acceptance records only. Research-side tooling batches. Read existing CONFIG/SCRIPT findings as acceptance inputs; do not claim or modify those research areas owned by config-static. No gameplay implementation.

## Must not touch

src/, game spec claims, parity statuses, other goals' queue items, proprietary content in Git, original runtime/DOSBox.

## Acceptance ledger

- Gap 38: closed; final merged policy-driven diagnostics/heap-dump protections adopted and verified.
- Gap 1: closed; merged template PR 33 refinements adopted and actual installed inventory reverified.
- Gap 2: closed; final merged script passes actual large-map page/name controls.
- Gap 3: closed; shared join exactly reproduces actual original-resident/mapped-overlay inventory.
- Gap 4: open; full request and cited controls in gaps.md.
- Gap 5: partial; portable/legacy identity checks pass, distinct disc-source validation pending; PR 33 merged and adopted.
- Gap 8: closed; entire request passes adopted target/format/boundary controls recorded in docs/REPORTER-CASE-AUDIT.md.
- Gap 9: open; full request and cited controls in gaps.md.
- Gap 10: closed; entire request passes adopted target/format/boundary controls recorded in docs/REPORTER-CASE-AUDIT.md.
- Gap 11: open; full request and cited controls in gaps.md.
- Gap 12: closed; actual two-entry tag/target controls and rejected third entry, recorded in docs/REPORTER-CASE-AUDIT.md.
- Gap 13: open; full request and cited controls in gaps.md.
- Gap 15: open; full request and cited controls in gaps.md.
- Gap 16: closed; reviewed merged source adopted and actual controls reverified.
- Gap 17: open; full request and cited controls in gaps.md.
- Gap 20: open; full request and cited controls in gaps.md.
- Gap 21: open; full request and cited controls in gaps.md.
- Gap 22: closed; entire request passes adopted target/format/boundary controls recorded in docs/REPORTER-CASE-AUDIT.md.
- Gap 23: closed; entire request passes adopted target/format/boundary controls recorded in docs/REPORTER-CASE-AUDIT.md.
- Gap 24: closed; reviewed merged source adopted and actual controls reverified.
- Gap 25: closed; reviewed merged source adopted and actual controls reverified.
- Gap 26: open; full request and cited controls in gaps.md.
- Gap 27: open; full request and cited controls in gaps.md.
- Gap 29: open; full request and cited controls in gaps.md.
- Gap 30: open; full request and cited controls in gaps.md.
- Gap 31: open; full request and cited controls in gaps.md.
- Gap 32: open; full request and cited controls in gaps.md.
- Gap 33: open; full request and cited controls in gaps.md.
- Gap 34: open; full request and cited controls in gaps.md.
- Gap 35: open; full request and cited controls in gaps.md.
- Gap 36: open; full request and cited controls in gaps.md.
- Gap 37: open; full request and cited controls in gaps.md.
- Gap 39: open; full request and cited controls in gaps.md.
- Gap 40: open; full request and cited controls in gaps.md.
- Gap 41: open; full request and cited controls in gaps.md.
- Gap 42: open; full request and cited controls in gaps.md.
- Gap 43: open; full request and cited controls in gaps.md.

## Dead ends

Synthetic success and broad guidance do not close a game-case request. Current static conditional call models invalidate memory/flags; preserve no effects without evidence. Existing cases stopped by unread callees or loop/path caps remain incomplete.

## Handover

- Stage: Slices; research-side tooling acceptance only. The next session reruns Dark Sun reporter cases statically: no DOSBox, no emulated or native run of the original, and no change to CONFIG/SCRIPT findings (config-static owns them).
- Last gate: 2026-10-01, tools/Invoke-Validation.ps1 passes, including Test.ps1, locked restore, Release build and assetless publish/smoke (artifacts/reporter-rerun-validation.log). Run it from repository-local PowerShell 7 (artifacts/pwsh7/runtime/pwsh.exe) with `NoDefaultCurrentDirectoryInExePath` unset, or the play-launcher test cannot start its `cmd.exe` child.
- Setup: `GAME_DIR` is not set in the environment; set it to `C:\GOG Games\Dark Sun 2` for the session. Existing case configurations, reports and the run.mjs/verify*.mjs drivers are in GAME_DIR/analysis/reporter-audit; docs/REPORTER-CASE-AUDIT.md names each case and its expected control. New configurations and reports go in the same directory and are never committed. Check DSUN.EXE against the length, XXH3-128 and SHA-256 in docs/SOURCE-EDITIONS.md before querying. Commands and inputs are in docs/EVIDENCE-TOOLS.md and docs/BOUNDED-EVIDENCE-REPORTERS.md.
- Unfinished: reporter 313bb7d (website ca39d07, template 8d0eef3) is adopted. Baseline and existing conditional instruction-model cases were rerun; acceptance dispositions are recorded in docs/REPORTER-CASE-AUDIT.md. Raw reports and replay drivers are in GAME_DIR/analysis/reporter-audit/adopted-313bb7d-rerun. Gaps 8, 10, 11, 13, 21, 22, 23, 26, 27, 35, 39 and 42 wait on those cases. Gaps 4 and 5 wait on the offline gate and on disc-source verification. Gap 9 needs a UI catalog fix in this repository, outside this goal's scope.
- Blockers: none. The facts gate passes (`tools/Bootstrap-Project.ps1 -ValidateFactsOnly`).
- Next:
  1. Complete the remaining instruction-model controls recorded in REPORTER-CASE-AUDIT.md: add explicit expected-output/negative controls for gap26-frame; narrow the gap27 conditional query below the output cap; bound gap26/gap21 loops from existing findings; select bounded gap35/gap39 paths without treating conditional callees as established effects. Existing reruns alone do not close those requests.
  2. Write new x86-target cases with `formatControls`: gap 8 (the Start Game setup helper's FBOV far calls with encoded segments 0160 and 01D0, including one call with no fixup as the negative control), gap 10 (FND-CONFIG-031's resident MZ far call) and gap 22 (FND-CONFIG-183 and FND-COMBAT-009 through descriptor and trampoline, with the build's range and fixup counts as positive controls).
  3. Write x86-bounds and x86-owner cases: gap 23 (FND-CONFIG-151 and FND-CONFIG-158's mode-setter branch, reporting holes) and gap 11 (FND-CONFIG-092's chain across the overlay setup routine).
  4. Rerun gap13-selector/-setup/-later-relative with declared `segments` and read `coverage`, `partialSearch`, `position` and `unresolvedTransfers`: FND-CONFIG-101 needs all eleven selector calls, FND-CONFIG-111 all six setup calls with the stored inventory as positive control, plus the FND-CONFIG-128 relative calls and FND-CONFIG-114 pointer canonicalization.
  5. Record each result in REPORTER-CASE-AUDIT.md (rewrite the case row, add new ones), close a gaps.md entry and move it to the ledger only when its whole request passes, open an upstream issue or PR for any new reporter limit, then run the gate and commit one batch.


