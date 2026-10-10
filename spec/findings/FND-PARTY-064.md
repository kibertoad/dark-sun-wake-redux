---
id: FND-PARTY-064
title: Three of the four callers of overlay 184 +1DCE pass the records of the slot in the word at 4E71:0B44, the slot +1DCE takes the class bytes from, and the fourth, the DUAL path of overlay 209 +123E, passes the slot that word held when the routine began
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007930C..0x00079349
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079479..0x000794BD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E696..0x0006E6D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D394..0x0006D3C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000985D5..0x00098608
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009463D..0x00094664
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 184 `+1DCE` copies the details and combatant records it is given to
`4E4F:0029` and `4E4F:006B`, and takes the class bytes from the details record numbered by the
word at `4E71:0B44` (FND-PARTY-063). `direct_callers.py` finds four calls of it:

- Overlay 184 `+1639`, in `+1606`. Overlay 190 `+0FFA..+1004` pushes the word at `4E71:0B44` and
  calls `+1606` through trampoline `56DD:0048`, the only call of `+1606` found, when the byte at
  `+0x14` of that slot's combatant record is 0. `+1606` stores its argument in `DS:112E`, 0 in
  `DS:42C0`, and passes the details and combatant records numbered by its argument to `+1DCE`
  (`+160D..+1639`), with no other call or store between.
- Overlay 184 `+0334` and overlay 212 `+0403` push the combatant and details records numbered by
  the word at `4E71:0B44` read just before the call (`+0304..+0331`, `+03D5..+0402`).
- Overlay 209 `+14FF`, in the routine at `+123E` (trampoline `57B0:0043`), pushes the records
  numbered by its own argument (`+14DD..+14FE`). Overlay 190 `+1178` is the only call of that
  routine found; it pushes the word at `4E71:0B44` and runs when the slot's details record has a
  level byte at `+0x1E` of 2 or more and origin byte 1, the case for which overlay 190 offers the
  label `DUAL` (`+0F19..+0F4C`, `+1139..+1178`). Between its entry and `+14FF` the routine makes
  far calls into overlays 182, 183, 186, 190 and 212 and the resident code, among them overlay 212
  `+0020` at `+14D5`.

A search of the file for an ES-overridden store to offset `0x0B44` (`mov` from any register,
`mov` of an immediate, `add`, `sub`, `inc`, `dec`, `pop`) finds 11, at file `0x58342`,
`0x6D874`, `0x75CBB`, `0x78933`, `0x7BA2D`, `0x8A743`, `0x93386`, `0x93BF2`, `0x9316F`,
`0x9614A` and `0x98828`. The last is overlay 212 `+0628`, at the start of the routine at `+061C`,
which stores its argument at `[bp + 0x0E]` there.

## Interpretation

On the edit path of overlay 190 (`+1606`) and on the calls from overlay 184 `+0334` and overlay
212 `+0403`, the class bytes `+1DCE` reads are those of the character whose records it copies, so
FND-PARTY-063's concern does not arise there. On the DUAL path they are the same only if nothing
the routine at overlay 209 `+123E` calls before `+14FF` stores another slot in the word.

## Alternatives

FND-PARTY-063 left open that `+1DCE` reads the slot of the last call of `+07D8` on the edit path,
since `+07D8` sets the word only after it. Overlay 190 passes the word itself as the slot, so the
two are the same there. Which of the 11 stores the DUAL path can reach was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `direct_callers.py <dsun> 184+1DCE 184+1606
209+123E`; `overlay_listing.py <dsun> 190 79190 79350`, `190 79479 794C0`, `184 6E696 6E6D8`,
`184 6D390 6D3CC`, `212 985C8 9860C`, `212 9881C 98830` and `209 9439E 94668`. Search the file
for the byte 0x26 followed by an opcode and ModRM byte of the listed stores with a direct address
and then the bytes `44 0B`.
