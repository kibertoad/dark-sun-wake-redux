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

- Stage: Slices; research-side tooling acceptance only. No original runtime, DOSBox or emulated calls; CONFIG/SCRIPT findings remain owned by config-static.
- Last gate: 2026-10-01, tools/Invoke-Validation.ps1 passes, including Test.ps1, locked restore, Release build and assetless publish/smoke. Latest audit logs: artifacts/incoming-controls-validation.log and artifacts/dispatch-pr-root-validation.log. Use artifacts/pwsh7/runtime/pwsh.exe with NoDefaultCurrentDirectoryInExePath unset.
- Setup: set GAME_DIR to C:\GOG Games\Dark Sun 2. Verify the DSUN.EXE length, XXH3-128 and SHA-256 in SOURCE-EDITIONS before querying. Local configs/reports/drivers are under GAME_DIR/analysis/reporter-audit: adopted-313bb7d-rerun, target-controls, boundary-controls, incoming-controls and dispatch-pr-controls. Never commit them.
- Unfinished: toolkit PR 33 (45db236) proposes evidenced indirect-jump CFG tables and relocated pointer exact-pair/alias inventories. Its isolated checkout is artifacts/upstream-dispatch-pr on tooling/evidenced-dispatch-pointer-inventories, pushed to upstream; no root reporter pin changed. Upstream local gates and recorded candidate controls pass. Gaps 11/13 remain open pending reviewed adoption and whole-request acceptance. Gaps 8/10/22/23 are closed in the ledger. Instruction-model acceptance remains incomplete for gaps 21/26/27/35/39/42. Gaps 4/5 still need the offline gate/disc-source case. Gap 9 requires a src/ catalog change outside this goal's scope.
- Blockers: no session-wide blocker. Do useful independent work while PR 33 is reviewed; do not treat the PR as adopted delivery. Facts gate passes.
- Next:
  1. Review toolkit PR 33 checks/review and address defects; once merged, propose exact template adoption with the new module/test files, review it and rerun source controls. Keep gaps 11/13 open until their entire acceptance contracts pass.
  2. Extend gap13 caller configurations with individually evidenced table declarations for the remaining selector/setup/relative paths. Do not bless call-site starts to bypass an unread dispatch. Verify all known positive controls and pointer exact/alias inventories, retaining unresolved pairs.
  3. Complete the remaining instruction-model controls in REPORTER-CASE-AUDIT: gap26-frame output/negative controls, bounded gap26/gap21 loops, narrowed gap27 query and bounded gap35/gap39 paths. Conditional callees remain assumptions.
  4. Deliver gap 4's explicit offline rerun in an upstream tooling PR and verify it locally; verify gap 5 against the distinct disc executable. Preserve the normal restoring CI gate and source identity.
  5. Continue other open requests in gaps.md; record whole-request acceptance before removal, run canonical validation/exact pins per finished batch and commit handover separately. Never push this restoration unless requested.
