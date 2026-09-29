# SCRIPT

Next ID: Q-SCRIPT-008

## Static

- Q-SCRIPT-001. FMT-SCRIPT-003, FMT-SCRIPT-004: When does the game convert the
  script entry points of the trigger records to `GPLI` entry numbers and back,
  and where are the trigger tables allocated and saved? Settles it: the callers
  of the two conversion routines of overlay 187 and the code that writes the
  far pointer at `57E0:40C0`. Tried: a literal tag search of the resident image
  (FND-SCRIPT-004), and a reading of overlay 187 (FND-SCRIPT-017), which shows
  the conversions but not when they run. Blocks: slices 2-4.
- Q-SCRIPT-002. RULE-SCRIPT-008, FMT-SCRIPT-004, FMT-SCRIPT-005: What do the
  keys and bytes of each trigger list and the 19-byte records stand for, and
  when does the game test each list and run its scripts? Settles it: the code
  that writes the words at `4C0D:0001` to `4C0D:0009`, the callers of the four
  walkers in segment `1695` and of `2D40:0EF1`, and the handlers of the other
  trigger instructions. Tried: `CSEQ` resource 1000, an XMIDI file
  (FND-SOUND-015), the separate 19-byte list and the `SCMD` loader
  (FND-SCRIPT-016, FND-SCRIPT-018), none of which fills the 13-byte records; the trigger instructions fill them (FND-SCRIPT-015).
  Blocks: slices 3-6.
- Q-SCRIPT-003. RULE-SCRIPT-010, RULE-SCRIPT-002, RULE-SCRIPT-003,
  RULE-SCRIPT-004: What do the helpers of the interpreter do: `fn_172C_31ED`,
  the slot choice `fn_172C_07BB`, the buffer allocation `fn_172C_0698`, the
  error routine `fn_5702_00B1`, and the far routine at `g_57E0_02F6`; what do
  `g_4C0E_000B`, `g_4C13_032B`, `g_4C13_0325` and `g_4C0E_0002` mean; and which
  archive the resource routines read scripts from? Settles it: reading those
  routines, the writers of those globals, and the resource routines at
  `38FF:04AB` and `38FF:05B5`. Tried: FND-SCRIPT-019 corrects the
  cache early returns, branch-specific age updates, double-word size output,
  low-word allocation arithmetic and pre-transfer state writes. FND-SCRIPT-020
  reads the guarded four-buffer reset; FND-CONFIG-160 reads the status-gated
  script-99 entry. FND-SCRIPT-021 reads replacement selection and writes.
  FND-SCRIPT-022 reads allocation search, comparisons and restart paths.
  FND-SCRIPT-023 reads the error entry and stop setter; FND-CONFIG-161
  bounds its first shared helper. FND-CONFIG-162 reads that helper's first
  callee and two polls; FND-CONFIG-163 bounds its setter guard and runtime
  mode. FND-CONFIG-164 reads the common poll's register/output contract.
  FND-CONFIG-165 bounds the pointer wrapper; FND-CONFIG-166 reads
  state-clear/status gates. FND-CONFIG-167 reads runtime dispatch and a
  local rejection path whose result the wrapper discards. FND-CONFIG-168
  reads another pointer consumer's ordered list/count changes and
  conditional child-result exit; the caller ignores its result. FND-CONFIG-179 reads the
  following helper's mode gates, callback and resource requests without
  success-result branches. FND-CONFIG-170 resolves local
  retained-result restoration through the near-state collector.
  FND-CONFIG-171 reads following resident callback gates, fresh targets,
  gated helper calls and fixed word writes. FND-CONFIG-172
  bounds child cleanup, recursive error propagation without a leaf origin,
  and ignored EBOX results. FND-CONFIG-173 reads the intervening list
  helper's signed gates, pointer walks and checked/ignored results.
  FND-CONFIG-174 reads the guarded EBOX dependency's common zero return;
  FND-CONFIG-175 bounds bracketed guard-bypass SI/DS and copy contracts.
  FND-CONFIG-176 reads setup and append result contracts and a count-16
  pre-write rejection. FND-CONFIG-177 reads wrapper staging, sentinel
  bypasses and conditional pair expansion. FND-CONFIG-178 bounds 0180's
  checked private append and output routes and the 0DEC self-copy, retaining
  full geometry and native input provenance.
  FND-CONFIG-180 resolves bitmap result gates and the callback/mask
  argument widths, superseding FND-CONFIG-169 through FND-CONFIG-179.
  FND-CONFIG-181 reads the filename conversion and archive requests,
  retaining near-DS/far-SS provenance and external path/error effects.
  FND-CONFIG-182 reads the path append and runtime scan/copy helpers;
  signed lengths, prefix-dependent zero placement, aliases, capacities
  and reachable filename/prefix inputs remain conditional.
  FND-CONFIG-183 reads the initializer's checked allocation/handle gates
  and raw descriptor/trampoline target; FND-CONFIG-184 reads failure
  cleanup's ordered record/pointer/handle writes and VGA dependency.
  FND-CONFIG-185 reads the named zero-option resident resource call.
  Full producer, allocation/runtime, service and termination outcomes
  remain open; a returning result alone does not settle the parent gate.
  FND-CONFIG-186 reads the temporary byte's earlier clear, local helper
  gates and mode/number changes. FND-CONFIG-187 bounds replacement's
  size/result asymmetry, unchecked transfer and later cache/byte writes.
  Complete child/service, register, segment, state and content producers
  remain open; none of these paths proves native message entry or timing.
  FND-CONFIG-188 reads pointer reset/replacement and word guards over
  low-byte writes. FND-CONFIG-189 reads active callback, handle and
  state-commit/restoration routes, including unchecked +5E targets and
  stored FFFF paths. Neighboring bytes, full producers/targets, primitive,
  runtime and VGA outcomes still prevent an end-to-end entry claim.
  FND-CONFIG-190 reads frame count/index and dimension contracts;
  FND-CONFIG-191 reads partial request metadata and wrapper/coordinate
  gates before the graphics primitive. Complete accepted image/slot
  producers, bounded reference chains and primitive effects remain open.
  FND-CONFIG-192 reads the complete local graphics primitive's shared
  preparation, reference walks, direction/overlap gates, port accesses
  and phased copies. Accepted root/mask/segment inputs, capacities,
  aliases, full callers and native VGA outcomes remain open.
  FND-CONFIG-193 reads one fixed-root/free-slot initializer and
  setup call, with no local pre-call bypass under returning callees.
  Later state writers and actual startup/hardware outcomes remain open.
  FND-CONFIG-194 reads the release gate and compaction chain;
  accepted slots, finite progress and hardware effects remain open.
  FND-CONFIG-195 connects the pointer consumer to a direct release call
  outside the signed wrapper; record fields and accepted handles remain open.
  Full external effects and returns, interrupt outcomes,
  cache/capacity inputs, valid pointers,
  aliases, complete input provenance and actual archive I/O remain open.
  Blocks: slices 3-6.
- Q-SCRIPT-004. RULE-SCRIPT-002, RULE-SCRIPT-004, RULE-SCRIPT-008,
  FMT-SCRIPT-001, FMT-SCRIPT-002: What do the instructions do whose handlers
  the spec does not describe yet, what do `fn_172C_284D`, `fn_172C_2914`,
  `fn_172C_31B7` and `fn_172C_325F` do, and how are strings of kind 2 stored?
  Settles it: the handlers FND-SCRIPT-005 lists, read one instruction family at
  a time, and those four routines. Blocks: slices 3-6.
- Q-SCRIPT-005. RULE-SCRIPT-004, RULE-SCRIPT-005: How many script variables of
  each kind are there, where and when does the game clear them, are local
  variables cleared on a region change, does a saved game hold them, and what
  do `global_name_pointers` point at? Settles it: the code that sets the far
  pointers at `4C13:032F` to `4C13:034B` and `4C13:0000` to `4C13:003C` and
  clears what they point at. Blocks: slices 3-6.
- Q-SCRIPT-006. RULE-SCRIPT-007: What do the far routines behind the output
  instructions do: where printed text goes and what its byte parameter
  selects, and how a portrait number becomes a `PORT` resource? Settles it:
  `5702:004D`, `5702:0052`, `5702:0057` and `5702:009D` in overlay 188.
  Tried: the sound and music routines `2D40:0B00` and `2D40:0B0F`, which play
  the effect and do nothing (FND-SOUND-009). Blocks: slice 3.

## Emulated call

- Q-SCRIPT-007. RULE-SCRIPT-010: Do resident loader and interrupt-free helper
  cases agree with the documented branches and local state effects?
  Settles it: after the harness in docs/RUNTIME.md exists, fixtures using
  the rule's parameters and glossary fields for stop/current-pair paths,
  matching and duplicate cache slots, selected-number bypass, invalid
  number/selector and signed age edges, plus replacement maxima, ties,
  mixed/all-negative ages and fields left unchanged (FND-SCRIPT-021).
  Add room-search cases for empty/fragmented bounds, equality, zero and
  equal-capacity sizes, word-end wrap, signed capacity and restart progress
  (FND-SCRIPT-022). Once their field layouts are supported, add valid
  early-zero status guards (FND-CONFIG-166), segment-zero dispatch and
  interrupt-free normalized-address/rejection cases (FND-CONFIG-167).
  Add near-copy count and alias cases once their layouts are supported
  (FND-CONFIG-170), and valid callback-zero/helper-gate/fixed-word-copy
  cases (FND-CONFIG-171). Add finite MENU propagation, named record-walk
  equality/signed-index,
  zero inner pointer-field and valid EBOX bypass cases (FND-CONFIG-172),
  plus signed outer-count/first-record/child gates (FND-CONFIG-173)
  once their layouts are supported. Indirect targets and callees need
  separate coverage. Add 1675 gate/signed-word cases (FND-CONFIG-174)
  and bracketed copy-direction, null-copy, coordinate and guard-bypass
  SI/DS/changed-ES cases (FND-CONFIG-175) after supported layouts exist.
  Add fill/count-16 append, valid one-record setup, signed extrema,
  sentinel and staged-output/pair-expansion cases (FND-CONFIG-176,
  FND-CONFIG-177) after supported layouts exist. The unfinished region
  helpers need separate complete branch coverage. FND-CONFIG-178 adds
  equality/sentinel/copy, private capacity-error and same-pointer 0DEC
  cases; geometry paths need independently supported case definitions.
  FND-CONFIG-182 adds resident path skip, full append and truncation
  cases with empty/nonempty prefixes, signed/wrapped lengths, null and
  scan-limit results, odd/even byte copies and explicit zero placement.
  Add them after supported argument/storage layouts exist; overlay
  callers, producer reachability and archive I/O need separate evidence.
  FND-CONFIG-183 adds resident slot exhaustion, geometry/product and
  paragraph-capacity cases after supported inputs exist. FND-CONFIG-185
  adds the resident word-eight setter and fixed table-fill cases; active
  region calls need complete service/resource input definitions first.
  FND-CONFIG-184 adds signed at-most-one handle-wrapper bypass cases.
  Its VGA path, overlay initializer/cleanup and process termination cannot
  be established by interrupt-free resident calls.
  FND-CONFIG-186 adds resident mode equality, signed selected-word and
  cached-number bypass cases plus the mode-five null-pointer byte write.
  FND-CONFIG-187 adds the resident nonzero bracket bypass and signed
  byte-decrement/clamp cases after supported fields and complete service
  boundaries exist. Overlay resource/cache paths need separate coverage;
  register/segment preservation and native outcomes remain independent.
  FND-CONFIG-188 adds valid null/non-null wrapper cases with both word
  services bypassed, checking pointer/default/index fields and AX zero.
  FND-CONFIG-189 adds direct word-guard and FFFF-mode exits plus
  rectangle-helper call-selection cases once supported fields, layouts
  and primitive boundaries exist. Indirect callbacks, active service and
  VGA children require separate complete target/input coverage; a zero
  wrapper result alone cannot establish them.
  FND-CONFIG-190 adds valid synthetic image count-zero/index-equality,
  raw dimension and normalized-offset cases using supported image fields.
  FND-CONFIG-191 adds slot exhaustion, finite reference chains, successive
  coordinate-failure metadata, negative handle and AL-coordinate endpoint
  gates after supported slot/argument layouts exist. An admitted 43AE
  primitive needs separate complete input/effect coverage; wrapper calls
  alone cannot establish actual graphics or hardware outcomes.
  FND-CONFIG-192 locates port I/O in every admitted primitive transfer
  route. Do not treat RAM-only MOVS emulation or mocked ports as evidence
  of accepted scratch/native pixels; its input/reference/mask and hardware
  outcomes need separate evidence from pure resident helper fixtures.
  Resource and error branches need separate provenance and cannot
  establish operating-system outcomes by emulation. Tried: FND-SCRIPT-019 reads the local instruction paths;
  the emulator harness does not exist yet.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.
