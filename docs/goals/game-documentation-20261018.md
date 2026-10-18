# Entire-game research and mapping

## Condition

Document the supported game through the research Audit exit: every research
queue item is closed or explicitly accepted by the owner as out of reach,
every required behavior and resource has an evidence-backed specification,
and the documentation check and repository gate pass. Temporary blockers and
completed batches do not complete this goal.

## Scope

Side: research. Areas: ACTOR, AI, COMBAT, EXE, EXPLORE, GFF, IMAGE, INPUT,
ITEM, MAGIC, PARTY, QUEST, REGION, RNG, SAVE, SOUND, TALK, TEXT, TIME, UI, VIDEO.
CONFIG and SCRIPT remain claimed by config-static; shared tooling acceptance
remains claimed by upstream-gap-resolution. Read their entries as dependencies
without modifying their claims or research.

## Must not touch

Gameplay implementation, other goals' owned entries and queues, generated
spec/index/ and PARITY.md, original-derived content in Git. No push without
an explicit owner request.

## Dead ends

- Default sandbox shell creation fails with Windows access denied; the
  supported escalation mechanism successfully runs scoped repository commands.
- Child Git commands need the checkout-scoped safe.directory setting.
- Preserve existing Tried notes; retry only with new evidence or a new reading.
- Q-PARTY-082's generic selector/stack enumeration is already FND-PARTY-054;
  do not repeat it as evidence for the heads' gameplay roles.
- Full gates use an external temporary directory. The export fixture correctly
  rejects a temporary output inside the checkout. Git hook snapshots can use
  artifacts/hook-tmp through TMPDIR; keep EVIDENCE_PYTHON checkout-local.
- Pinned Capstone's 16-bit decoder mislabels unprefixed synthetic 98/99 as
  cwde/cdq rather than cbw/cwd. Check mode and operand-size prefixes directly;
  prior-analysis impact is unverified. Reported with a synthetic reproduction:
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/538.

## Handover

- Stage: Slices; resume the unfinished research in docs/HANDOVER.md.
- Last gate: 2026-10-18, ./tools/Test.ps1 -NoRestore passed: 715 .NET tests,
  105 Node tests and five option controls; documentation passed with 1367
  entries, 168 parity rows and five deviations. Existing argument-count and
  absent-Kaitai skips remain. Tracking warnings remain for Q-PARTY-020/078/082/085/087,
  Q-COMBAT-009/010 and Q-RNG-001/003/004; later attempts must reduce Settles it.
- Unfinished: no tracked research draft. Q-PARTY-007's screen/resource
  binding has no new finding yet; reports are in
  GAME_DIR/analysis/party-dual-window-20261018/.
  Cache caller/census reports remain in
  GAME_DIR/analysis/party-record-heads/drawing-20261018/.
  Use draw-linear-context.txt from the declared entry rather than restarting
  decoding at the inventory's internal fragment boundary.
- Blockers: existing person-only runtime observations in docs/RUNTIME.md.
- Next: Q-PARTY-007 screen identity and class-button mapping; Q-PARTY-087
  remaining direct/indirect cache writers and consumers; Q-PARTY-088/089
  resource/buffer and runtime binding/selection admission; Q-PARTY-086
  remaining action mutations; Q-COMBAT-014/015/016 remaining children and inputs.
- Checkpoint only: the standing research goal remains active. FND-COMBAT-051/052
  and FND-PARTY-220..231 are the current research references. No full-game run
  started; the gate retained its existing declared initializer call.
