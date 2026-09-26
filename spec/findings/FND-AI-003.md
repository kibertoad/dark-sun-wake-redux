---
id: FND-AI-003
title: The computer-control buttons toggle bit 5 of byte 0x18 of the party member's combatant record unless bit 6 is set, and several screens read bit 5
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0605
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5713:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; Python 3.14.7 byte search
environment: null
---

## Observation

Overlay 190 has its code at file offset `0x78340` and its header at segment `571F`.

The handler at its offset `0x09DA` runs for the buttons `0x2C31 + i` (`BUTN` 11313 to 11316),
with `i` from 0 to 3. It does nothing when the word at offset `0x0E` of record `i` of the 66-byte
records at `57E0:19C5` (FMT-COMBAT-002) is 0. Otherwise, with `r` record `i` of the 49-byte
records at `57E0:19C9` (FMT-COMBAT-001):

1. when bit 6 (`0x40`) of byte `0x18` of `r` is set, it writes `COMPUTER CONTROL IS LOCKED`;
2. otherwise, when bit 5 (`0x20`) is set, it clears it (`and 0xDF` at offset `0x0A34`), gives
   the button frame 5 through `3EBE:071F`, and writes `COMPUTER CONTROL IS OFF`;
3. otherwise it sets bit 5 (`or 0x20` at `0x0A64`), gives the button frame 4, and writes
   `COMPUTER CONTROL IS ON`.

Each message goes to the text box `0x2C06` through the routine at offset `0x06B4`.

The hover text at offsets `0x0504` to `0x05A3` covers the regions 11209 to 11212 for slot
`i = region - 11209`: `INACTIVE CHARACTER` when the word at offset `0x0E` is 0, otherwise
`COMPUTER CONTROL IS ` followed by `LOCKED ` when bit 6 is set, then `ON` when bit 5 is set and
`OFF` when not. The regions 11205 to 11208 give `%Fs IS LEADER` for the slot in `4C13:0369` and
`MAKE %Fs LEADER` for the others, with the slot's name.

A byte search for `test byte es:[bx+0x18], 0x20` finds, besides overlay 190, these readers, each
after indexing the 49-byte records at `57E0:19C9`:

| Place | What follows |
|---|---|
| `DSUN.EXE+0x0001B039` (resident) | For a slot from 0 to 3, a branch on the bit |
| `28C9:0605` | For the combatant in the word at `57E0:44E6`, below 4, a jump past the rest of the routine when the bit is set |
| Overlay 189 offset `0x0192`, overlay 202 offset `0x0026`, overlay 211 offset `0x0026`, overlay 212 offset `0x002B` | When the bit is set, frame 4 on the button `0x2C31` plus the slot |

No instruction found sets bit 6 (`or 0x40`) of the byte.

## Interpretation

Bit 5 of byte `0x18` is the computer-control setting of a party member, and bit 6 locks it. The
buttons 11313 to 11316 beside the character boxes toggle it, and the other screens that show
them draw frame 4 for on. The buttons 11309 to 11312 choose the leader (FND-COMBAT-023).

## Alternatives

What the resident readers do with the bit, and where the game sets bit 6, were not followed; the
lock may be set through a write of the whole byte.

## How to reproduce

Disassemble overlay 190 from its code offset at `0x0470` to `0x05A9` and `0x09DA` to `0x0A95`.
Search `DSUN.EXE` for `26 F6 47 18 20`, `26 80 4F 18 20`, `26 80 67 18 DF` and `26 80 4F 18 40`,
find each match's overlay with the overlay map reporter, and read the instructions before it.
