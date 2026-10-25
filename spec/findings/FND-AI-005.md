---
id: FND-AI-005
title: Byte 0x18 of a combatant record is written by the effect handlers for five effect codes, by script function 49 and field paths for field 25, and by record loads and copies; the effects set bits 5 and 6 together and the removal handler clears them together
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008268E..0x000826CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082A59..0x00082B2A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E43F..0x0008E451
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000702A8..0x000702AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E798..0x0006E79D
tool: Capstone 5.0.9 over overlay and resident code; tools/research/exec-census/overlay_listing.py, field_stores.py, block_copies.py, immediate_search.py and script_decode.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. Overlay 195's code starts at file `0x81770`,
overlay 204's at `0x8BDC0`. The combatant records are the 49-byte records at the far pointer
`DS:19C9`; a party slot's record index is `4F49:0C34 + 3 × slot`.

| Writer | What it stores in byte `+0x18` | When |
|---|---|---|
| overlay 195 `+0F59` | OR `0x60` | the application handler `+0AF9`, for effect codes 35, 102, 20, 7 and 21, when the owner slot is below 4 (`+0F1E..+0F5E`) |
| overlay 195 `+1392` | OR `0x60` | the removal handler `+1000`, owner below 4, when another of the five codes is still active and the word `4C10:0019` is not 0 (`+12E9..+13B6`) |
| overlay 195 `+13B1` | AND `0x9F` | the same tail in every other case |
| overlay 204 `+268D` | the value's low byte | script opcode `0x22` function 49 with field 25 on a kind-2 object |
| `1AA0:012A` | the value | a script field-path store to field 25 of a kind-2 object; `OBJEX.GFF` `FNFO` 1 gives field 25 the offset `0x18` and `FNFO` 2 the type 1, a byte |
| overlay 187 `+0478` | `CHAR` byte `0x22`, by a 49-byte copy | the `CHAR` load (FND-PARTY-049); the byte is 0 in all 19 shipped records |
| overlay 184 `+1708` | byte `+0x18` of the buffer at `4E4F:006B`, by a 49-byte copy | storing a generated or edited character; the buffer's byte is 0 in the file and has no direct store |
| overlay 187 `+0A6C` | the saved byte, by reading `SAVE` entry 5 into the table | loading a saved game (FND-SAVE-012) |

In the application handler, code 35 is refused before `+0F1E` when the target's byte `+0x12` in
its FMT-COMBAT-002 record is 9, and when effect 18 is active it removes effect 18 and is refused.
Code 20 shows a message for a party owner on its way to `+0F1E`. The shipped scripts call
function 49 with field 25 81 times in 47 `GPL` resources, with the literal values 255 (8 times),
192, 193, 64 and 1 and otherwise a variable, and make 117 field-path stores to field 25.

`field_stores.py` with `--es 18`, `--es 17` and `--es 15 16`, and without `--es`, finds no other
store to byte `+0x18` of a record reached from `DS:19C9`. The other byte stores at displacement
`0x18` it lists write the 37-byte object records at `DS:67BB` (`31E0:0A06`, `31E0:1854`,
`31E0:26E5`, `31E0:2758`, `31E0:280B`, `31E0:2A47`, `31E0:2A76`), the step records at `4F49:0363`
(`2D40:161F`), or are word and double-word stores in routines with no reference to `DS:19C9` in
the 1,024 bytes after their start (`3A8E:04A5`, `3A8E:0597`, `3BA6:0362`, `3BA6:03A0`,
`3CFA:0175`, `3CFA:01C7`). Of the 14 block copies `block_copies.py` lists next to a read of
`DS:19C9`, all but the two in the table copy into stack buffers or other tables. Bit 5 alone is
written by the button and Space (FND-AI-003, FND-AI-004).

## Interpretation

By FND-COMBAT-027, effect codes 7 and 102 are Berserk, 20 Controlled, 21 Confused and 35
Afraid. Each of them, applied to a party member, turns on `computer_control` and
`control_locked` together. When an active effect ends on a party member, both bits are set again
when another of the five remains and a fight is on, and cleared otherwise. That clears
`computer_control` the player set with the button too, whenever any active effect of that member
ends. Scripts can set the whole byte, and so both bits, for any combatant.

## Alternatives

The removal handler's callers (overlay 193 `+1873` and the other removal paths) and the
application wrapper (overlay 193 `+166F`) were not read here; FND-COMBAT-031, FND-COMBAT-032 and
FND-COMBAT-053 place them. `1AA0:012A` was not disassembled here; that field 25 reaches `+0x18`
rests on FND-PARTY-051 and the `FNFO` tables. Which objects the 81 calls and 117 stores name was
not read. A writer that loads a combatant pointer more than six instructions before the store,
or keeps it in memory, escapes the census, as would one of the six excluded resident stores if
its routine received a combatant pointer as an argument.

## How to reproduce

`overlay_listing.py <dsun> 195 82269 82724` and `... 195 82770 82B2A`; `... 204 8E35D 8E5A1`;
`... 187 70290 702C0`; `... 184 6E6D8 6E7A0`. `field_stores.py <dsun> 18 --es 18`, `... 17 --es
17`, `... 15 16 --es 15 16` and `... 18 17 16`; `block_copies.py <dsun>` with context 24;
`immediate_search.py <dsun> 19C9 19CB`; `script_decode.py --all` over `GPLDATA.GFF`, searched for
function 49 and for path stores with field 25. `OBJEX.GFF` resources `FNFO` 1 and 2.
