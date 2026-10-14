---
id: FND-EXE-585
title: DSUN.EXE's timer callbacks and sound driver function calls go to routines in the file or to driver code loaded from ADV resources, and the driver dispatch's far return is a transfer the walk does not report
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0445..4842:049D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:077F..4842:084A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:03BC..4842:040C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0A23..4842:0B06
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0B1A..4842:0B27
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0B79..4842:0C2B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:13DA..4842:1500
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:029A..4734:02DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:07FD..4734:080B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49E9:019B..49E9:01C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47E5:0074..47E5:00ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00083387..0x00083393
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000894A9..0x000894B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0003E39E..0x0003E3D6
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_timer_census.json, dsun_interrupt_census.py, far_pointer_words.py, trampoline_target.py); Capstone 5.0.9 for the store and return searches
environment: null
---

## Observation

Addresses, regions and the walk are those of FND-EXE-584, whose walk leaves 59 computed transfers
unresolved. One of them is the far call `call far cs:[si+6]` at `4842:0498` in the timer interrupt
handler of segment `4842` (FND-EXE-575, FND-TIME-007). It runs for each of the 17 slots whose state
word at `CS:006C + 2 * slot` is 2 (`4842:0445`). The words at `CS:0006 + 4 * slot`, the callbacks,
are written only at `4842:07B7` and `4842:07BC`, the hits of a search of every byte offset of
segment `4842` for a store through CS to displacement 6 or 8. Both are in the routine at
`4842:077F`, which takes the first slot whose state is 0, sets it to 1, and stores its far argument
there. That routine has five callers, the hits of a search of the file's words equal to `077F` and
of the near calls in segment `4842`'s 64 KiB frame:

| Caller | Callback passed |
|---|---|
| `4734:0806` | `4654:09A9` (the segment word `3654` carries an MZ relocation) |
| overlay 196 `+041E` | `0620:003E`, an overlay fixup word for descriptor 196 and its trampoline 6, whose target is `+040B` |
| overlay 200 `+0350` | `00F0:31A7`, an overlay fixup word for descriptor 30 (segment `31E0`), so `31E0:31A7` |
| `4842:0BBF` | the far pointer `4842:03BC` returns for function `0067` of a driver slot, when it is not `0000:0000` |
| `4842:1483` | `0000:0000` |

`4654:09A9`, `196+040B` and `31E0:31A7` each start with `push bp`, `mov bp, sp`. The null callback's
slot number is kept at `CS:0E78`, whose only other use, at `4842:14F0`, passes it to the release
routine at `4842:084A`, so the code that holds that number never starts the slot.

`4842:03BC` takes a driver slot below 16, loads the far pointer at `CS:0126 + 4 * slot`, and scans
its four-byte pairs for a word equal to AX, stopping at `FFFF`; on a match it returns the pair's
second word with the table's segment, otherwise `0000:0000`. The routine at `4842:03F4` calls it and,
when the result is not `0000:0000`, pushes it and runs a far return at `4842:040A`, which jumps to
that address. The slot pointers are written by the install routine at `4842:0A23` and cleared at
`4842:0B1A..0B27`, the hits of the same search for displacements `0126` and `0128`. `4842:0A23` takes
a far pointer to an image in memory. When the words at `+3` spell `DIGPAK`, it keeps the image's
address at `CS:0E26` and puts `4842:0D7E` in the slot; when the words at `+2` and `+4` spell `Copy`,
it puts the image address plus the image's first word in the slot. The table at `4842:0D7E` holds 13
pairs before its `FFFF`: functions `0064`, `0065`, `0066`, `0068`, `0078`, `0079`, `007A`, `007B`,
`007C`, `007D`, `007E`, `0085` and `0086`, at `4842:13DA`, `145A`, `1468`, `14CD`, `1500`, `1626`,
`1685`, `169A`, `18F7`, `186B`, `18B3`, `1740` and `182E`, each an inventory row. It has no pair for
`0067`.

`4842:0A23` has two callers, `4734:02D7` and `49E9:01C3`. Each passes the far pointer `47E5:0074`
returns for its argument, 0 from `4734` and 1 from `49E9`. `47E5:0074` takes the word at `+0x38` (for
0) or `+0x36` (for 1) of the loaded `SOUND.CFG` and passes it with the tag `ADV ` to the resource
query at `38FF:05B5` (FND-CONFIG-023), so the image is an `ADV ` resource.

**Far returns that jump.** The walk's report lists no unresolved transfer at `4842:040A`: it reads
the far return as the routine's return, and the 13 functions of the table at `4842:0D7E` are not
reached. Of the 1,437 near and far returns in the ranges FND-EXE-584's walk decodes, `4842:040A` is the only
one whose previous instruction is a push.

**The walk with these targets.** The query `q007_dsun_timer_census.json` is FND-EXE-584's query with
the 13 functions as starts and a declaration for `4842:0498` naming `4654:09A9`, `196+040B` and
`31E0:31A7`, not exhaustive, so the walk reads those targets and keeps the call unresolved. The walk
reaches 1,481 routines and decodes 109,604 instructions in 1,598 ranges. Every interrupt 21 and 2F
site keeps the reading of FND-EXE-584. It newly reaches six `int 66` instructions, at `4842:0FE2`,
`103E`, `1058`, `13D1`, `1403` and `18DE`; of the file's other two `CD 66` pairs, `4842:0EAD` is not
reached and `3BA6:11B5` lies inside a `jmp`. It leaves 61 transfers unresolved: the 59, with
`4842:0498` kept, plus `call far cs:[0E1C]` at `4842:13FB` and `4842:14EA`. Those two build their
target from `CS:0E26`, the `DIGPAK` image's address: the image's bytes at `+0x100` for `4842:13FB`
and `+0x103` for `4842:14EA`. In this walk too, `4842:040A` is the only return directly after a push.

## Interpretation

The timer interrupt calls routines in the file, which three callers register, and the function
`0067` routine of a `Copy` driver loaded from an `ADV ` resource, which `4842:0BBF` registers. The
driver dispatch at `4842:040A` jumps to the 13 routines in segment `4842` for a `DIGPAK` driver, or
to the function routines of a `Copy` driver. Those 13 routines call into the `DIGPAK` image and
issue interrupt 66, whose handler the program does not install. So `4842:0498`, `4842:040A`,
`4842:13FB`, `4842:14EA` and the six interrupt 66 sites can leave the file, all into code loaded
from an `ADV ` resource or into whatever handles interrupt 66.

## Alternatives

- The return search finds a return directly after a push. A return reached after the stack is
  rewritten some other way, such as a store into the return address, is not excluded.
- That the null slot is never started rests on the uses of `CS:0E78`; a start routine called with
  that slot number taken from elsewhere is not excluded, and would call `0000:0000`.
- The store searches find stores through CS with those displacements. A block copy into the slots
  or the driver table pointers is not excluded.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_timer_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
`far_pointer_words.py <dsun> 4842:077F 4842:0A23` lists the far callers, and
`trampoline_target.py <dsun> 5755:003E` resolves overlay 196's pointer. The store searches decode an
instruction at every byte offset of file `0x3D622..0x3EF26` (segment `4842`'s region) and keep stores
through CS to displacements 6, 8, `0126` and `0128`. The return search decodes each of the
report's decoded ranges, for this walk and FND-EXE-584's, and keeps each `ret` or `retf` whose previous instruction is a `push`.
