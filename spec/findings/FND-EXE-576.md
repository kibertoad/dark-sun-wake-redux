---
id: FND-EXE-576
title: Earlier findings locate a site of each kind that the DSUN.EXE program-execution census searched for, and the census finds each with the value those findings give
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:019E..1000:01A7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07B0..1000:07CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:00BC..15F3:00DA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1E8C..1000:1EAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:0630..2660:0641
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0140..4AE5:0150
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:00B5..44B6:00BD
tool: Capstone 5.0.9 and xxhash 4.0.1, with the outputs of FND-EXE-574 and FND-EXE-575
environment: null
---

## Observation

FND-EXE-574 and FND-EXE-575 search for six kinds of site. For each kind, an earlier finding that
did not come from those searches locates a site, and the searches find it with the same value:

| Kind of site | Located by | Found by the census as |
|---|---|---|
| `int 21` instruction with its AH | FND-EXE-551: the termination request at `1000:019E`, AH 4C | `1000:01A5`, reached, AH 4C |
| `int 21` instruction with its AH | FND-EXE-502: the positioning request in `1000:07B0`, AH 42 | `1000:07CC`, reached, AH 42 |
| `int 2F` instruction with its AX | FND-EXE-394: the installation probe in `15F3:00BC`, AX 4300, then AX 4310 | `15F3:00CC` AX 4300 and `15F3:00D8` AX 4310, reached |
| call of a routine that builds an interrupt on the stack, with the interrupt and AX it passes | FND-SOUND-013: `2660:0614` passes interrupt 2F with AX 1500 to `1000:1E8C`, which writes `55 CD n 5D CB` on the stack | the far call at `2660:063C`, interrupt 2F, AX 1500 |
| near caller of a routine (the search behind the argument-selected site and the stack routines) | FND-SOUND-013: `1000:1E8C` calls the stack routine | the near call at `1000:1EAA` to `1000:1EB4` |
| AH=35 site with its vector | FND-EXE-562: `4AE5:0140` reads the vector numbered by byte `[0111]`, `0x3F` | `4AE5:014E`, vector 3F |
| `pushf` and a far call through a saved vector | FND-INPUT-005: the keyboard handler `44B6:00B5` first calls the saved vector of interrupt 9 | `44B6:00B5`, `call far cs:[000C]`, vector 9 |

FND-EXE-574's walk also decodes its two instruction controls, `277B:0024` and `188+1604`, and its
own list of decoded interrupt instructions matches the byte search for `CD 21` and `CD 2F`.

## Interpretation

Each kind of site the census looks for counts as searched: interrupt 21 and interrupt 2F
instructions with their selecting values, the callers of the routines that build an interrupt on
the stack with the values they pass, the near and far caller searches behind them, the vectors the
program reads and the calls through saved vectors. None of the controls was seen under a debugger;
each rests on a static reading recorded before the census. No kind of site is left without a
control.

## Alternatives

- A control from a static reading shows that the search finds a site that a reading located by
  other means, not that the program runs that site. A run of the original under a debugger that
  stops at one of these sites would add a control of the other kind the standard names.
- The interrupt 2E search has no positive control: the file holds no `CD 2E` pair, and no earlier
  finding locates an interrupt 2E site. The byte search for it is the same search that finds every
  `CD 21` and `CD 2F` control above.

## How to reproduce

Compare the census output of FND-EXE-574 (`dsun_interrupt_census.py` at the commit that adds that
finding) and the searches of FND-EXE-575 with the locations and values the cited findings give.
