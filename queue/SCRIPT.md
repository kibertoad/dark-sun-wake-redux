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
  reads another pointer consumer's ordered list/count changes and child
  failure; the caller ignores its result. FND-CONFIG-169 reads the
  following helper's mode gates, callback and resource requests without
  success-result branches. Full external
  effects and returns, interrupt outcomes,
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
