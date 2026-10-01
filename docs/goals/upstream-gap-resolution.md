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
- Gap 8: open; full request and cited controls in gaps.md.
- Gap 9: open; full request and cited controls in gaps.md.
- Gap 10: open; full request and cited controls in gaps.md.
- Gap 11: open; full request and cited controls in gaps.md.
- Gap 12: closed; actual two-entry tag/target controls and rejected third entry, recorded in docs/REPORTER-CASE-AUDIT.md.
- Gap 13: open; full request and cited controls in gaps.md.
- Gap 15: open; full request and cited controls in gaps.md.
- Gap 16: closed; reviewed merged source adopted and actual controls reverified.
- Gap 17: open; full request and cited controls in gaps.md.
- Gap 20: open; full request and cited controls in gaps.md.
- Gap 21: open; full request and cited controls in gaps.md.
- Gap 22: open; full request and cited controls in gaps.md.
- Gap 23: open; full request and cited controls in gaps.md.
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

- Stage: Slices; tooling maintenance only.
- Last gate: 2026-10-01, tools/Invoke-Validation.ps1 passes (137 Python, 77 Node, 700 .NET); artifacts/reporter-provenance-adoption.log.
- Unfinished: website ca39d07, toolkit 313bb7d (checker f7da132) and template 8d0eef3 are adopted. Gaps 8, 10, 11, 13, 21, 22, 23, 26, 27, 35, 39 and 42 now have reporter support whose Dark Sun cases are unrun. Gap 9 needs a game-repo fix in the UI catalog, outside this goal's scope.
- Blockers: the facts gate (docs/SOURCE-EDITIONS.md) refuses executable analysis until latest-patch provenance and the baseline SHA-256 are recorded, so no game case can be rerun.
- Next: once the gate passes, run x86-target on the FND-CONFIG-031 and FND-CONFIG-183/FND-COMBAT-009 calls with formatControls, x86-bounds/x86-owner on FND-CONFIG-092, -151 and -158, incoming with declared segments for FND-CONFIG-101/-111/-113, and the stopped trace cases with visitLimit; then distinct disc-source verification for gap 5 and the offline gate for gap 4.
