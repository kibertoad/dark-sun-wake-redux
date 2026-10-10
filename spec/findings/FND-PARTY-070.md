---
id: FND-PARTY-070
title: The generation window's handler enables DONE only when the character has a class and a discipline, a sphere if it is a cleric or druid, and all three disciplines if it is a psionicist, and its portrait button sets the origin and gender from one of 14 portraits and remakes the character as a rolled fighter
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006DE37..0x0006DFB9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006DFCB..0x0006E023
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E023..0x0006E0B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E0B9..0x0006E181
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006AFE0..0x0006B194
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B7AD..0x0006B8BB
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. `DS:1429` points at the generation details record and `DS:142D` at its combatant
record (FND-PARTY-063); `DS:1431` holds the generation window.

**The handler.** Overlay 184 `+0DA7` takes an event number and the 24-byte event. For event 1 it
stores 0 in `DS:1462` and returns -1 (`+0DB2..+0DCA`). For event 6 with the word `0x11B` at
`[bp + 0x12]` it calls `+1029` with the word at `4E71:0B44` and 0 (`+0DCD..+0DEC`). For event 2 it
looks up the button number among the 22 words at `+0F3B` (file `0x6DFCB`) and jumps through the
word 22 words after the match (`+0DEF..+0E0B`):

| Button | Target | Action |
| --- | --- | --- |
| `0x7D1`, `0x7EB` | `+0E0F` | overlay 183 `+0013` with the event |
| `0x7D2` | `+0E24` | overlay 183 `+026D` (the Cleric handler, FND-PARTY-066) |
| `0x7D3` | `+0E2C` | overlay 183 `+02EB` (Druid) |
| `0x7D4`, `0x7D5`, `0x7D6`, `0x7D9` | `+0E34`, `+0E3C`, `+0E44`, `+0E5C` | overlay 183 `+0A3E` with code 3, 4, 5 or 8, 0 and 1 |
| `0x7D7` | `+0E4C` | overlay 183 `+032A` (Psionicist, FND-PARTY-067) |
| `0x7D8` | `+0E54` | overlay 183 `+02AC` (Ranger) |
| `0x7DA` | `+0E6D` | overlay 183 `+10FF` |
| `0x7DB` | `+0EC3` | overlay 183 `+0542` with the event |
| `0x7DC` to `0x7E1` | `+0ED7` | overlay 183 `+05A1` with the event |
| `0x7E2` | `+0EEB` | overlay 183 `+07CD` with the event |
| `0xFA3` | `+0F02` | overlay 183 `+0871` |
| `0x477E`, `0x4B68` | `+0E75` | sets the button's state of kind 4 and then 5, and calls `+1029` with the word at `4E71:0B44` and 1 for `0x4B68`, 0 for `0x477E` |

After any event 2 whose button less `0x7D1` is 0 to 8 it calls `+0F93` (`+0F07..+0F22`, the nine
words at `+0F29` all `+0F20`). Overlay 183 calls `+0F93` too, through trampoline `56DD:0039`, at
`+1088` and `+10E4`, the ends of the sphere and discipline window handlers (FND-PARTY-066,
FND-PARTY-067).

**DONE's state.** Overlay 184 `+0F93` starts a flag at 1 and clears it when the first class byte
at `+0x1B` is 0 or `DS:42C0` is 0 (`+0F96..+0FAA`). It then goes over the class positions 0 to 2
until one holds 1 or 2, and if one does clears the flag when `DS:42C2` is 0 (`+0FAC..+0FDF`); and
over them again until one holds 6, and if one does clears the flag when `DS:42C0` is not `0xE0`
(`+0FE1..+1008`). It sets the state of button `0x4B68` in the window at `DS:1431` to 0 when the
flag is 1 and to 1 when it is 0, through `3EBE:071F` (`+100A..+1024`), the call and values
overlay 183 `+091C` uses to enable and disable the class buttons (FND-PARTY-068).

**Finishing.** Overlay 184 `+1029` takes a slot and a flag. With the flag 0 it goes to `+10F1`.
With the flag 1 it calls overlay 186 `+0572` (trampoline `56E9:0052`); when the first class byte
is 0 it stores 0 in the dword at `+0x00` and calls overlay 183 `+0A3E` with 3, 1 and 1; when
`DS:42C0` is 0 it stores `0x80` there; it copies the six bytes at `+0x19` of the combatant record
to `+0x15` of the details record, the word at `+0x08` of the details record to `+0x00` of the
combatant record, the word at `+0x0C` to `+0x02` and the byte at `+0x21` to `+0x12`; and it calls
overlay 183 `+0871`, then `+1648`, `+1876`, `+1980` and `+1CED` with the slot, and `27E5:000F`
(`+1046..+10EF`). No instruction in `+1029` or `+0F93` reads the six bytes at `+0x19` other than to
copy them, or the alignment byte at `+0x14`.

**The portrait.** Overlay 183 `+0000` returns 1 when the word at `[bp + 0x18]`, offset `0x12` of
the event, is below 8 (unsigned) and -1 otherwise (`+0003..+0011`). Overlay 183 `+0013` adds that
to the word at `+0x10` of the combatant record and wraps it to 13 below 0 and to 0 above 13
(`+002D..+0053`). It stores in the origin byte at `+0x12` the word halved, rounded down, plus 1,
or 8 when the word is 13, and in the gender byte at `+0x13` the word's lowest bit plus 1, or 1
when the word is 12 and 2 when it is 13 (`+00A2..+0104`). It then stores 0 in the three class
bytes; when `3A8E:0C2B` with `0x4C31` returns 8 it stores the result of `+0CA1` for the window at
`DS:1138` and buttons `0x7FA` to `0x7FD` in `DS:42C2` (FND-PARTY-066) and closes that window; it
calls overlay 186 `+0321` (trampoline `56E9:003E`), which opens the discipline window
(FND-PARTY-067), stores `0x80` in `DS:42C0`, 0 in `DS:42C2`, in the word at `+0x08` and in the dword at `+0x00`, 5 in the
alignment byte at `+0x14`, and 0 in `DS:42D8` and `DS:42D6`, and calls `+0A3E` with 3, 1 and 1
(`+0109..+01B1`).

**The other two buttons.** Overlay 183 `+07CD` adds the result of `+0000` to the word at `+0x08`,
wraps it to `DS:42D8` above `DS:42D6` and to `DS:42D6` below `DS:42D8`, and copies it to `+0x00`
of the combatant record (`+07E0..+082B`). Overlay 183 `+0871` edits the 15-byte text at `+0x21`
of the combatant record in button `0xFA3` and puts the string at `DS:1092` (`Default Name`) there
when the text is empty (`+0874..+08D6`).

## Interpretation

DONE is button `0x4B68`, and `0x477E` leaves the screen without keeping the character, as Escape
does. DONE can be pressed only when the character has a class and a discipline, a sphere if one of
its classes is Cleric or Druid, and all three disciplines if one is Psionicist; it checks nothing
else. Origin and gender are not chosen separately: the portrait button steps through 14
portraits, two for each of the first six origins (male, then female, as FND-PARTY-017 orders
gender), then a male mul and a female thri-kreen. Each step remakes the character as a fighter of
true neutral alignment with rolled scores. The word at `+0x08`, which `+07CD` edits between two
bounds, is the hit points (FND-PARTY-050).

## Alternatives

- What the event word at offset `0x12` holds was not read; the left and right mouse buttons
  stepping forward and back would fit.
- `+1029` with the flag 1 and no class makes the character a fighter, which DONE's state rules out;
  whether another path reaches it that way was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 184 6DE37 6E181`,
`183 6AFE0 6B194` and `183 6B7AD 6B8BB`; `direct_callers.py <dsun> 184+0F93 184+1029`;
`trampoline_target.py <dsun>` for `56CC:0089`, `56CC:00C0`, `56CC:00C5`, `56CC:00CA`,
`56CC:00CF`, `56CC:00D4`, `56CC:00D9`, `56CC:00DE`, `56CC:00ED`, `56CC:00F7` and `56CC:0061`.
Read the 22 button words at file `0x6DFCB` and the 22 target words after them, and the nine words
at file `0x6DFB9`.
