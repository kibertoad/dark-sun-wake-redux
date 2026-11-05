# Full game research and mapping

## Condition

The game is fully researched and mapped through the research Audit exit:
every queue item is closed or the owner has accepted its evidence as out of
reach; installation/media accounting, format and executable-function coverage,
caller/state/indirect-transfer obligations and complete readings are audited;
the documentation check passes and every batch ends with its status block.
Temporary blockers and completed individual batches do not meet this condition.

## Scope

Side: research. Areas: all game specification areas, their queues and research
tooling. Continue the owner's standing full-game research objective. Existing
upstream-gap-resolution tooling work remains separate; do not modify its claim.
Use the local goal branch; no push is authorized by this continuation.

## Must not touch

Gameplay implementation in src/ or tests/. Original content in Git.
Generated spec/index/ and PARITY.md. Unowned original-game processes.

## Dead ends

- Node child-process Git reads fail with EPERM in the restricted shell; use
  supported sandbox escalation for checks requiring child processes. The
  documentation check passes with that access, retaining its declared skips.
- The --contexts profile at f21e2142 rejects a source spelling whose native
  lookup is ignored. FND-SOUND-037 retains that diagnostic; use the corrected
  profile from 595c7602 for complete source accounting, not the former
  tool rejection as evidence of native failure.

## Handover

- Stage: Slices; research Audit remains unmet.
- Last gate: 2026-11-05, Test.ps1 -Research -ChangedSince 5d12821d
  -NoRestore passed: 1899 entries, 208 parity rows and five deviations,
  retaining declared skips. Tooling integration at 5d12821d passed 202
  Node tests and 758 .NET tests. FND-SOUND-040's research batch also passed
  its gate against c5c359c4. Use project-local EVIDENCE_PYTHON for gates
  and commit hooks.
- Retained inputs: docs/HANDOVER.md names the existing canonical reports
  and checkpoints. FND-SOUND-030 through FND-SOUND-038 name static/source
  reports and exact repeats in the same approved local disc-ADV analysis
  store. New overflow/value, reviewed jumper-correction, first-exception
  diagnostics and complete source-context reports have verified repeat pairs.
- Utility runtime: FND-SOUND-039/041 and docs/RUNTIME.md name the fixed
  setup parser capability and limits. New captures remain under
  GAME_DIR/captures/rust-dos-setup-parser/ as
  v181-setup-records-20261105-a and v181-setup-records-20261105-b.
  Both input copies were reverified; sessions closed with muted sound.
  Use setup_parser.py at 5d12821d only for its fixed stops/reads. Retain
  native/expected buffers locally. Further lifetime/chooser observations
  require their own planned bounded extension and prior static attempt.
- Unfinished: none; no owned original emulator remains. Process audit also
  saw existing dotnet processes under other parents; none was stopped.
- Blockers: none known.
- Next: Q-CONFIG-006 (reuse FND-SOUND-034 through FND-SOUND-041 for remaining parser
  obligations), Q-CONFIG-011 (reuse FND-SOUND-030 through FND-SOUND-033),
  Q-CONFIG-012, Q-SOUND-010 and Q-PARTY-116. Keep source/profile conditions
  separate from native admission. Q-CONFIG-011/012 hold the independent
  stream and field-consumer obligations split from Q-CONFIG-006.
- Correction review: the separate read-only reviewer completed the required
  re-reading for FND-SOUND-037; no delegated work remains.
- Standing goal remains active; this handover is a continuation checkpoint,
  not an Audit exit, owner acceptance of inaccessible evidence or stop request.
