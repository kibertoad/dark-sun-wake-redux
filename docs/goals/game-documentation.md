# Entire-game documentation

## Condition

Document the entire supported game through the research Audit exit: every
research queue item is closed or explicitly accepted by the owner as out of
reach, every required behavior and resource has an evidence-backed spec entry,
and the documentation check and repository gate pass. Temporary blockers and
completed individual batches do not complete this goal.

## Scope

Side: research. Areas: ACTOR, AI, COMBAT, EXE, EXPLORE, GFF, IMAGE, INPUT,
ITEM, MAGIC, PARTY, QUEST, REGION, RNG, SAVE, SOUND, TALK, TEXT, TIME, UI, VIDEO.
Resume from docs/HANDOVER.md. CONFIG and SCRIPT remain claimed by config-static;
shared tooling acceptance remains claimed by upstream-gap-resolution. Read their
entries as dependencies without modifying their claims or research.

## Must not touch

Gameplay implementation, other goals' owned entries and queues, generated
spec/index/ and PARITY.md, original-derived content in Git. No push without
an explicit owner request.

## Dead ends

- Default sandbox command creation fails before execution with Windows access
  denied. The supported require_escalated path successfully reads the checkout.
- Preserve prior Tried notes; repeat a question only with new evidence or a
  new reading, and split independently answerable obligations when required.

## Handover

- Stage: Slices; current work resumes the unfinished research in docs/HANDOVER.md.
- Last gate: prior session's 2026-10-15 Test.ps1 passed. This session's pinned
  rules verification and documentation check passed with existing skips.
- Unfinished: no research draft yet.
- Blockers: existing person-only runtime observations; see docs/RUNTIME.md.
- Next: Q-COMBAT-009 and its independently answerable state obligations;
  Q-COMBAT-006's unattempted local command helpers; Q-COMBAT-010;
  Q-RNG-001; Q-PARTY-082.
