---
id: FND-COMBAT-023
title: The word at 4C10:0019 gates the status panel, saving, resting, adding characters, changing the leader and the G, W and Q keys
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4C10:0019..4C10:001B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:06E8..2C5F:0705
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4C13:0369..4C13:036B
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; Python 3.14.7 byte search
environment: null
---

## Observation

In overlay code, the segment `4C10` appears as the fixup word `0x02E0`, which the segment table
at file offset `0x4B080` resolves to `4C10`. The following instructions test the word at
`4C10:0019`:

| Where | Test | What follows |
|---|---|---|
| `2C5F:06ED`, `2C5F:06F7` | 0, then 1 | The status panel routine `2C5F:03F1` runs only when the word is neither (FND-COMBAT-007) |
| Overlay 190 offset `0x08F0` | not 0 | For a button `0x2C2D + i` whose `i` is not the word at `57E0:426D`, the message `CAN'T CHANGE LEADER IN COMBAT` through `566A:002A`, and nothing else |
| Overlay 190 offset `0x0E48` | not 0 | For the character box of an empty slot, the message `CAN'T ADD CHARS IN COMBAT` in place of the menu titled `INACTIVE CHARACTER` with `NEW`, `ADD` and `CANCEL` |
| Overlay 190 offset `0x16CC` | 0 | The G key does nothing (FND-COMBAT-025) |
| Overlay 190 offset `0x1768` | 0 | The W key does nothing |
| Overlay 190 offset `0x17C2` | not 0 | The F1 key shows `CAN'T SAVE DURING COMBAT` in place of its usual action |
| Overlay 190 offset `0x1920` | 0 | The Q key does nothing after its callback |
| Overlay 204 offset `0x0CD6` | not 0 | The rest routine shows `NO RESTING DURING COMBAT` and returns |

Overlay 190 has code at file offset `0x78340` and header segment `571F`; overlay 204 has code at
`0x8BDC0` and header segment `5787`.

The rest of the handler at offset `0x08EB` of overlay 190, reached outside combat or for the
character whose turn it is: for the button `0x2C2D + i`, it does nothing when the word at offset
`0x0E` of record `i` of the 66-byte records at `57E0:19C5` (FMT-COMBAT-002) is 0, or when byte
`0x14` of record `i` of the 49-byte records at `57E0:19C9` (FMT-COMBAT-001) is more than 1.
Otherwise it sets frame 5 on the button `0x2C2D` plus the word at `4C13:0369` through
`3EBE:071F`, stores `i` in the word at `57E0:426D`, calls `56BD:0020` with `i` and 0, sets frame
4 on the button `0x2C2D` plus the word at `4C13:0369`, and writes `%Fs IS LEADER` with the name of
record `4C13:0369` into the text box `0x2C06`.

A byte search for an immediate store (`26 C7 06 19 00`) after a load of the segment finds seven,
storing these values:

| File offset | Place | Value |
|---|---|---|
| `0x0001CE55` | resident | 0 |
| `0x00023A37` | resident | 4 |
| `0x0005CBD7` | overlay 173, offset `0x22B7` | 1 |
| `0x0007336F` | overlay 188, offset `0x04CF` | 1 |
| `0x00073BB3` | overlay 188, offset `0x0D13` | 0 |
| `0x0007E823` | overlay 193, offset `0x0533` | 1 |
| `0x0008162B` | overlay 194, offset `0x04FB` | 0 |

## Interpretation

The word is 0 outside combat and not 0 in combat. The panel is drawn only when it is neither 0
nor 1, so 1 is a state in which combat has begun or is ending without the panel, and 4 is another
combat state. Saving, resting, adding a character and changing the leader to anyone but the
character whose turn it is are refused during combat.

The buttons `0x2C2D` to `0x2C30` choose the leader. The word at `4C13:0369` is the leader's party
slot, which `56BD:0020` sets from its first argument, since the handler reads it after that call
to name the new leader; the word at `57E0:426D` is set to the same slot.

## Alternatives

Writes through a register, or with the segment loaded another way, were not searched, so the word
may take other values. Which of the values 1 and 4 marks which part of a combat is not known;
FND-COMBAT-011 lists the values of another combat word, `57E0:0DAB`.

## How to reproduce

Disassemble the listed offsets from each overlay's code offset. Search `DSUN.EXE` for
`26 C7 06 19 00` and keep the matches preceded by `B8 10 3C` (resident, before relocation) or
`B8 E0 02` (overlay), and find each match's overlay with the overlay map reporter.
