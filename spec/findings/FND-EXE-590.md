---
id: FND-EXE-590
title: The sound drivers the installed SOUND.CFG selects, ADV resources 11 and 8 of RESOURCE.GFF, are the disc's SBP2FM.ADV and SBPDIG.ADV, whose only transfer out of driver code goes to DSUN.EXE's music callback 4602:0040
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
    file: RESOURCE.GFF
    offset: 0x0007C46A..0x0007D7BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0D5A..4842:0D66
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4602:0021..4602:0030
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4602:0040..4602:00FC
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/installed_adv_resources.py, far_pointer_words.py, callback_args.py, reach_config.py with q007_dsun_driver_census.json, dsun_interrupt_census.py)
environment: null
---

## Observation

**The resources.** The installed `RESOURCE.GFF` has an indexed `ADV ` table of 17 resources,
numbered 1 to 13 and 17 to 20. `installed_adv_resources.py` compares each with the 19 `.ADV` files
in the disc's root directory, every file checked against the build manifest:

| ADV | Offset | Size | Signature | Bytes |
|---|---|---|---|---|
| 1 | `0x6A737` | 14,775 | `Copy` at +2 | equal to `CD:ADLIB.ADV` |
| 2 | `0x6E0EE` | 10,707 | `Copy` at +2 | equal to `CD:MT32MPU.ADV` |
| 3 | `0x70AC1` | 4,361 | `Copy` at +2 | equal to `CD:PASDIG.ADV` |
| 4 | `0x71BCA` | 15,242 | `Copy` at +2 | equal to `CD:PASFM.ADV` |
| 5 | `0x75754` | 8,434 | `Copy` at +2 | equal to `CD:PCSPKR.ADV` |
| 6 | `0x77846` | 4,667 | `Copy` at +2 | equal to `CD:SBDIG.ADV` |
| 7 | `0x78A81` | 14,825 | `Copy` at +2 | equal to `CD:SBFM.ADV` |
| 8 | `0x7C46A` | 4,946 | `Copy` at +2 | equal to `CD:SBPDIG.ADV` |
| 9 | `0x7D7BC` | 15,235 | `Copy` at +2 | equal to `CD:SBP1FM.ADV` |
| 10 | `0x8133F` | 2,720 | `DIGPAK` at +3 | equal to no disc file |
| 11 | `0x81DDF` | 16,357 | `Copy` at +2 | equal to `CD:SBP2FM.ADV` |
| 12 | `0x85DC4` | 16,257 | `Copy` at +2 | equal to `CD:ADLIBG.ADV` |
| 13 | `0x89D45` | 5,770 | `Copy` at +2 | equal to `CD:ALGDIG.ADV` |
| 17 | `0x8B3CF` | 10,816 | `Copy` at +2 | equal to `CD:GF1MIDI.ADV` |
| 18 | `0x8DE0F` | 5,232 | `Copy` at +2 | equal to no disc file; the first 51 bytes match `CD:GF1DIGI.ADV` |
| 19 | `0x8F27F` | 8,792 | `Copy` at +2 | equal to `CD:ARIATSR.ADV` |
| 20 | `0x914D7` | 5,926 | `Copy` at +2 | equal to no disc file |

The signatures are the two DSUN.EXE's driver install routine `4842:0A23` tests (FND-EXE-585). The
installed `SOUND.CFG` names ADV 11 and 8 (FND-CONFIG-003, FND-CONFIG-023). The records of the
installed `SOUND.INI` give `MidiDriverChunkNumber` 0, 1, 2, 4, 5, 7, 9, 11, 12 or 19 and
`DigitalDriverChunkNumber` 0, 1, 3, 6, 8, 10, 13 or 20.

**What the two drivers do.** FND-EXE-492, FND-EXE-496 and FND-EXE-573 decode each of the 19 disc
drivers from every entry of its function table and the handlers it passes: no decoded code requests
program execution. `SBPDIG.ADV` has no far transfer out of its own code. `SBP2FM.ADV` has one, the
music callback at file `0x2FF0` through `CS:2963`, written only by its function `00BD` from the far
pointer argument and cleared by its function `00BE`.

**The callback DSUN.EXE registers.** DSUN.EXE asks for driver function `00BD` only through
`4842:0D5A` (`mov ax, 00BD` and a jump to the driver call routine `4842:03F4`), whose one caller,
listed by `far_pointer_words.py`, with no near call to it in segment `4842`'s frame, is the far call
at `4602:002B`. `callback_args.py` reads the far pointer it passes as `3602:0040`, whose segment word
carries an MZ relocation, so `4602:0040`, an inventory routine. Function `00BE` is asked for only
through `4842:0D60`, from `4734:009D`.

**The walk with the callback.** The query `q007_dsun_driver_census.json` is FND-EXE-588's query with
`4602:0040` as one more start. The walk reaches 2,197 routines and decodes 167,165 instructions in
2,403 ranges. It reaches no interrupt instruction that FND-EXE-588's walk did not and leaves the
same 49 transfers unresolved.

## Interpretation

With the installed `SOUND.CFG`, the driver code DSUN.EXE loads is the disc's `SBP2FM.ADV` and
`SBPDIG.ADV`. Neither requests program execution, and the only way control leaves them is the
music callback, which goes back into DSUN.EXE at `4602:0040`. Both are `Copy` drivers, so the
driver dispatch at `4842:040A` goes to their function table entries, as does the function `0067`
routine `4842:0BBF` registers as a timer callback, and no slot holds the table at `4842:0D7E`: the 13 `DIGPAK` functions, the six `int 66`
sites and the calls at `4842:13FB` and `4842:14EA` in them do not run. So in the shipped
configuration, nothing the game runs requests program execution.

A player who runs the setup program can pick a `SOUND.INI` record that names ADV 10, the only
`DIGPAK` image, or ADV 20; neither matches a disc driver, and they are not read here (Q-EXE-028).
ADV 18 is named by no `SOUND.INI` record.

## Alternatives

- A `SOUND.CFG` changed by hand, or a GFF file other than `RESOURCE.GFF` holding an `ADV ` table
  that the resource system opens first, could load other code; neither ships.
- The driver census rests on FND-EXE-496's decode from the function table entries and the handlers
  it lists; a transfer only reachable from elsewhere in a driver is not covered (FND-EXE-573
  classifies every `CD` byte in the drivers as the control).

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, in
`tools/research/exec-census/`: run
`installed_adv_resources.py <install dir> ../../../spec/builds/BLD-GOG-EN-1.1.files.yaml`, which
prints the table and the `SOUND.INI` values. `far_pointer_words.py <dsun> 4842:0D5A 4842:0D60` lists
the far callers, and `callback_args.py <dsun> 00BD 8 3B24B` reads the callback passed at
`4602:002B`. Then run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_driver_census.json`,
`python -I -m scientific_method_engine reach <config> > <report>` and
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
