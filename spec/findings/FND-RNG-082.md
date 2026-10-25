---
id: FND-RNG-082
title: In each of the ten ADV drivers with a timer function 0067, functions 0067, 0xB5 and 0xC1 write only their stack frames, the driver's own code segment and the 0x208-byte sequence blocks that function 0x97 registers, which the game allocates through 4AB9:001C
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00081DDF..0x00085DC4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:03BC..4842:040C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0420..4734:0490
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0852..4734:08DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0B9E..4734:0CD4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45CD:0373..45CD:044C
tool: Capstone 5.0.9 over the load image and the ADV resources; tools/research/exec-census/adv_function_stores.py, direct_callers.py and immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses in `DSUN.EXE` are static; segment immediates in load-image code are relative to 0, so
`0x1000` is added. Offsets in a driver are from the start of its `ADV ` resource in
`RESOURCE.GFF`, whose word at offset 0 points at its function table: pairs of words, a function
number and an entry offset, ending at function `0xFFFF`.

**The dispatch.** `4842:03F4` takes the driver slot from the caller's first argument, and
`4842:03BC` looks up the function number in `AX` in the table that the far pointer at
`CS:0126 + 4 × slot` names (slots below `0x10`), returning the entry as a far pointer, or
`0000:0000` when the slot is empty or the function is absent; `4842:03F4` jumps there through
`retf`, or returns. Neither writes memory. The wrappers at `4842:0CB8`, `0CBE`, `0D30`, `0D5A` and
`0D72` load function numbers `0x96`, `0x97`, `0xB5`, `0xBD` and `0xC1`.

**The driver images.** `4734:0B9E..0CD4` reads the `ADV ` resource numbered by word `+0x38` of
`SOUND.CFG` (the far pointer `DS:3411`) when its argument is 0, or by word `+0x36` when it is 1:
it allocates its size
plus `0x10` bytes through `4AB9:001C`, keeps the far pointer at `DS:3401` or `DS:3405`, rounds it
up to a paragraph through `45CD:0000`, and reads the resource there.

**The census.** `adv_function_stores.py` reads the function table of each of the 17 `ADV `
resources and walks functions `0x67`, `0xB5`, `0xC1` and `0x96` by recursive descent, following
every near transfer with an immediate target. `ADV 10` has no table that ends inside the resource.
Seven drivers (`ADV 3`, 6, 8, 13, 18 and 20, and `ADV 10`) list none of these functions. The other
ten (`ADV 1`, 2, 4, 5, 7, 9, 11, 12, 17 and 19) list all four. In all ten:

- every memory write is to a frame word through `BP`, to the driver's own variables through a `CS`
  prefix, or one of these, through `DS`: `[si + 0x0C]`, `0x0E`, `0x10`, `0x1A`, `0x1E`, `0x20`,
  `0x24`, `0x28`, `0x2A`, `0x30`, `0x32`, `0x36`, `0x38`, `0x3E`, `0x40`, `0x42`, `0x44`, `0x46`,
  `0x48`, `0x4A`, `0x4C` and `0x4E`, and `[bx + si + 0x50]`, `0x52`, `0x60`, `0x68`, `0x78`,
  `0x88`, `0x98`, `0xA8`, `0xB8`, `0x148`, `0x168`, `0x188` and `0x1C8`; and through `ES`,
  `[di + 0x1C]`;
- function `0xC1` writes only frame words and `CS` variables;
- function `0x96` returns a constant without writing memory (`0x208` in `ADV 11`);
- the walk reaches one far call through a `CS` word with `DS` loaded from another `CS` word, the
  pattern of `ADV 11`'s `2FF0` below; `ADV 17` also reaches three far calls through `CS:1425`
  (one at `04A1` with `AX` 0x10) and `ADV 19` an `int 0x65` with `DX` 0x330 (`0599`), which go to
  code outside the driver and were not followed.

**`ADV 11`** (`SBP2FM`, which the installed `SOUND.CFG` names for music). The table gives
`0x67` at `336C`, `0x96` at `37F3`, `0x97` at `3852`, `0xB5` at `3BC7`, `0xBD` at `380A` and
`0xC1` at `3F01`. Function `0x97` takes a free entry of the eight far pointers at `CS:293B + 4n`
(-1 when none) and stores there the far pointer `[bp + 0x0E]` it was given; function `0x98`
(`3998`) clears an entry's segment word, or, when the block's word `+0x1A` is 1, stores 1 at the
block's `+0x1C`. Function `0x67` goes through those entries and loads `DS:SI` from each
non-null one (`33CD`); every other routine on its walk that writes through `DS` loads `DS:SI` from
its own far argument (`2CA6`, `2CF9`, `2DAF`, `2E8F`, `2EF2`, `30D4`, `318C`, `328E`), which its
callers push from the `DS:SI` they hold, or from an entry (`2C44`, `3326`); `ES:DI` at `39BC` is
loaded from an entry (`39B0`). Function `0xB5` loads `DS:SI` from the entry its argument names (`3BDA`);
function `0xC1` writes only `CS` variables and frame words. The routines that load `DS` from `CS:[bx + 0xD45]`
(`22C1`, `23A2`) write only through `CS`.

The far call at `2FF0` goes to the far pointer at `CS:2963` with `DS` set from `CS:2961`. Function
`0xBD` (`380A`) stores the caller's `DS` at `CS:2961` and its far argument at `CS:2963`; `0xBE`
clears the pointer.

**The game's side.** `direct_callers.py` finds one caller of each of `4842:0CB8`, `0CBE` and
`0D5A`, and none of `4842:0D30` other than `45CD:0413`:

- `4734:043A` calls function `0x96` and stores its result in the double word `4E71:0C10`; then
  `4734:0459` and `4734:0483` allocate that many bytes through `4AB9:001C` and store the far
  pointers at `DS:344A` and `DS:3446`. `immediate_search.py` finds the other stores of those words
  to be stores of 0 (`4734:0736`, `4734:075B`).
- `4734:0852` passes its far argument `[bp + 8]` to function `0x97` as the block, with `DS:A3A4` as
  a further far pointer. Its three callers (far calls written as `47B9:0002`, at file offsets
  `0x3C002`, `0x3C0C8` and `0x3D2ED`) pass the far pointers held at `DS:3446`, `DS:3446` and
  `DS:344A`.
- `45CD:037B` calls function `0xBD` with the far pointer `45CD:0390`, with the program's `DS`.
  `45CD:0390` stores its second argument at `4E71:0C29` and adds 1 to `4E71:0C25` when it is
  `0x7F`, stores 1 at `4E71:0C1F` when it is `0x7E`, stores it at `4E71:0C2B` when it lies from
  `0x24` to `0x4C`, and when it lies from 1 to `0x22` calls function `0xB5` and stores 0 at
  `4E71:0C1F`, 1 at `4E71:0C27` and the value's low byte at `4E71:0C28`; for 1 to `0x23` it then
  stores 0 at `DS:A3A4` and `0x7F` at `DS:A423`.

## Interpretation

What the timer's driver function `0067`, and function `0xC1`, write is: their own stack frames,
their driver image (a block from `4AB9:001C`), the sequence blocks at `DS:3446` and `DS:344A`
(also from `4AB9:001C`), and, through the callback `45CD:0390`, `4E71:0C1F..0C2D`, `DS:A3A4` and
`DS:A423`. None of `4E71`'s words or `DS:A3A4`, `DS:A423` is in the clock conversion's windows
(Q-RNG-005, which end at `DS:A4EC`). Where the blocks from `4AB9:001C` lie is FND-RNG-083.

## Alternatives

The walk follows only immediate near transfers, so a jump table inside a driver would hide code;
it found no indirect near jump. The other drivers' `DS` and `ES` loads were listed by the tool and
match `ADV 11`'s pattern, but only `ADV 11`'s were read routine by routine. The calls through
other drivers' `CS` words and `ADV 19`'s `int 0x65` were not followed. The stores' offsets were
not checked against the block size of `0x208` bytes; FND-RNG-083 places every byte a block's
segment reaches above the windows, so the offsets do not decide the question.

## How to reproduce

`python -I adv_function_stores.py <install>/RESOURCE.GFF 67 C1 B5 96` with the evidence
interpreter; disassemble `ADV 11` from `RESOURCE.GFF` offset `0x81DDF` at `336C`, `3852`, `380A`,
`3998`, `3BC7` and `3F01`; disassemble `4842:03BC..040C`, `4734:0420..0490`, `4734:0852..08DB`,
`4734:0B9E..0CD4` and `45CD:0373..044C` from the load image;
`direct_callers.py <dsun> 4734:0852 4842:0CB8 4842:0CBE 4842:0D5A 4842:0D72`;
`immediate_search.py <dsun> 3446`, `3448`, `344A` and `344C`.
