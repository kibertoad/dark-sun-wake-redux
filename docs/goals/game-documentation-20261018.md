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

## Handover

- Stage: Slices; resume the unfinished research in docs/HANDOVER.md.
- Last gate: pinned rules verify and documentation check passed on 2026-10-18
  with the existing argument-count, Kaitai and scheduled-generation skips.
- Unfinished: no research draft yet.
- Blockers: existing person-only runtime observations in docs/RUNTIME.md.
- Next: Q-COMBAT-014/015/016, then the remaining prioritized handover items.
