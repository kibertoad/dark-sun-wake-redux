---
id: FND-SCRIPT-013
title: Nine far calls run scripts through 172C:000C, and two of them run MAS resources from offset 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:000B..1695:0103
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:0407..1695:040C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:0546..1695:054B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:06D0..1695:06D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:0790..1695:07B8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0512..277B:052E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0F4A..2D40:0F4F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:205A..2D40:205F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:218A..2D40:218F
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

A search of `DSUN.EXE` for a far call to `172C:000C` with the relocated segment finds nine, at
file offsets `0xBC49`, `0xBF57`, `0xC096`, `0xC220`, `0xC303`, `0x1CED9`, `0x2354A`, `0x2465A` and
`0x2478A`: `1695:00F9`, `1695:0407`, `1695:0546`, `1695:06D0`, `1695:07B3`, `277B:0529`,
`2D40:0F4A`, `2D40:205A` and `2D40:218A`. None is in overlay code.

- `277B:0529` passes script 99, start 0 and selector 2. It follows the copy of `DARKSAVE.GFF` to
  `DARKRUN.GFF` at `277B:04B0` (FND-SAVE-006), whose failure message, `57E0:0739`, is passed to
  `56B2:0039` just before the call.
- `1695:00F9` is in the far routine `1695:000B(n)`. That routine stores n at `4C10:0021` and
  returns unless the word at `4C0E:000B` is 2. It then calls `1695:07DD` on each of the list
  heads `4C10:0005`, `0007`, `0009` and `000B` until the head is `0xFFFF`, calls `1695:010F` on
  the heads `4C10:000D`, `0011`, `0013`, `0015` and `000F`, calls `1695:0170` on `4C10:0017`, and
  runs script n from start 0 with selector 2.
- `1695:07B3` and the calls at `1695:0407`, `0546` and `06D0` pass the word at offset 0 of a
  13-byte record, found through the far pointer at `57E0:40C0` as `index * 13`, as the start, the
  word at offset 2 as the script number, and selector 1 (FND-SCRIPT-014).
- The three calls in `2D40` pass word pairs of a 19-byte record with selector 1
  (FND-SCRIPT-016).

An earlier Ghidra reading counted eight direct callers, with the call at `1695:00F9` missing, and
found the call in `277B` pushing two literal words.

## Interpretation

Selector 2 runs a `MAS ` resource and selector 1 a `GPL ` resource (FND-SCRIPT-008). `MAS `
resource 99 runs when the game has made its working copy of the save archive, which the
start-up code in segment `277B` does (FND-SCRIPT-014).
`1695:000B` changes the current value at `4C10:0021`, empties four lists of script triggers,
prunes six others, and runs the `MAS ` resource with that number; the `MAS ` numbers 50 to 69
(FND-SCRIPT-001) fit this being the change of region, with n the region number. The other seven
calls run a `GPL ` script from an entry point stored in a trigger record.

## Alternatives

That `1695:000B` runs on a region change is a reading of its shape; its callers were not traced.
`56B2:0039` is in overlay 180 and was not read. That the call at `277B:0529` belongs to the
start-up routine `277B:0024` was not checked.

## How to reproduce

Search `DSUN.EXE` for `9A 0C 00 2C 17` and disassemble the context of each hit, and
`1695:000B` to `1695:0103`.
