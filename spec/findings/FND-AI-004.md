---
id: FND-AI-004
title: A resident key routine at 28C9:0CFF handles 1 to 6, N, P, Space and the arrow keys, and Space turns computer control off for every unlocked party member
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0CFF..28C9:1261
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00D9
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The routine at `28C9:0CFF` returns at once unless its first argument is 6. From `28C9:0D55` it
compares the word at `[bp+0x12]` with a table of 24 BIOS key words at `28C9:1201` and jumps
through the table of 24 handler offsets that follows at `28C9:1231`:

| Key word | Key | Handler |
|---|---|---|
| `0231`, `0332`, `0433`, `0534` | 1, 2, 3, 4 | `0x0E0C`, `0x0E33`, `0x0E5A`, `0x0E81` |
| `0635`, `0736` | 5, 6 | `0x0EC5`, `0x0ECF` |
| `1950`, `1970` | P, p | `0x1001`, `0x1006` |
| `314E`, `316E` | N, n | `0x0FE3` |
| `3920` | Space | `0x1131` |
| `4300`, `4400` | F9, F10 | `0x0FDB`, `0x0D75` |
| `4700`, `4800`, `4900`, `4B00`, `4D00`, `4F00`, `5000`, `5100` | Home, Up, PgUp, Left, Right, End, Down, PgDn | `0x10D8`, `0x10B6`, `0x10BA`, `0x10D3`, `0x10BF`, `0x10CE`, `0x10C9`, `0x10C4` |
| `6800`, `6900`, `6B00` | Alt+F1, Alt+F2, Alt+F4 | `0x0ED9`, `0x0EFF`, `0x102B` |

A key word not in the table goes to `28C9:1139`; from there, when the word at `57E0:0DAB` is 1,
the routine copies its 24-byte argument block and calls `571F:0020`, stub entry 0 of overlay 190,
whose target is the key routine of FND-COMBAT-025.

- Space (`0x1131`) calls `5702:00D9`, stub entry 37 of overlay 188 (code at file offset
  `0x72EA0`), whose target is the code offset `0x0E60`. That routine visits the slots 0 to 3; for
  each it calls `2D40:3E64` with the slot, and when that returns a byte other than 0 and bit 6 of
  byte `0x18` of the 49-byte record it gives (FMT-COMBAT-001) is clear, it clears bit 5 of that
  byte (`and 0xDF` at offset `0x0EA3`).
- N (`0x0FE3`) does nothing when the word at `4C10:0019` is 0 or the word at `57E0:426D` is 4 or
  more, and otherwise calls `5682:004D`, a stub of overlay 174, with 1. P and p do the same with 0.
- 1 (`0x0E0C`) does nothing when the word at offset 6 of the first 49-byte record is 0 or when
  `28C9:156C` returns a byte other than 0 for the word at `57E0:426D`; otherwise it sets that
  word to 0 and goes on at `28C9:0EA7`. 2 to 4 have handlers of the same size for the next slots.
- 5 and 6 call `56BD:002A` and `56BD:0025`, stubs of overlay 182, with 1.

## Interpretation

Space turns off computer control for each party member whose setting is not locked, as the
manual says (SRC-MANUAL-1994, page 77), at any time the routine runs. N and P pass the next and
the previous target to overlay 174 during a party member's turn in combat. The keys 1 to 4 choose
the character in `57E0:426D`, and this routine takes them before overlay 190 sees them.

## Alternatives

No direct far call to `28C9:0CFF` was found, so which screens pass key events to it is not shown,
and the manual's other keys may be handled elsewhere first. What overlay 174 does with the
target, what `28C9:0EA7` does after a key from 1 to 4, and what the other handlers do was not
read.

## How to reproduce

Disassemble `28C9:0CFF` to `28C9:1201` with the relocations applied, and read the two tables at
`28C9:1201` and `28C9:1231` as words (they start at odd offsets). Read the stub entries of the
header at `5702:0020` onward, five bytes each, and disassemble overlay 188 from `0x72EA0 +
0x0E60`.
