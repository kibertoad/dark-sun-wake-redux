---
id: FND-SAVE-012
title: Clearing DS 1462 makes the resident main loop return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024..277B:05A1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:1462
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0158..1000:0400
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident routine; physical displacement search
environment: null
---

## Observation

The byte at `57E0:1462` is 1 in the approved `DSUN.EXE` load image.
A physical search for its two-byte displacement finds 18 occurrences:
one resident read at file offset `0x0001CF43` and 17 stores of zero in
resident or overlay code. The stores include the `F3` exit-choice Quit
branch at `0x00079D7C` (FND-SAVE-010) and the Start Game window's Exit
to DOS branch at `0x0008144D` (FND-UI-035).

The read is in the resident routine starting at `277B:0024` (file offset
`0x0001C9D4`). Near its end, after several calls and an optional state
branch, it tests `57E0:1462`. When the byte is nonzero, control jumps
back to the routine's work cycle at file offset `0x0001CCE0`; when it is
zero, the routine sets its return word to zero and returns far at
`0x0001CF50`.

The executable's startup path calls the resident routine at file offset
`0x00005358`, pushes its return word, and calls the cleanup entry at
`0x000055DF`. That entry reaches `0x00005588`, which runs cleanup calls
and passes the return word to `0x0000539E`. The latter sets `AH` to
`0x4C` and invokes DOS `INT 21h` with the return word's low byte in
`AL`. Some cleanup callees are indirect and have not been resolved.

## Interpretation

The exit-control byte is the continuation flag for this resident main
loop. The Quit choice's zero store causes the loop to return when it
next reaches this guard. The startup path then performs cleanup and
submits a DOS terminate-process request. This is the executable's
static exit path for the `F3` Quit branch.

## Alternatives

The indirect cleanup callees and the result of DOS `INT 21h` were not
read or observed, so this finding does not establish every cleanup
effect or the operating-system outcome. The displacement search covers direct
references in the physical executable; an indirect alias remains
possible. The other zero stores have their own entry conditions and do
not by themselves identify additional player-facing quit commands.

This replaces FND-SAVE-011, whose locations ended at `277B:05A0` and `1000:03FF`, the first bytes of
the closing `retf` instructions of the routines they cover, instead of the byte after them, and so
did the same ranges where the text repeats them. The documentation check found this when the
reconciled `DSUN.EXE` inventory placed functions whose last bytes are those returns. The ends now
give the byte after each return. Its other observations are unchanged.

## How to reproduce

Read byte `57E0:1462` at file offset `0x0004E462` in the approved build.
Search the physical executable for displacement bytes `62 14`, then
classify each bounded instruction site. Disassemble resident
`277B:0024..277B:05A1`, especially file offsets
`0x0001CCE0..0x0001CCFC` and `0x0001CF29..0x0001CF51`. Compare the
zero stores in FND-SAVE-010 and FND-UI-035. Follow the direct raw far
call to `177B:0024` at `0x00005358` through cleanup entries
`0x000055DF` and `0x00005588` to the DOS interrupt at
`0x0000539E`.
