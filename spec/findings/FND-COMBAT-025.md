---
id: FND-COMBAT-025
title: A key dispatcher in overlay 190 handles G, W and Q for the character in 57E0:426D during combat, and has no case for N or the manual's P
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
    address: 57E0:426D..57E0:426F
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Overlay 190 has its code at file offset `0x78340` and its header at segment `571F`. The routine
at its offset `0x139B` (`DSUN.EXE+0x000796DB`) compares the word at `[bp+0x12]` with a table of
33 words at offset `0x1F44` and jumps through the table of 33 handler offsets that follows it at
`0x1F86`. The words are BIOS keyboard words, scancode in the high byte and character in the low:

| Key word | Key | Handler |
|---|---|---|
| `011B` | Esc | `0x1953` |
| `0231`, `0332`, `0433`, `0534` | 1, 2, 3, 4 | `0x15B5` |
| `0D3D` | = | `0x1562` |
| `0F09` | Tab | `0x15A3` |
| `1051` | Q | `0x18FE` |
| `1071` | q | `0x18F4` |
| `1157`, `1177` | W, w | `0x1763` |
| `1454`, `1474` | T, t | `0x13E2` |
| `1E41`, `1E61` | A, a | `0x1894` |
| `1F53`, `1F73` | S, s | `0x13C6` |
| `2247`, `2267` | G, g | `0x16C7` |
| `2348`, `2368` | H, h | `0x1718` |
| `2D00` | Alt-X | `0x1953` |
| `2E03` | Ctrl-C | `0x1953` |
| `324D`, `326D` | M, m | `0x1454` |
| `353F` | ? | `0x1470` |
| `3920` | Space | `0x15B5` |
| `3B00` | F1 | `0x17BD` |
| `3C00` | F2 | `0x1807` |
| `3D00` | F3 | `0x1953` |
| `3E00` | F4 | `0x183C` |
| `3F00` | F5 | `0x1868` |
| `4000` | F6 | `0x1894` |

A key word not in the table goes to offset `0x1A66`, which sets bit 5 of the low byte (lower
case), subtracts `0x63` and, for `c` to `v`, jumps through a table of 20 words at `0x1F0C`: `c`,
`e`, `i`, `u` and `v` go to `0x1A8E`, `o` and `p` to `0x1A82`, and every other letter, `n`
included, to the routine's end at `0x1EC9`, which does nothing more.

The combat handlers, where `c` is the word at `57E0:426D` and `combat_state` the word at
`4C10:0019` (FND-COMBAT-023):

- G (`0x16C7`): nothing when `combat_state` is 0 or `c` is 4 or more (signed). Otherwise it
  formats `%Fs GUARDS` with the name at offset `0x21` of record `c` of the 49-byte records at
  `57E0:19C9` (FMT-COMBAT-001), shows it through `566A:002A`, and calls `5671:0093` with `c` and
  1.
- W (`0x1763`): the same with `%Fs WAITS` and `5671:0093` with `c` and 2.
- q (`0x18F4`): nothing when `c` is 4 or more, then as Q.
- Q (`0x18FE`): nothing when the byte at `57E0:143C` is 0 and `c` is 4 or more. Otherwise it calls
  the far callback at `[bp+0x1E]` when that is not null. Then, when `combat_state` is not 0, it
  calls `56BD:00CA` (FND-COMBAT-026) with x = the word at offset 3 of record `c` of the 37-byte
  records at `57E0:67BB` minus the word at `57E0:1408`, and y = the word at offset 5 of that
  record minus the word at `57E0:140A`.
- F1 (`0x17BD`): when `combat_state` is not 0, the message `CAN'T SAVE DURING COMBAT`.
- 1 to 4 (`0x15B5`): when the far pointer at `57E0:1431` is not null, they post an event with
  the value 2 in its first word, the button number `0x2C24` to `0x2C27` and the value 7 at
  `[bp+0x18]`. Space takes the same path with the button `0x2C24` plus the word at `4E71:0B44`
  and the value 8 in place of 7.

`5671:0093` and `5671:0098` are stub entries 23 and 24 of overlay 173 (header segment `5671`),
with targets at its code offsets `0x2FE9` and `0x306B`.

## Interpretation

G and W make the character whose turn it is guard or wait, the second argument 1 or 2 naming the
action, and only during combat and only for one of the four party members. Q during combat opens
the end-of-move menu at the character's position on screen. The dispatcher has no case for N, and
P is handled with O, so these two combat keys of the manual (SRC-MANUAL-1994, page 77) are not
handled here. Space here repeats a click on the selected character box with another event value,
so it does not turn off computer control in this routine.

## Alternatives

The routine is one keyboard consumer of overlay 190. Another routine, such as one that reads the
keyboard while it is a computer-controlled character's turn, could handle N, P and Space as the
manual says; no other table holding those key words was found (FND-COMBAT-001). What the event
values 7 and 8 mean to the character box, and what the handlers of the other keys do, was not
read.

## How to reproduce

Disassemble overlay 190 from its code offset at `0x139B`, read the tables at `0x1F44`, `0x1F86`
and `0x1F0C`, and disassemble each handler. Read the stub entries of the header at `5671:0020`
onward, five bytes each (`CD 3F` and a code offset).
