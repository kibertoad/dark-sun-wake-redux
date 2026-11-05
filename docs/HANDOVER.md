# Handover

## Current state and retained inputs

- Stage: Slices. Full research Audit remains unmet. Full-game goal claim cleared.
  Owner cleared the goal on 2026-11-04 and requested batch completion,
  documentation, progress, push to main and stop. Goal dropped, not achieved.
- Latest research: FND-SOUND-019 through FND-SOUND-029 and FND-SOUND-017, with FND-ITEM-071/072,
  with FND-ITEM-043 through FND-ITEM-070;
  active corrected DUAL evidence is FND-PARTY-262/263. Q-ITEM-009 is closed.
  FMT-ACTOR-001 retains its FND-COMBAT-162 Defect note.
  Q-SOUND-008 is closed; FMT-SOUND-004/005 are supported, code still missing.
- Latest research gate: 2026-11-04, Test.ps1 -Research -ChangedSince f3bcd060
  -NoRestore passed. Documentation, queue, policy and package checks passed.
  Tooling gate: Test.ps1 -ChangedSince b418fe72 -NoRestore passed,
  including source record-span controls. Last integration:
  2026-11-04, Test.ps1 -NoRestore passed all Node controls and 758 .NET tests. Existing argument-count/generation skips and 61 uncompiled
  Kaitai definitions remain.
- Select artifacts/evidence-python/Scripts/python.exe explicitly through
  EVIDENCE_PYTHON; the inherited override belongs to another project. Use UTF-8.
  Each research gate uses the pre-batch commit as ChangedSince. Uncertain or
  mixed input ownership requires integration; staged hooks remain enabled.
- Runtime: session 1.0.0 and rust-dos v1.8.1 are pinned and installed.
  Follow docs/RUNTIME.md and the existing session-package contract, muted host
  output and machine run lock. Never operate an unowned emulator.
  Valid current-pin checkpoints are play-ready under
  GAME_DIR/captures/rust-dos-regenerated/v181-q046-play-ready-20261104/ and
  saved-map under
  GAME_DIR/captures/rust-dos-native-save/v181-q063-native-save-input-20261104/.
  Current accepted paused table observation is
  GAME_DIR/captures/rust-dos-native-save/v181-q109-placed-table-20261104/;
  native-save.json retains the complete original-derived report privately.
  FND-ITEM-067 and CHECKPOINT-METADATA name its command, bounds and limits.
  No gameplay continuation, input or guest writes occurred in that observation.
  The fixed startup flag observation is retained under
  GAME_DIR/captures/rust-dos-startup-segments/v181-q118-main-flag-20261104/;
  FND-ITEM-071 names its guarded endpoint and reproduction command.
- Disc-audio Survey reports and exact repeats are retained under
  GAME_DIR/analysis/disc-audio-survey-20261104/. FND-SOUND-017 and
  DISC-AUDIO-SURVEY name the committed inspector, source controls and limits.
  No decoder or GOG runtime execution was attempted.
- ADV reports/repeats and setup listings are retained under
  GAME_DIR/analysis/disc-adv-survey-20261104/. FND-SOUND-019 through FND-SOUND-029 name the
  envelope, tag/dispatch/case controls and initial-binding listings.
  Keep the +0A00..0A74 linear decoder output diagnostic: it is table data.
  FND-SOUND-021 names the header/top dispatch and incoming controls plus
  selection, fallback, label-preparation and dialog listings. The separately
  named setup-choice-labels listing is not the label builder.
  FND-SOUND-037/029 name setup-jumper-jumps controls/repeat and
  setup-source-record-spans report/repeat.
  Q-SOUND-007 retains other setters/callers and later state preservation;
  Q-SOUND-010 owns dialog row/index, storage and child-state admission.
  Source profile reports/repeats are setup-source-label-profile.json and
  setup-source-label-profile-repeat.json in that directory. FND-SOUND-022
  names startup/concat and vector listings; retain its conditional bounds.
  FND-SOUND-024/025/026/027/028/029 name the group/count, decimal scanner, allocation,
  clearing and label-formatter listings. Number-formats, scan/print classes
  and jumps have exact report/repeat pairs there. Use the findings' exact
  ranges, not adjacent linear table decodes. Next SOUND ID is 011. Q-CONFIG-006 retains other
  numeric inputs, bounds and complete file/error/child contracts. Q-SOUND-010
  retains native heap/break, vector, state and chooser/child admission.
- Earlier paused getter/header/item-type observations are FND-COMBAT-169/170/171.
  FND-COMBAT-133 through FND-COMBAT-168 retain load/save/caller obligations.
  Session event_log is null; do not reconstruct unreported journals or bytes.
  Keep FND-EXE-562/FND-COMBAT-142's trap/frame guards and remove a consumed
  return watch before interrupt-frame reuse. restore_caller.py's fixed failure
  and synthetic control remain authoritative; do not repeat its old deletion.
- Canonical item research reports and entry-based listings remain under
  GAME_DIR/analysis/item-removal-20261104/. FND-ITEM-043 through FND-ITEM-067
  give exact controls and regeneration procedures. New joint rows are
  item-hook-inputs.json; the old type-50 Look rows are not an all-item join.
  FND-ITEM-063 names area-list/range/object-scan and data42 reports;
  FND-ITEM-064 names area-filter-complete and paired table reports;
  FND-ITEM-065 names effect-row constructor/insert/search/insert-leaf listings;
  FND-ITEM-066 names placement-table-allocation.txt; FND-ITEM-068 names
  placement-effect-samples.txt there. FND-ITEM-069 names the exact/overlap
  equipment-flag operand reports and census controls there. FND-ITEM-070
  names equipment-flag-startup.txt there. FND-ITEM-072 names the new
  equipment-sound-gates.txt and placement-sound-data/source-summary reports.
  Partial area-filter and generic-transfer-bindings listings remain diagnostic.
  placement-caller-incoming.txt, temporary-presentation-candidates.txt,
  effect-code-indexed-store-candidates.txt and hit-flags windows are candidate
  diagnostics, not complete writer censuses. Keep canonical inputs and captures.
- Shared heap prerequisites remain FND-RNG-093, FND-CONFIG-209/210/211/212
  and FND-PARTY-215; Q-CONFIG-008 and Q-SCRIPT-003 retain their admission.
  Active script corrections are FND-SCRIPT-038 and FND-PARTY-261.
  Script reports remain under GAME_DIR/analysis/walk-script-20261104/.
- Next: Q-SOUND-010 and Q-CONFIG-006 retain native admission and parser gaps.
  Q-PARTY-109 retains action/owner/item/reset and other child admission.
  Independent obligations are Q-PARTY-116 counted row publication/lifetime,
  Q-PARTY-117 DATA coherence and Q-PARTY-118 flag producers/consumers.
  Reuse FND-ITEM-056 through FND-ITEM-072 and the controlled flag reports;
  the paused endpoint is not a constructor observation. Next PARTY ID is 119.
- Q-PARTY-077's DUAL slot lifetime, using FND-PARTY-262/263. Keep its actual
  action/target conditions separate from the equipment source composition.
- Q-ITEM-003's native placement/graph/class retention, Q-ITEM-005/006's other
  consumers/writers and Q-ITEM-013's aliases. Q-ITEM-011 is the separate segment
  prerequisite; spell/store work is Q-ITEM-014/015. Reuse FND-ITEM-044..064.
- Q-SCRIPT-017/023's coordinate/grid/DS/stack and reset preservation;
  Q-PARTY-115's other name/type producers, using FND-ITEM-036..042 and
  FND-SCRIPT-036..041 rather than repeating those bounded readings.
- Q-COMBAT-067 and Q-COMBAT-045 cleanup/save admission; Q-COMBAT-064/065/066
  shared record, pointer/backing and caller/child lifetime. Reuse
  FND-COMBAT-149..171 and FND-COMBAT-032/033/046 censuses.
- No unfinished batch or owned process remains. Research stopped at owner
  request. This is not an Audit exit or acceptance of out-of-reach evidence.
  docs/PROGRESS.md is the current generated snapshot.
