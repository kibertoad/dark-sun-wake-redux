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

- Stage: Slices; full research Audit remains unmet. This is a continuation
  checkpoint, not owner acceptance of inaccessible evidence or a stop request.
- Last gate: 2026-11-05, Test.ps1 -Research -ChangedSince ef7f9a51
  -NoRestore passed: 1921 entries, 208 parity rows and five deviations,
  retaining declared skips. Tooling at 697e811f passed selected evidence
  controls against 25461ebb. Prior full integration remains 5d12821d.
  Use project-local EVIDENCE_PYTHON for gates and commit hooks.
- Current priority: Q-PARTY-116, FMT-PARTY-006. Reuse FND-ITEM-063 through
  FND-ITEM-068 for scanner/filter/constructor/allocation/caller inputs and
  FND-ITEM-073 through FND-ITEM-087 for removal/queue/cell/redraw/unload
  and descriptor acquisition/caller/eviction/refresh/pointer compositions.
  Latest batch is 7052c768.
  Next assess remaining descriptor/state producers and
  remaining cleanup children or an actual constructor/use-time
  state-writer obligation. FND-PARTY-213 supplies existing cleanup facts;
  FND-PARTY-262/263 retain corrected request/slot-lifetime limits. A paused
  endpoint is not a constructor call. Do not repeat only a recorded local
  copy, selected handler, cell leaf, redraw wrapper or flagged residual gate.
- Retained effect evidence is under GAME_DIR/analysis/item-removal-20261104/.
  Each finding names its canonical reports, repeated pairs and digests.
  New effect-unload-flagged-gate.txt and its -repeat pair exist and match.
  effect-unload-complete.txt retains an exploratory full-body listing for
  later cleanup work, not a complete-reading declaration. Earlier canonical
  removal/queue/consumer/cell/redraw pairs remain there; use source-specific
  MZ mappings, not the setup utility's header/data layout. Native row/slot,
  map/queue/page/source and neighbor preservation remains explicit in queue.
- Later unload: FND-ITEM-079 names effect-unload-later-contract.txt
  and its matching -repeat pair under the same local store. Reuse its
  extra-row/kind/frame and cleanup-call binding. Native metadata/frame
  bounds, termination and named child effects remain Q-PARTY-116; do not
  repeat only the already recorded later body.
- Common cleanup: FND-ITEM-080 names effect-common-cleanup-contract.txt
  and its matching -repeat pair under the same local store. Reuse its
  full local cleanup/fill/resource-wrapper contract; native header/count/
  descriptor/object identity and event/heap/chunk/free-list/graphics
  children remain Q-PARTY-116. Return/null/fill is not child-success proof.
- Acquisition: FND-ITEM-081 names effect-descriptor-acquire-contract.txt
  and its matching -repeat pair in the same store. Reuse its local number-hit,
  content-hit and fresh-publication paths. Native caller selectors, actual
  resource lengths, descriptor counts and other writers remain Q-PARTY-116.
  The content comparison uses byte length while fresh records store frame count;
  preserve that distinction without inferring a native defect.
- Acquisition callers: FND-ITEM-082 names effect-descriptor-callers-contract.txt
  and its matching -repeat pair. Reuse its complete local caller/decrement
  contracts; native object/resource/count inputs, other writers, indirect
  sites and the flag-one child's effects remain Q-PARTY-116. Do not reread
  only the automatic-selector or decrement-zero gate.
- Eviction cleanup: FND-ITEM-083 names effect-eviction-cleanup-contract.txt
  and its matching -repeat pair. Reuse its saved-handle/count gates and
  neighbor/object/child ordering. Actual eviction/link/descriptor inputs,
  other writers and transitive child effects remain Q-PARTY-116; no native
  count invariant or exhaustive writer coverage was declared.
- Image refresh: FND-ITEM-084 names effect-image-refresh-contract.txt and
  its matching -repeat pair. Reuse its complete local recursion, image
  overrides, handle publication and repeated dimension-read ordering.
  Link termination, native owner/cache/frame/buffer admission and 3568
  effects remain Q-PARTY-116; refresh AL does not cancel parent acquisition.
- Pointer helper: FND-ITEM-085 names effect-pointer-normalization-contract.txt
  and its matching -repeat pair. Reuse its signed-displacement, normalized
  register-result and no-data-mutation contract. Native buffer admission
  remains open; 060E itself is no longer an unread child obligation.
- Bitmap provider: FND-ITEM-086 names effect-bitmap-provider-contract.txt
  and its matching -repeat pair. Reuse its hit/tag/reload and miss/count
  publication gates. Native count/flags/offset and archive size/data identity,
  remaining reload/allocation and backing-store children remain Q-PARTY-116.
  A returned index is not successful content transfer or frame admission.
- Reload/allocation: FND-ITEM-087 names
  effect-bitmap-reload-allocation-contract.txt and its matching -repeat pair.
  Reuse local pre-read/index/allocation/transfer and pool search/reset gates.
  Actual pool/cache state, 3A66 reclamation, 0586 pointer update, backing-store
  and remaining reset-child effects stay Q-PARTY-116. A null allocation may
  follow global cleanup requests; no native failure was claimed.
- Setup: Q-CONFIG-006/011/012 and Q-SOUND-010 remain open. Reuse
  FND-SOUND-030 through FND-SOUND-048 and retained reports under
  GAME_DIR/analysis/disc-adv-survey-20261104/. The census exclusions,
  field/formatter contracts and exact repeat pairs are named in those
  findings. partialSearch=false is not a complete reading. No unchanged
  exact-literal/direct-transfer or plain formatter search is needed again.
- Utility runtime: FND-SOUND-039/041 and docs/RUNTIME.md name the fixed
  setup capability. Captures remain under
  GAME_DIR/captures/rust-dos-setup-parser/ as
  v181-setup-records-20261105-a and v181-setup-records-20261105-b.
  Input copies were reverified and owned muted sessions closed. Use the
  fixed setup_parser.py stops/reads only; keep buffers locally. Extensions
  need their own plan, controls and prior static attempt.
- Shared retained inputs: docs/HANDOVER.md names earlier stores/checkpoints.
  The separate review for FND-SOUND-037 completed; no delegated work remains.
  Existing DSUN-only scripts do not accept setup inputs.
- Unfinished: none. Final process audits found only their own shell among
  inspected emulator/analysis/runtime names. No original ran during this
  effect-row batch. Blockers: none known.
