---
id: FND-PARTY-156
title: Overlay 204's combatant field getter and setter, which read and write combatant byte 0x20 as field 33, are functions 48 and 49 of script opcode 0x22
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF2A..0x0008BFCB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C487..0x0008C4F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D828..0x0008D93A
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 204 `+0169` switches on its first argument less 1 through the 53 words at
`cs:06C7` (`+017B..+0189`), and FND-PARTY-108 and FND-PARTY-122 show it is reached from the handler
of script opcode `0x22` with the script's arguments. Its second argument goes to `SI`, and its
third and fourth are the dwords at `bp+0xA` and `bp+0xE`.

Word 48 of the table is `0x01DA` and word 49 is `0x01FB`; no other word is either. The case at
`+01DA` leaves when `SI` is below 0 or 9999 or more, and otherwise calls `+2327` with `SI` and the
two dwords (`+01DA..+01F5`). The case at `+01FB` pushes the two dwords and the far pointer
`0x0660:0x00BB`, the trampoline `5787:00BB` that enters `+259D`, and joins the case at `+018E`,
which calls `+1A68` with `SI`, -1, that pointer and the dwords (`+018E..+01A9`, `+01FB..+0209`).

`+1A68` takes a target `t`, a state `s`, a far routine and two dwords, and calls the routine with a
slot and the two dwords: for `t` from 1 to 9998, with `t` when the state byte at `4F49:0C33 + 3t`
equals `s` or `s` is -1; for `t` of 0x7FFE, when `DS:13FA` is not 0, with each slot 0 to 3 whose
combatant word at `+0x06` is not 0, and otherwise with the slot in `4C13:0369`; for any other `t`,
with each slot `2D40:0D94` returns for `t`, while the state test passes, until it returns 9999.
It returns the number of calls that returned a nonzero byte (`+1A70..+1B79`).

FND-PARTY-152 reads `+2327` as returning a field of a slot's combatant record by its number, with
field 33 the byte at `+0x20`, and `+259D` as storing a byte by the same numbers.

## Interpretation

Combatant byte `+0x20`, loaded from CHAR byte `0x2A`, is read by script function 48 and written by
script function 49 of opcode `0x22` with field number 33; function 49 can write it for one object,
for every party member present, or for a group of objects. No other reader or writer through the
combatant table was found (FND-PARTY-152), so what the byte means is what the scripts that use
field 33 make of it.

## Alternatives

- Which shipped scripts call function 48 or 49 with field 33, and with what values, needs the
  script instruction layouts (Q-SCRIPT-004).
- What `2D40:0D94` returns for a target that is neither a slot number nor 0x7FFE, such as a group,
  was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `overlay_listing.py <dsun> 204 8BF2A 8BFCB` and
`overlay_listing.py <dsun> 204 8D828 8D93A`, and read the 53 words at file `0x8C487`.
