# SCRIPT

Next ID: Q-SCRIPT-007

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
  trigger instructions. Tried: `CSEQ` resource 1000, the separate 19-byte list
  and the `SCMD` loader (FND-SCRIPT-016, FND-SCRIPT-018), none of which fills
  the 13-byte records; the trigger instructions fill them (FND-SCRIPT-015).
  Blocks: slices 3-6.
- Q-SCRIPT-003. RULE-SCRIPT-001, RULE-SCRIPT-002, RULE-SCRIPT-003,
  RULE-SCRIPT-004: What do the helpers of the interpreter do: `fn_172C_31ED`,
  the slot choice `fn_172C_07BB`, the buffer allocation `fn_172C_0698`, the
  error routine `fn_5702_00B1`, and the far routine at `g_57E0_02F6`; what do
  `g_4C0E_000B`, `g_4C13_032B`, `g_4C13_0325` and `g_4C0E_0002` mean; and which
  archive the resource routines read scripts from? Settles it: reading those
  routines, the writers of those globals, and the resource routines at
  `38FF:04AB` and `38FF:05B5`. Blocks: slices 3-6.
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
  selects, how a portrait number becomes a `PORT` resource, and what the sound
  and music routines play? Settles it: `5702:004D`, `5702:0052`, `5702:0057`
  and `5702:009D` in overlay 188, and `2D40:0B00` and `2D40:0B0F`. Blocks:
  slice 3.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.
