---
id: FND-COMBAT-024
title: The combat messages and labels are strings of the data segment, each pushed at one to four places in the panel routine and overlays 182, 190 and 204
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0F4D..57E0:106A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:1CCD..57E0:1ED8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:28DA..57E0:28F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0000
tool: Python 3.14.7 byte search; Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Each string below is a NUL-terminated string of the data segment `57E0`. A search of `DSUN.EXE`
for `push imm16` (`68` and the offset) finds the places listed, and at each the disassembly
decodes a `push` of that offset. Overlay 182 has code at file offset `0x68850` (header segment
`56BD`), overlay 190 at `0x78340` (`571F`) and overlay 204 at `0x8BDC0` (`5787`).

| Address | String | Pushed at |
|---|---|---|
| `57E0:0F4D` | `???/???` | `2C5F:04EB` |
| `57E0:0F55` | `%d/%d` | `2C5F:051D` |
| `57E0:0F5B` | `Okay` | `2C5F:0595` |
| `57E0:0F60` | `Move : %d` | `2C5F:060C` |
| `57E0:103A` | `END %Fs's MOVE` | overlay 182, `0x1C45` |
| `57E0:1049` | `END MOVE` | overlay 182, `0x1C65` |
| `57E0:1052` | `GUARD` | overlay 182, `0x1CBA` |
| `57E0:1058` | `WAIT` | overlay 182, `0x1CB6` |
| `57E0:105D` | `END TURN` | overlay 182, `0x1CB2` |
| `57E0:1CCD` | `PRESS RIGHT MOUSE BUTTON` | overlay 190, `0x048E` and `0x0697` |
| `57E0:1CE3` | `ON` | overlay 190, `0x0570` |
| `57E0:1CE6` | `%Fs IS LEADER` | overlay 190, `0x04D5` and `0x09B3` |
| `57E0:1CF4` | `MAKE %Fs LEADER` | overlay 190, `0x04EE` |
| `57E0:1D04` | `INACTIVE CHARACTER` | overlay 190, `0x0590` and `0x0E6E` |
| `57E0:1D17` | `COMPUTER CONTROL IS ` | overlay 190, `0x0526` |
| `57E0:1D2C` | `LOCKED ` | overlay 190, `0x054B` |
| `57E0:1D34` | `OFF` | overlay 190, `0x0578` |
| `57E0:1D38` | `CAN'T CHANGE LEADER IN COMBAT` | overlay 190, `0x0905` |
| `57E0:1D56` | `COMPUTER CONTROL IS LOCKED` | overlay 190, `0x0A0A` |
| `57E0:1D71` | `COMPUTER CONTROL IS OFF` | overlay 190, `0x0A50` |
| `57E0:1D89` | `COMPUTER CONTROL IS ON` | overlay 190, `0x0A80` |
| `57E0:1DA0` | `CAN'T ADD CHARS IN COMBAT` | overlay 190, `0x0E51` |
| `57E0:1DBA` | `NEW` | overlay 190, `0x0E68` |
| `57E0:1DBE` | `ADD` | overlay 190, `0x0E64` |
| `57E0:1DC2` | `CANCEL` | overlay 190, `0x0E60`, `0x0E95`, `0x10E9` and `0x19CB` |
| `57E0:1E42` | `R:%u %u,%u M:%u,%u` | overlay 190, `0x14D2` |
| `57E0:1EAA` | `%Fs GUARDS` | overlay 190, `0x16F6` |
| `57E0:1EB5` | `%Fs WAITS` | overlay 190, `0x1792` |
| `57E0:1EBF` | `CAN'T SAVE DURING COMBAT` | overlay 190, `0x17CB` |
| `57E0:28DA` | `NO RESTING DURING COMBAT` | overlay 204, `0x0CDF` |

`%Fs` is the far-string conversion of the formatter at `3150:000E`. Every caller of a pattern
with `%Fs` passes it the name at offset `0x21` of a 49-byte record (FMT-COMBAT-001).

## Interpretation

The combat messages, the end-of-move menu (FND-COMBAT-026), the combat keys (FND-COMBAT-025) and
the party buttons' hover texts are all driven from these few routines. The strings that end in
`COMBAT` are whole messages, so `COMBAT` is never a label of its own (FND-COMBAT-003).

## Alternatives

A push of the offset can also be data that decodes as `68`; each match was checked as an
instruction. A string used through a pointer held elsewhere would not be found this way.

## How to reproduce

Convert each address to a file offset (`f = x + 0x57E00 - 0x10000 + 0x5200`), read the string,
and search the file for `68` followed by the offset, little-endian. Find each match's overlay
with the overlay map reporter and disassemble around it.
