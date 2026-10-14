---
id: FND-EXE-575
title: The installed DSUN.EXE never reads or calls through the interrupt 21 vector; the vectors it reads are 0, 4, 5, 6, 8, 9, 19, 23, 3F and 67, and it calls the saved handlers of 5, 8, 9 and 19 outside the file
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:01B0..1000:01DD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0494..1000:04A3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2CEA..1000:2CF3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2D51..1000:2D63
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2A23..1000:2A28
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0029..44B6:0042
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:00B5..44B6:00BD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:0482..44DE:048E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0519..4842:053E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:1103..4AE5:112D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 55E3:003F..55E3:0044
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0DE6..4AE5:0DEB
tool: Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/far_pointer_words.py, store_values.py)
environment: null
---

## Observation

Addresses and regions are those of FND-EXE-574. The standard counts a far call through a saved
copy of the interrupt 21 vector, after a `pushf`, as a site of the program-execution mechanism, so
this finding lists every place the program reads an interrupt vector and every such call.

**Vectors read through DOS (AH=35).** FND-EXE-574 lists 8 reached AH=35 sites and no unreached one.
Their vector numbers:

- `1000:01B4`, `01C1`, `01CE` and `01DB` in the startup code load AX with 3500, 3504, 3505 and
  3506: vectors 0, 4, 5 and 6.
- `1000:049C`, in the routine `1000:0494` that takes the vector number as its argument, has two
  callers, both near calls, which push 23 (`1000:2CF0`) and 5 (`1000:2D57`). The second caller stores
  the result at `DS:A4DE..A4E2`.
- `44DE:048C`, in `44DE:0482`, which also takes the number as its argument, has one caller, the far
  call at `44B6:0032`, which writes 9 into the argument slot. The caller stores the result at
  `CS:000C..0010` of its segment.
- `4AE5:014E` reads the vector numbered by byte `[0111]` of the overlay manager's data, `0x3F`
  (FND-EXE-562).
- `4AE5:0DE9` loads AX with 3567: vector 67.

`1000:0494` and `44DE:0482` have no other caller: a decode of `E8` and `E9` at every
byte offset of the 64 KiB frame of each routine's segment, and the words of the file equal to each
routine's offset, find only the calls above (no word is a relocated far call, pointer, fixup or
immediate beside a segment word apart from them).

**Vectors read from the vector table.** Decoding every byte offset of the regions for an operand
with no base or index register at displacement `0084` or `0086`, the interrupt 21 slot, finds 30
decodes, 22 once the eight that start one byte into a segment-override prefix are dropped. None
goes through segment 0: the segment register each uses is loaded within the 80 bytes before it
with `0040` (`1000:1239`, `205+007E`), the relocated overlay data segment `0368` (six pushes in
overlays 183 and 184), or a module's data segment taken from `CS:0005` (`4AE5:01A9`, `03CB`,
`054C`, `08DC`) or `CS:2875` (`1000:2981`, `2987`), or not loaded in those bytes at all (the
startup's two pushes of `DS:0084..0088`, `1000:11B5`, `15F3:073F`, `15F3:0743`, `4AE5:09B9`, and
`4AE5:09DB` and `4AE5:0BDF` after a `pop ds`). Two routines read other slots directly with a segment register set to 0:
`4842:0520` takes the interrupt 8 vector from `0000:0020` into `CS:011A..011E` (`mov ax, 0` at
`4842:0525` carries no relocation), and `4AE5:1103` takes the interrupt 19 vector from `0000:0064`
into `55E3:002F..0033` and writes `55E3:003F` in its place.

**Calls through a saved vector.** Decoding every byte offset of the regions for a `pushf` followed
within three instructions by a far call through memory finds five starts. `1000:03EA` lies inside
the near call at `1000:03E9`. The other four call saved handlers:

| Site | Call | Saved vector |
|---|---|---|
| `1000:2A23` | `call far [A4DE]` | 5, stored by the caller at `1000:2D57` |
| `44B6:00B5`, `44B6:00B6` | `call far cs:[000C]` after one or two `pushf` | 9 |
| `4842:0519` | `call far cs:[011A]` | 8 |

The interrupt 19 handler `55E3:003F` is `jmp far cs:[002F]`, the saved vector 19.

## Interpretation

No code in the file reads the interrupt 21 vector, through DOS or from the vector table, so no
call through a saved copy of it can be a site FND-EXE-574's census missed. The vectors the program
reads are 0, 4, 5, 6, 8, 9, 19, 23, 3F and 67. Four of the saved handlers are called, or jumped to,
from the program's own handlers: the print-screen and BOUND handler 5, the timer 8, the keyboard 9
and the bootstrap 19. These are transfers out of the file to whatever handler was installed before
the game started, the BIOS's or a resident program's, which the census does not cover.

## Alternatives

- A vector-table access through a base or index register holding `0x84` or `0x86`, or a block
  copy of the vector table, is outside the displacement search; so is a far pointer to the
  interrupt 21 handler that DOS or another program hands over other than through AH=35.
- Where a routine does not load the segment register itself, the access goes through the
  segment its callers leave there. The search does not follow callers, so a caller that sets DS
  or ES to 0 before one of those routines is not excluded.
- `44B6:0032` writes the vector number into the argument slot through `BP`, after pushing AX as
  that slot, so the argument is the 9 written there, not the pushed AX.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python: the AH=35 sites come from
FND-EXE-574's census output. In `tools/research/exec-census/`,
`far_pointer_words.py <dsun> 1000:0494 44DE:0482 44DE:046D 55E3:003F` lists the words equal to
those offsets, and `store_values.py <dsun> 11a 11c` the stores to the timer's saved vector. The
near callers were found by decoding `E8` and `E9` at every byte offset of the routine's 64 KiB
frame. The displacement and `pushf` searches decode an instruction at every byte offset of each
region FND-EXE-574 lists, keep a memory operand with no base or index register at displacement
`0x84` or `0x86`, or a `pushf` followed within three instructions by `lcall`, `ljmp` or a far
`call` through memory, and read the instructions within 80 bytes before each hit for the last
load of the segment register it uses.
