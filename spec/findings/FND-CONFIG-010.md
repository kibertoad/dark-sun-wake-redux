---
id: FND-CONFIG-010
title: Preferences button dispatch changes sound, animation, difficulty and message-delay state
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:0000
tool: Ghidra 12.1.3 mapped-overlay MZ import with ReportDecompileWindow, ReportReferences and ReportInstructionContext; Capstone 5.0.7 16-bit disassembly of bounded shipped-file ranges
environment: null
---

## Observation

Overlay 203's dispatcher at `DSUN.EXE+0x0008B32E` receives a control number
and, for one input-event value, compares it against 13 button numbers in a
table beginning at `DSUN.EXE+0x0008B735`. A parallel table gives one branch
for each. The relevant branches, relative to the overlay's code start
`DSUN.EXE+0x0008B240`, act as follows:

| Button | Branch | Direct state change |
|---|---|---|
| `16300` | `0x01CC` | Passes the inverse of byte `DS:1436` with the current `DS:1435` to a routine that writes both bytes. It then updates the music button and bar. |
| `16301` | `0x035F` | Passes the inverse of byte `DS:1435` with the current `DS:1436` to the same routine. It then updates the sound-effects button and bar. |
| `16303` | `0x0491` | When `DS:1438` is zero, changes `DS:1437` from zero to one or nonzero to zero, passes one of two four-word groups to another routine, and updates button `16303`. |
| `16304` | `0x018E` | Adds eight to word `DS:26B7`. |
| `16305` | `0x0176` | Decreases `DS:26B7` by eight when its previous value exceeds 28; otherwise sets it to 20. |
| `16306` | `0x029E` | Adds seven to byte `DS:26B5`, then caps values above 127 at 127. If sound effects are off, calls the two-byte state routine with effects on. |
| `16307` | `0x0280` | Decreases `DS:26B5` by seven when its previous value exceeds seven; otherwise sets it to zero. At zero it calls the two-byte state routine with effects off. |
| `16308` | `0x03D0` | Adds one to word `DS:143A`, then clamps it to 0 through 3 with signed comparisons. |
| `16309` | `0x03D5` | Subtracts one from `DS:143A` and applies the same clamp. |
| `16310` | `0x01B9` | Changes byte `DS:14E4` from zero to one or nonzero to zero and updates button `16310`. |

The two-byte routine used by `16300` and `16301` writes `DS:1436` and
`DS:1435` at `DSUN.EXE+0x0008B7AC`; it also calls sound-library routines when
the master sound byte permits it. The dispatcher initializes the sound-effect
volume step to seven and the message-delay step to eight. The first two bytes
are saved at `PREF/100` offsets `0x06` and `0x05`, the sound-effect volume at
offset `0x03`, animation state at `0x07`, and difficulty at `0x00`
(FND-SAVE-004, FND-SAVE-005, FND-CONFIG-009).

In the loaded image `DS:26B7` begins at 50, and it is not among the nine
bytes copied into `PREF/100`. A separate routine in overlay 172 reads the
word, multiplies it by 100 using 16-bit arithmetic, and passes the result to
the millisecond wait at `1000:12FA` (`DSUN.EXE+0x00059B62`, FND-TIME-004).

A physical search for the two-byte little-endian displacement `0x26B7` in
the shipped executable finds six occurrences: `0x59B63` in the overlay 172
read, `0x8B3BD`, `0x8B3C8`, `0x8B3D0` and `0x8B3D3` in the Preferences click
branches, and `0x8B867` in its bar redraw. This bounds literal references;
it does not rule out indirect writes or initialization through a copied block.

The renderer reads `DS:14E4` when drawing voice button `16310`. The speech
player also tests `DS:14E4` and separately tests `DS:1439`, the byte saved at
`PREF/100` offset `0x08` (FND-SOUND-008). The click branch for `16310` changes
`DS:14E4`, not `DS:1439`.

## Interpretation

`PREF/100` offset `0x03` is the sound-effect volume, offset `0x05` the
sound-effects enable state and offset `0x06` the music enable state. The
Preferences voice button uses an additional runtime byte distinct from the
saved speech gate. The first arrow pair changes the word whose hover label is
`MESSAGE DELAY` (FND-UI-034); it does not change the saved music-volume byte.
The difficulty arrows stop at 0 and 3 instead of wrapping for ordinary values.

## Alternatives

The event reaching the wait and its effect on text pacing remain open. The
code that initializes
`DS:14E4` and `DS:1439`, and any path that synchronizes them, was not read.
The sound-library calls and the click event's precise native input timing also
remain open. The volume addition occurs in an eight-bit byte before its upper
cap, so a malformed saved value above 248 can wrap below the cap; ordinary
0-to-127 values follow the listed seven-unit step and cap.

## How to reproduce

Use `ReportFbovOverlayMap.ps1` to locate overlay 203 and overlay 172 in the
approved `DSUN.EXE`. In overlay 203, read the bounded dispatcher at file
offset `0x0008B32E`, its 13-entry control and branch tables at `0x0008B735`,
and only the listed branch targets relative to `0x0008B240`. Inspect the
two-byte state routine at `0x0008B7AC`. In the mapped import, report references
to `DS:26B7`, `DS:14E4` and `DS:1439`, and inspect bounded instruction context
at `648A:00C2` for the word-times-100 consumer. Compare the save/load copies
in FND-SAVE-004 and FND-SAVE-005. Search the shipped file for the bounded
two-byte pattern `B7 26` and inspect each of its six instruction contexts.
